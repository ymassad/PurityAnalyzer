using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PurityAnalyzer
{
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
}