using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PurityAnalyzer
{
    public static class Utils
    {
        public static bool IsReturnsNewObjectAttribute(AttributeSyntax attribute)
        {
            return attribute.Name is IdentifierNameSyntax name && IsReturnsNewObjectAttribute(name.Identifier.Text);
        }

        public static bool IsReturnsNewObjectAttribute(string attributeName)
        {
            return attributeName == "ReturnsNewObject" || attributeName == "ReturnsNewObject" + "Attribute";
        }

        public static bool IsIsPureAttribute(AttributeSyntax attribute)
        {
            return attribute.Name is IdentifierNameSyntax name && IsIsPureAttribute(name.Identifier.Text);
        }

        public static bool IsIsPureExceptLocallyAttribute(AttributeSyntax attribute)
        {
            return attribute.Name is IdentifierNameSyntax name && IsIsPureExceptLocallyAttribute(name.Identifier.Text);
        }

        public static bool IsIsPureAttribute(string attributeName)
        {
            return attributeName == "IsPure" || attributeName == "IsPure" + "Attribute";
        }

        public static bool IsIsPureExceptLocallyAttribute(string attributeName)
        {
            return attributeName == "IsPureExceptLocally" || attributeName == "IsPureExceptLocally" + "Attribute";
        }

        public static (SyntaxNode node, string message)[] GetImpurities(SyntaxNode methodDeclaration, SemanticModel semanticModel, bool exceptLocally = false)
        {
            var vis = new Visitor(semanticModel, IsIsPureAttribute, exceptLocally);

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

        public static bool IsNewlyCreatedObject(SemanticModel semanticModel, ExpressionSyntax expression)
        {
            if (expression is ObjectCreationExpressionSyntax)
                return true;

            if (expression is InvocationExpressionSyntax invocationExpression)
            {
                if (semanticModel.GetSymbolInfo(invocationExpression.Expression).Symbol is IMethodSymbol invokedMethod)
                {
                    if (invokedMethod.IsInCode())
                    {

                        var location = invokedMethod.Locations.First();

                        var locationSourceTree = location.SourceTree;

                        var node = locationSourceTree.GetRoot().FindNode(location.SourceSpan);

                        if (node is BaseMethodDeclarationSyntax methodNode)
                        {
                            if (ReturnsNewObject(methodNode, semanticModel))
                                return true;
                        }
                    }
                }
            }

            if (!(expression is IdentifierNameSyntax identifier))
                return false;

            var identifierSymbol = semanticModel.GetSymbolInfo(identifier).Symbol;

            if (!(identifierSymbol is ILocalSymbol local))
                return false;

            var method = expression.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrNoValue();

            if (method.HasNoValue)
                return false;

            List<ExpressionSyntax> FindValuesAssignedToVariable(SyntaxNode containingBlockNode, ILocalSymbol local1)
            {
                var declaration = local1.DeclaringSyntaxReferences.Single();

                List<ExpressionSyntax> list = new List<ExpressionSyntax>();

                var declarationNode = declaration.SyntaxTree.GetRoot().FindNode(declaration.Span);

                if (declarationNode is VariableDeclaratorSyntax variableDecl)
                {
                    if (variableDecl.Initializer != null)
                    {
                        list.Add(variableDecl.Initializer.Value);
                    }
                }

                var usages = containingBlockNode
                    .DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Where(x => x.Identifier.Text == local1.Name)
                    .Where(x => semanticModel.GetSymbolInfo(x).Symbol?.Equals(local1) ?? false)
                    .Where(x => x.Parent is AssignmentExpressionSyntax assignment && assignment.Left == x)
                    .Select(x => ((AssignmentExpressionSyntax) x.Parent).Right)
                    .ToArray();

                list.AddRange(usages);

                return list;
            }
            
            return FindValuesAssignedToVariable(method.GetValue().Body, local).All(x => IsNewlyCreatedObject(semanticModel, x));
        }

        public static bool ReturnsNewObject(BaseMethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
        {
            return !GetNonNewObjectReturnsForMethod(methodDeclaration, semanticModel).Any();
        }

        public static IEnumerable<ExpressionSyntax> GetNonNewObjectReturnsForMethod(BaseMethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
        {
            var returnExpressions =
                methodDeclaration.Body != null
                    ? methodDeclaration.Body
                        .DescendantNodes()
                        .OfType<ReturnStatementSyntax>()
                        .Select(x => x.Expression)
                        .ToArray()
                    : new[] { methodDeclaration.ExpressionBody.Expression };

            foreach (var expression in returnExpressions)
            {
                if (!Utils.IsNewlyCreatedObject(semanticModel, expression))
                {
                    yield return expression;
                }
            }
        }
    }
}