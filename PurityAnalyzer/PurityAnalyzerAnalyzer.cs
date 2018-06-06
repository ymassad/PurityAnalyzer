using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;

namespace PurityAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PurityAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string ReadDiagnosticId = "ReadPurityAnalyzer";



        private const string Category = "Purity";


        private static DiagnosticDescriptor ImpurityRule =
            new DiagnosticDescriptor(
                ReadDiagnosticId,
                "Impure read operation",
                "Impure read operation",
                Category,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: "Impure read descruption");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ImpurityRule);

        public override void Initialize(AnalysisContext context)
        {

            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax) context.Node;

            

            if (methodDeclaration.AttributeLists.SelectMany(x => x.Attributes).Select(x => x.Name)
                .OfType<IdentifierNameSyntax>().Any(x => Utils.IsIsPureAttribute(x.Identifier.Text)))
            {
                var impurities = Utils.GetImpurities(methodDeclaration, context.SemanticModel);

                foreach(var impurity in impurities)
                {


                    var diagnostic = Diagnostic.Create(
                        ImpurityRule,
                        impurity.node.GetLocation());

                    context.ReportDiagnostic(diagnostic);
                    

                }
            }

            int a = 0;
        }
    }


    public static class Utils
    {
        public static bool IsIsPureAttribute(string attributeName)
        {
            return attributeName == "IsPure" || attributeName == "IsPure" + "Attribute";
        }

        public static (SyntaxNode node, string message)[] GetImpurities(SyntaxNode methodDeclaration, SemanticModel semanticModel)
        {
            var vis = new Visitor(semanticModel, IsIsPureAttribute);

            vis.Visit(methodDeclaration);

            return vis.impurities.ToArray();
        }
    }


    [CreateMatchMethods(typeof(IdentifierUsage))]
    public static class ExtensionMethods
    {
       

        public static TResult Match<TResult>(this IdentifierUsage instance, Func<IdentifierUsage.ReadFromAndWrittenToCase, TResult> readFromAndWrittenToCaseCase, Func<IdentifierUsage.WrittenToCase, TResult> writtenToCaseCase, Func<IdentifierUsage.ReadFromCase, TResult> readFromCaseCase)
        {
            if (instance is IdentifierUsage.ReadFromAndWrittenToCase readFromAndWrittenToCase)
                return readFromAndWrittenToCaseCase(readFromAndWrittenToCase);
            if (instance is IdentifierUsage.WrittenToCase writtenToCase)
                return writtenToCaseCase(writtenToCase);
            if (instance is IdentifierUsage.ReadFromCase readFromCase)
                return readFromCaseCase(readFromCase);
            throw new Exception("Invalid IdentifierUsage type");
        }

        public static void Match(this IdentifierUsage instance, Action<IdentifierUsage.ReadFromAndWrittenToCase> readFromAndWrittenToCaseCase, Action<IdentifierUsage.WrittenToCase> writtenToCaseCase, Action<IdentifierUsage.ReadFromCase> readFromCaseCase)
        {
            if (instance is IdentifierUsage.ReadFromAndWrittenToCase readFromAndWrittenToCase)
            {
                readFromAndWrittenToCaseCase(readFromAndWrittenToCase);
                return;
            }

            if (instance is IdentifierUsage.WrittenToCase writtenToCase)
            {
                writtenToCaseCase(writtenToCase);
                return;
            }

            if (instance is IdentifierUsage.ReadFromCase readFromCase)
            {
                readFromCaseCase(readFromCase);
                return;
            }

            throw new Exception("Invalid IdentifierUsage type");
        }

        public static bool IsInCode(this ISymbol symbol)
        {
            return symbol.Locations.All(x => x.IsInSource);
        }
    }

    public class CreateMatchMethodsAttribute : Attribute
    {
        public Type[] Types { get; }

        public CreateMatchMethodsAttribute(params Type[] types)
        {
            Types = types;
        }
    }

    public abstract class IdentifierUsage
    {
        private IdentifierUsage()
        {
        }

        public class ReadFromCase : IdentifierUsage
        {

        }

        public class WrittenToCase : IdentifierUsage
        {

        }

        public class ReadFromAndWrittenToCase : IdentifierUsage
        {

        }

        public static IdentifierUsage ReadFrom() => new ReadFromCase();
        public static IdentifierUsage WrittenTo() => new WrittenToCase();
        public static IdentifierUsage ReadFromAndWrittenTo() => new ReadFromAndWrittenToCase();
    }

    public class Visitor : CSharpSyntaxWalker
    {

        public List<(SyntaxNode node, string message)> impurities = new List<(SyntaxNode node, string message)>();

        private readonly Func<string, bool> isIsPureAttribute;

        private readonly SemanticModel semanticModel;

        public Visitor(SemanticModel semanticModel, Func<string, bool> isIsPureAttribute)
        {
            this.semanticModel = semanticModel;
            this.isIsPureAttribute = isIsPureAttribute;
        }




        private IdentifierUsage GetUsage(IdentifierNameSyntax identifier)
        {
            if (identifier.Parent is MemberAccessExpressionSyntax memberAccess)
            {
                return GetUsageForOperation(memberAccess.Parent);
            }

            if (identifier.Parent is ElementAccessExpressionSyntax elementAccess)
            {
                return GetUsageForOperation(elementAccess.Parent);
            }

            IdentifierUsage GetUsageForOperation(SyntaxNode operationNode)
            {
                if (operationNode is AssignmentExpressionSyntax assignent)
                {
                    switch (assignent.Kind())
                    {
                        case SyntaxKind.SimpleAssignmentExpression:
                            return IdentifierUsage.WrittenTo();
                        case SyntaxKind.AddAssignmentExpression:
                        case SyntaxKind.SubtractAssignmentExpression:
                            //TODO: add more assignment expressions
                            return IdentifierUsage.ReadFromAndWrittenTo();

                    }
                }
                else if (operationNode is PostfixUnaryExpressionSyntax postfix)
                {
                    return IdentifierUsage.ReadFromAndWrittenTo();
                }

                return IdentifierUsage.ReadFrom();
            }

            return GetUsageForOperation(identifier.Parent);
        }


        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbol = semanticModel.GetSymbolInfo(node);

            if (symbol.Symbol is IFieldSymbol field)
            {
                if (!SymbolHasIsPureAttribute(field))
                {
                    if (!(field.IsReadOnly || field.IsConst))
                    {
                        impurities.Add((node, "Non const/readonly field is accessed"));
                    }
                }
            }

            if (symbol.Symbol is IPropertySymbol property)
            {
                if (!SymbolHasIsPureAttribute(property) && !SymbolHasIsPureAttribute(property.ContainingSymbol))
                {
                    if (!property.IsInCode())
                    {
                        if (!property.IsReadOnly || !IsKnownPureMethod(property.GetMethod))
                        {
                            impurities.Add((node, "Property access on type that is not in code and that does not have the Pure attribute"));
                        }
                    }
                    else if (!property.IsReadOnly)
                    {

                        impurities.Add((node, "Non read only property access"));
                    }
                    else
                    {
                        var localtion = property.GetMethod.Locations.First();

                        var getNode = localtion.SourceTree.GetRoot().FindNode(localtion.SourceSpan);

                        var imp = Utils.GetImpurities(getNode, semanticModel);

                        if(imp.Any())
                            impurities.Add((node, "Property getter is impure"));
                    }
                }
            }

            if (symbol.Symbol is IParameterSymbol parameter)
            {
                if (parameter.Type.TypeKind == TypeKind.Array)
                {
                    GetUsage(node).Match(
                        readFromCaseCase: _ => { },
                        writtenToCaseCase: _ => impurities.Add((node, "error")),
                        readFromAndWrittenToCaseCase: _ =>
                        {
                            impurities.Add((node, "error"));
                        });
                }
            }

            if (symbol.Symbol is IMethodSymbol method)
            {
                if (!SymbolHasIsPureAttribute(method))
                {
                    if (method.IsInCode())
                    {
                        var localtion = method.Locations.First();

                        var methodNode = localtion.SourceTree.GetRoot().FindNode(localtion.SourceSpan);

                        var imp = Utils.GetImpurities(methodNode, semanticModel);

                        if (imp.Any())
                            impurities.Add((node, "Method is impure"));
                    }
                    else
                    {
                        if(!IsKnownPureMethod(method))
                            impurities.Add((node, "Method is impure"));
                    }
                }



            }

            base.VisitIdentifierName(node);
        }

        private bool IsKnownPureMethod(IMethodSymbol method)
        {
            var inttype = semanticModel.Compilation.GetTypeByMetadataName(typeof(int).FullName);

            var booltype = semanticModel.Compilation.GetTypeByMetadataName(typeof(bool).FullName);

            var enumerableType = semanticModel.Compilation.GetTypeByMetadataName(typeof(Enumerable).FullName);

            var iGroupingType = semanticModel.Compilation.GetTypeByMetadataName(typeof(IGrouping<,>).FullName);

            if (method.ContainingType.Equals(inttype))
            {
                if (method.Name == "ToString")
                    return true;
            }

            if (method.ContainingType.Equals(booltype))
            {
                if (method.Name == "ToString")
                    return true;
            }

            if (method.ContainingType.Equals(enumerableType))
            {
                return true;
            }


            if (method.ContainingType.IsGenericType &&
                method.ContainingType.ConstructedFrom.Equals(iGroupingType))
            {
                return true;
            }

            return false;
        }

        private bool SymbolHasIsPureAttribute(ISymbol symbol)
        {
            return symbol.GetAttributes().Any(x => isIsPureAttribute(x.AttributeClass.Name));
        }
    }

    public static class OperationExtensions
    {
        public static bool IsWrite(this IOperation operation)
        {
            switch (operation.Kind)
            {
                case OperationKind.SimpleAssignment:
                case OperationKind.Increment:
                case OperationKind.Decrement:
                case OperationKind.CompoundAssignment:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsRead(this IOperation operation)
        {
            switch (operation.Kind)
            {
                case OperationKind.SimpleAssignment:
                    return false;
                default:
                    return true;
            }
        }
    }
}
