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

        private static DiagnosticDescriptor ReturnsNewObjectRule =
            new DiagnosticDescriptor(
                ReadDiagnosticId,
                "Returns new object error",
                "{0}",
                Category,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: "Returns new object error");

        public static Maybe<string> CustomPureTypesFilename { get; set; }
        public static Maybe<string> CustomPureMethodsFilename { get; set; }

        public static Maybe<string> CustomPureExceptLocallyMethodsFilename { get; set; }
        public static Maybe<string> CustomReturnsNewObjectMethodsFilename { get; set; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ImpurityRule, ReturnsNewObjectRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeMethodSyntaxNode, SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodSyntaxNode, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodSyntaxNode, SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeClassSyntaxNode, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertySyntaxNode, SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeMethodSyntaxNode(SyntaxNodeAnalysisContext context)
        {

            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods =
                Utils.GetKnownReturnsNewObjectMethods(context.SemanticModel);

            var methodDeclaration = (BaseMethodDeclarationSyntax) context.Node;

            var attributes = GetAttributes(methodDeclaration.AttributeLists);

            if (attributes.Any(Utils.IsIsPureAttribute))
            {
                ProcessImpuritiesForMethod(context, methodDeclaration, knownReturnsNewObjectMethods);
                return;
            }

            attributes.FirstOrNoValue(Utils.IsIsPureExceptLocallyAttribute).ExecuteIfHasValue(attribute =>
            {
                if (methodDeclaration.IsStatic())
                {
                    var diagnostic = Diagnostic.Create(
                        ImpurityRule,
                        attribute.GetLocation(),
                        "IsPureExceptLocallyAttribute cannot be applied on static methods");

                    context.ReportDiagnostic(diagnostic);
                    return;
                }

                ProcessImpuritiesForMethod(context, methodDeclaration, knownReturnsNewObjectMethods, exceptLocally: true);
            });

            attributes.FirstOrNoValue(Utils.IsReturnsNewObjectAttribute).ExecuteIfHasValue(attribute =>
            {
                var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);

                if (methodSymbol != null)
                {
                    if (methodSymbol.ReturnType.IsValueType)
                    {
                        var diagnostic = Diagnostic.Create(
                            ReturnsNewObjectRule,
                            attribute.GetLocation(),
                            "ReturnsNewObjectAttribute cannot be applied on methods that return value types");

                        context.ReportDiagnostic(diagnostic);
                        return;
                    }

                    ProcessNonNewObjectReturnsForMethod(context, methodDeclaration, knownReturnsNewObjectMethods);

                }
            });

        }

        private void ProcessNonNewObjectReturnsForMethod(
            SyntaxNodeAnalysisContext context,
            BaseMethodDeclarationSyntax methodDeclaration,
            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods)
        {
            foreach (var expression in Utils.GetNonNewObjectReturnsForMethod(methodDeclaration, context.SemanticModel, knownReturnsNewObjectMethods))
            {
                var diagnostic = Diagnostic.Create(
                    ReturnsNewObjectRule,
                    expression.Parent.GetLocation(),
                    "non-new object return");

                context.ReportDiagnostic(diagnostic);
            }
        }


        private static AttributeSyntax[] GetAttributes(SyntaxList<AttributeListSyntax> methodDeclarationAttributeLists)
        {
            return methodDeclarationAttributeLists.SelectMany(x => x.Attributes).ToArray();
        }


        private void AnalyzeClassSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods =
                Utils.GetKnownReturnsNewObjectMethods(context.SemanticModel);

            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            if (classDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes).Select(x => x.Name)
                .OfType<IdentifierNameSyntax>().Any(x => Utils.IsIsPureAttribute(x.Identifier.Text)))
            {
                foreach (var methodDeclaration in
                    classDeclarationSyntax.Members
                        .OfType<MethodDeclarationSyntax>()
                        .Cast<MemberDeclarationSyntax>()
                        .Concat(classDeclarationSyntax.Members.OfType<ConstructorDeclarationSyntax>()))
                {
                    ProcessImpuritiesForMethod(context, methodDeclaration, knownReturnsNewObjectMethods);
                }

                foreach (var propertyDeclaration in classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>())
                {
                    ProcessImpuritiesForProperty(context, propertyDeclaration, knownReturnsNewObjectMethods);
                }

                foreach (var fieldDeclaration in classDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>())
                {
                    foreach (var fieldVar in fieldDeclaration.Declaration.Variables)
                    {
                        if (fieldVar.Initializer != null)
                        {
                            var initializedTo = fieldVar.Initializer.Value;

                            ProcessImpuritiesForMethod(context, initializedTo, knownReturnsNewObjectMethods);
                        }
                    }
                }
            }
        }

        private void AnalyzePropertySyntaxNode(
            SyntaxNodeAnalysisContext context)
        {
            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods =
                Utils.GetKnownReturnsNewObjectMethods(context.SemanticModel);

            var propertyDeclarationSyntax = (PropertyDeclarationSyntax)context.Node;

            if (propertyDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes).Select(x => x.Name)
                .OfType<IdentifierNameSyntax>().Any(x => Utils.IsIsPureAttribute(x.Identifier.Text)))
            {
                ProcessImpuritiesForProperty(context, propertyDeclarationSyntax, knownReturnsNewObjectMethods);
            }
        }

        private static void ProcessImpuritiesForProperty(
            SyntaxNodeAnalysisContext context,
            PropertyDeclarationSyntax propertyDeclarationSyntax,
            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods)
        {
            if (propertyDeclarationSyntax.AccessorList != null)
            {
                foreach (var accessor in propertyDeclarationSyntax.AccessorList.Accessors)
                {
                    ProcessImpuritiesForMethod(context, accessor, knownReturnsNewObjectMethods);
                }
            }
            else if (propertyDeclarationSyntax.ExpressionBody != null)
            {
                ProcessImpuritiesForMethod(context, propertyDeclarationSyntax.ExpressionBody, knownReturnsNewObjectMethods);
            }

            if (propertyDeclarationSyntax.Initializer != null)
            {
                var initializedTo = propertyDeclarationSyntax.Initializer.Value;

                ProcessImpuritiesForMethod(context, initializedTo, knownReturnsNewObjectMethods);
            }

        }

        private static void ProcessImpuritiesForMethod(
            SyntaxNodeAnalysisContext context,
            SyntaxNode methodLikeNode,
            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods,
            bool exceptLocally = false)
        {
            var impurities =
                Utils.GetImpurities(
                    GetBodyIfDeclaration(methodLikeNode),
                    context.SemanticModel,
                    knownReturnsNewObjectMethods,
                    exceptLocally)
                    .ToList();

            if (methodLikeNode is ConstructorDeclarationSyntax constructor)
            {
                var containingType = constructor.FirstAncestorOrSelf<TypeDeclarationSyntax>();

                if (Utils.AnyImpureFieldInitializer(containingType, context.SemanticModel, knownReturnsNewObjectMethods, constructor.IsStatic()))
                    impurities.Add((methodLikeNode, "There are impure field initializers"));

                if (Utils.AnyImpurePropertyInitializer(containingType, context.SemanticModel, knownReturnsNewObjectMethods, constructor.IsStatic()))
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

        public static SyntaxNode GetBodyIfDeclaration(SyntaxNode method)
        {
            if (method is BaseMethodDeclarationSyntax declarationSyntax)
                return GetMethodBody(declarationSyntax);

            return method;
        }

        public static SyntaxNode GetMethodBody(BaseMethodDeclarationSyntax method)
        {
            return method.Body ?? (SyntaxNode)method.ExpressionBody;
        }
    }
}
