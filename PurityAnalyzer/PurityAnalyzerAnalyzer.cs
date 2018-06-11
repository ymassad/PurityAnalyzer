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
                foreach (var methodDeclaration in
                    classDeclarationSyntax.Members
                        .OfType<MethodDeclarationSyntax>()
                        .Cast<MemberDeclarationSyntax>()
                        .Concat(classDeclarationSyntax.Members.OfType<ConstructorDeclarationSyntax>()))
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
}
