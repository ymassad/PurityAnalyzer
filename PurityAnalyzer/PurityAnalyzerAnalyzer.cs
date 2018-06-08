using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
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
                "Impurity error",
                "{0}",
                Category,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: "Impurity error");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ImpurityRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeMethodSyntaxNode, SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodSyntaxNode, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeClassSyntaxNode, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertySyntaxNode, SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeMethodSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (BaseMethodDeclarationSyntax) context.Node;

            if (methodDeclaration.AttributeLists.SelectMany(x => x.Attributes).Select(x => x.Name)
                .OfType<IdentifierNameSyntax>().Any(x => Utils.IsIsPureAttribute(x.Identifier.Text)))
            {
                ProcessImpuritiesForMethod(context, methodDeclaration);
            }
        }


        private void AnalyzeClassSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            if (classDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes).Select(x => x.Name)
                .OfType<IdentifierNameSyntax>().Any(x => Utils.IsIsPureAttribute(x.Identifier.Text)))
            {
                foreach (var methodDeclaration in classDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>())
                {
                    ProcessImpuritiesForMethod(context, methodDeclaration);
                }

                foreach (var propertyDeclaration in classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>())
                {
                    ProcessImpuritiesForProperty(context, propertyDeclaration);
                }

                foreach (var fieldDeclaration in classDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>())
                {
                    foreach (var fieldVar in fieldDeclaration.Declaration.Variables)
                    {
                        var initializedTo = fieldVar.Initializer.Value;

                        ProcessImpuritiesForMethod(context, initializedTo);
                    }
                }
            }
        }

        private void AnalyzePropertySyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclarationSyntax = (PropertyDeclarationSyntax)context.Node;

            if (propertyDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes).Select(x => x.Name)
                .OfType<IdentifierNameSyntax>().Any(x => Utils.IsIsPureAttribute(x.Identifier.Text)))
            {
                ProcessImpuritiesForProperty(context, propertyDeclarationSyntax);
            }
        }

        private static void ProcessImpuritiesForProperty(SyntaxNodeAnalysisContext context,
            PropertyDeclarationSyntax propertyDeclarationSyntax)
        {
            if (propertyDeclarationSyntax.AccessorList != null)
            {
                foreach (var accessor in propertyDeclarationSyntax.AccessorList.Accessors)
                {
                    ProcessImpuritiesForMethod(context, accessor);
                }
            }
            else if (propertyDeclarationSyntax.ExpressionBody != null)
            {
                ProcessImpuritiesForMethod(context, propertyDeclarationSyntax.ExpressionBody);
            }

            if (propertyDeclarationSyntax.Initializer != null)
            {
                var initializedTo = propertyDeclarationSyntax.Initializer.Value;

                ProcessImpuritiesForMethod(context, initializedTo);
            }

        }

        private static void ProcessImpuritiesForMethod(
            SyntaxNodeAnalysisContext context,
            SyntaxNode methodLikeNode)
        {
            var impurities = Utils.GetImpurities(methodLikeNode, context.SemanticModel).ToList();

            if (methodLikeNode is ConstructorDeclarationSyntax constructor)
            {
                var containingType = constructor.FirstAncestorOrSelf<TypeDeclarationSyntax>();

                if (Utils.AnyImpureFieldInitializer(containingType, context.SemanticModel, constructor.IsStatic()))
                    impurities.Add((methodLikeNode, "There are impure field initializers"));

                if (Utils.AnyImpurePropertyInitializer(containingType, context.SemanticModel, constructor.IsStatic()))
                    impurities.Add((methodLikeNode, "There are impure property initializers"));
            }

            foreach (var impurity in impurities)
            {
                var diagnostic = Diagnostic.Create(
                    ImpurityRule,
                    impurity.node.GetLocation(),
                    impurity.message);

                context.ReportDiagnostic(diagnostic);
            }
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

        public static bool AnyImpurePropertyInitializer(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel, bool onlyStaticFields = false)
        {
            var props = typeDeclaration
                .Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(x => !onlyStaticFields || x.IsStatic())
                .ToArray();

            foreach (var var in props.Select(x => x.Initializer).Where(i => i != null))
            {
                if (Utils.GetImpurities(var, semanticModel).Any()) return true;
            }

            return false;
        }

        public static bool AnyImpureFieldInitializer(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel, bool onlyStaticFields = false)
        {
            var fields =
                typeDeclaration.Members
                    .OfType<FieldDeclarationSyntax>()
                    .Where(x => !onlyStaticFields || x.IsStatic())
                    .ToArray();

            foreach (var var in fields.SelectMany(x => x.Declaration.Variables))
            {
                if (Utils.GetImpurities(var, semanticModel).Any()) return true;
            }

            return false;
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

        public static bool IsStatic(this BaseMethodDeclarationSyntax method)
        {
            return method.Modifiers.Any(SyntaxKind.StaticKeyword);
        }

        public static bool IsStatic(this PropertyDeclarationSyntax prop)
        {
            return prop.Modifiers.Any(SyntaxKind.StaticKeyword);
        }

        public static bool IsStatic(this FieldDeclarationSyntax field)
        {
            return field.Modifiers.Any(SyntaxKind.StaticKeyword);
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


        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var symbol = semanticModel.GetSymbolInfo(node.Type).Symbol as INamedTypeSymbol;

            if (symbol != null)
            {
                if (!IsTypePureForConstruction(symbol))
                {
                    impurities.Add((node, "Constructed object is not pure"));
                }
            }

            base.VisitObjectCreationExpression(node);
        }

        private bool IsTypePureForConstruction(INamedTypeSymbol symbol)
        {
            if (SymbolHasAssumeIsPureAttribute(symbol))
                return true;

            var allInterfaceMethods =
                symbol.AllInterfaces.SelectMany(i => i.GetMembers().OfType<IMethodSymbol>());


            var interfaceImplementationMethods =
                new HashSet<ISymbol>(
                    allInterfaceMethods.Select(x => symbol.FindImplementationForInterfaceMember(x)));

            if (!
                symbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(x =>
                        x.MethodKind == MethodKind.Constructor ||
                        x.MethodKind == MethodKind.StaticConstructor ||
                        x.IsOverride ||
                        interfaceImplementationMethods.Contains(x))
                    .All(IsMethodPure))
                return false;

            if (symbol.IsInCode())
            {
                if (AnyImpureFieldInitializer(symbol))
                    return false;

                if (AnyImpurePropertyInitializer(symbol))
                    return false;
            }

            return true;
        }

        private bool AnyImpurePropertyInitializer(INamedTypeSymbol symbol)
        {
            return
                symbol.Locations
                    .Select(x => x.SourceTree.GetRoot().FindNode(x.SourceSpan))
                    .OfType<TypeDeclarationSyntax>()
                    .Any(x => Utils.AnyImpurePropertyInitializer(x, semanticModel));
        }

        private bool AnyImpureFieldInitializer(INamedTypeSymbol symbol)
        {
            return
                symbol.Locations.Select(x => x.SourceTree.GetRoot().FindNode(x.SourceSpan))
                .OfType<TypeDeclarationSyntax>()
                .Any(x => Utils.AnyImpureFieldInitializer(x, semanticModel));
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
                        writtenToCaseCase: _ => impurities.Add((node, "Writing to an array")),
                        readFromAndWrittenToCaseCase: _ =>
                        {
                            impurities.Add((node, "Writing to an array"));
                        });
                }
            }

            if (symbol.Symbol is IMethodSymbol method)
            {
                if (!IsMethodPure(method))
                {
                    impurities.Add((node, "Method is impure"));
                }
            }

            base.VisitIdentifierName(node);
        }

        private bool IsMethodPure(IMethodSymbol method)
        {
            if (SymbolHasAssumeIsPureAttribute(method))
                return true;

            if (SymbolHasAssumeIsPureAttribute(method.ContainingType))
                return true;

            if (!SymbolHasIsPureAttribute(method))
            {
                if (method.IsInCode())
                {
                    if (method.IsImplicitlyDeclared)
                        return true;

                    var localtion = method.Locations.First();

                    var methodNode = localtion.SourceTree.GetRoot().FindNode(localtion.SourceSpan);

                    var imp = Utils.GetImpurities(methodNode, semanticModel);

                    if (imp.Any()) return false;
                }
                else
                {
                    if (!IsKnownPureMethod(method)) return false;
                }
            }

            return true;
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

            if (method.ContainingType.TypeKind == TypeKind.Delegate)
                return true;

            return false;
        }

        private bool SymbolHasIsPureAttribute(ISymbol symbol)
        {
            return symbol.GetAttributes().Any(x => isIsPureAttribute(x.AttributeClass.Name));
        }

        private bool SymbolHasAssumeIsPureAttribute(ISymbol symbol)
        {
            return symbol.GetAttributes().Any(x => x.AttributeClass.Name == "AssumeIsPureAttribute");
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
