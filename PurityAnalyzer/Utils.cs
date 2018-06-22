using System;
using System.Collections.Generic;
using System.IO;
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

        public static (SyntaxNode node, string message)[] GetImpurities(SyntaxNode methodDeclaration,
            SemanticModel semanticModel,
            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods,
            bool exceptLocally = false)
        {
            var vis = new Visitor(semanticModel, IsIsPureAttribute, exceptLocally, knownReturnsNewObjectMethods);

            vis.Visit(methodDeclaration);

            return vis.impurities.ToArray();
        }

        public static bool AnyImpurePropertyInitializer(
            TypeDeclarationSyntax typeDeclaration,
            SemanticModel semanticModel,
            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods,
            bool onlyStaticFields = false)
        {
            var props = typeDeclaration
                .Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(x => !onlyStaticFields || x.IsStatic())
                .ToArray();

            foreach (var var in props.Select(x => x.Initializer).Where(i => i != null))
            {
                if (Utils.GetImpurities(var, semanticModel, knownReturnsNewObjectMethods).Any()) return true;
            }

            return false;
        }

        public static bool AnyImpureFieldInitializer(
            TypeDeclarationSyntax typeDeclaration,
            SemanticModel semanticModel,
            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods,
            bool onlyStaticFields = false)
        {
            var fields =
                typeDeclaration.Members
                    .OfType<FieldDeclarationSyntax>()
                    .Where(x => !onlyStaticFields || x.IsStatic())
                    .ToArray();

            foreach (var var in fields.SelectMany(x => x.Declaration.Variables))
            {
                if (Utils.GetImpurities(var, semanticModel, knownReturnsNewObjectMethods).Any()) return true;
            }

            return false;
        }

        public static bool IsNewlyCreatedObject(
            SemanticModel semanticModel,
            ExpressionSyntax expression,
            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods)
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
                            if (ReturnsNewObject(methodNode, semanticModel, knownReturnsNewObjectMethods))
                                return true;
                        }
                    }
                    else
                    {
                        if (GetAllAttributes(invokedMethod)
                            .Any(x => IsReturnsNewObjectAttribute(x.AttributeClass.Name)))
                        {
                            return true;
                        }

                        if (knownReturnsNewObjectMethods.TryGetValue(
                                Utils.GetFullMetaDataName(invokedMethod.ContainingType), out var methods) &&
                            methods.Contains(invokedMethod.Name))
                        {
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
            
            return FindValuesAssignedToVariable(method.GetValue().Body, local).All(x => IsNewlyCreatedObject(semanticModel, x, knownReturnsNewObjectMethods));
        }

        public static bool ReturnsNewObject(BaseMethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods)
        {
            return !GetNonNewObjectReturnsForMethod(methodDeclaration, semanticModel, knownReturnsNewObjectMethods).Any();
        }

        public static IEnumerable<ExpressionSyntax> GetNonNewObjectReturnsForMethod(
            BaseMethodDeclarationSyntax methodDeclaration,
            SemanticModel semanticModel,
            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods)
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
                if (!Utils.IsNewlyCreatedObject(semanticModel, expression, knownReturnsNewObjectMethods))
                {
                    yield return expression;
                }
            }
        }

        public static AttributeData[] GetAllAttributes(ISymbol symbol)
        {
            if (symbol is IMethodSymbol methodSymbol)
                return GetAllAttributes(methodSymbol);

            return symbol.GetAttributes().ToArray();
        }

        public static AttributeData[] GetAllAttributes(IMethodSymbol methodSymbol)
        {
            var attributes = methodSymbol.GetAttributes().ToList();

            if (methodSymbol.MethodKind == MethodKind.PropertyGet || methodSymbol.MethodKind == MethodKind.PropertySet)
            {
                var property = (IPropertySymbol) methodSymbol.AssociatedSymbol;

                attributes.AddRange(property.GetAttributes());
            }

            return attributes.ToArray();
        }
        public static Dictionary<string, HashSet<string>> GetKnownReturnsNewObjectMethods(SemanticModel semanticModel)
        {
            var returnsNewObjectMethodsFileContents =
                Resources.ReturnsNewObjectMethods
                    + Environment.NewLine
                    + PurityAnalyzerAnalyzer
                        .CustomReturnsNewObjectMethodsFilename.ChainValue(File.ReadAllText)
                        .ValueOr("");

            return returnsNewObjectMethodsFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(','))
                .Select(x => x.ThrowIf(v => v.Length != 2, "Invalid returns-new-object method line"))
                .Select(x => new { Type = x[0], Method = x[1].Trim() })
                .GroupBy(x => x.Type, x => x.Method)
                .ToDictionary(
                    x => x.Key,
                    x => new HashSet<string>(x));
        }

        public static string GetFullMetaDataName(INamedTypeSymbol typeSymbol)
        {
            string name = typeSymbol.MetadataName;

            if (typeSymbol.ContainingType != null)
                return GetFullMetaDataName(typeSymbol.ContainingType) + "." + name;

            if (typeSymbol.ContainingNamespace != null)
            {
                if (typeSymbol.ContainingNamespace.IsGlobalNamespace)
                    return name;

                return GetFullMetaDataName(typeSymbol.ContainingNamespace) + "." + name;
            }

            return name;
        }

        public static string GetFullMetaDataName(INamespaceSymbol @namespace)
        {
            string name = @namespace.Name;

            if (@namespace.ContainingNamespace != null && !@namespace.ContainingNamespace.IsGlobalNamespace)
                return GetFullMetaDataName(@namespace.ContainingNamespace) + "." + name;

            return name;

        }
    }
}