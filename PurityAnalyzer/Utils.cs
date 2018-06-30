using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public static bool IsIsPureExceptReadLocallyAttribute(AttributeSyntax attribute)
        {
            return attribute.Name is IdentifierNameSyntax name && IsIsPureExceptReadLocallyAttribute(name.Identifier.Text);
        }

        public static bool IsIsPureAttribute(string attributeName)
        {
            return attributeName == "IsPure" || attributeName == "IsPure" + "Attribute";
        }

        public static bool IsIsPureExceptLocallyAttribute(string attributeName)
        {
            return attributeName == "IsPureExceptLocally" || attributeName == "IsPureExceptLocally" + "Attribute";
        }

        public static bool IsIsPureExceptReadLocallyAttribute(string attributeName)
        {
            return attributeName == "IsPureExceptReadLocally" || attributeName == "IsPureExceptReadLocally" + "Attribute";
        }

        public static Impurity[] GetImpurities(SyntaxNode methodDeclaration,
            SemanticModel semanticModel,
            Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods,
            PurityType purityType = PurityType.Pure)
        {
            var impuritiesFinder = new ImpuritiesFinder(semanticModel, purityType, knownReturnsNewObjectMethods);

            return impuritiesFinder.GetImpurities(methodDeclaration).ToArray();
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

            if (expression is ArrayCreationExpressionSyntax)
                return true;

            if (expression is ImplicitArrayCreationExpressionSyntax)
                return true;

            if (expression is InitializerExpressionSyntax initSyntax &&
                initSyntax.Kind() == SyntaxKind.ArrayInitializerExpression)
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

            var methodBody =
                expression
                    .Ancestors()
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrNoValue()
                    .ChainValue(x => x.Body)
                    .ValueOrMaybe(() => expression.Ancestors().OfType<AccessorDeclarationSyntax>().FirstOrNoValue().ChainValue(x => x.Body));

            if (methodBody.HasNoValue)
                return false;

            return FindValuesAssignedToVariable(semanticModel, local, methodBody.GetValue()).All(x => IsNewlyCreatedObject(semanticModel, x, knownReturnsNewObjectMethods));
        }

        public static List<ExpressionSyntax> FindValuesAssignedToVariable(
            SemanticModel semanticModel,
            ILocalSymbol variable,
            SyntaxNode containingBlockNode)
        {
            var declaration = variable.DeclaringSyntaxReferences.Single();

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
                .Where(x => x.Identifier.Text == variable.Name)
                .Where(x => semanticModel.GetSymbolInfo(x).Symbol?.Equals(variable) ?? false)
                .Where(x => x.Parent is AssignmentExpressionSyntax assignment && assignment.Left == x)
                .Select(x => ((AssignmentExpressionSyntax) x.Parent).Right)
                .ToArray();

            list.AddRange(usages);

            return list;
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

        public static IdentifierUsage GetUsage(SyntaxNode identifier)
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

        public static IEnumerable<IMethodSymbol> GetAllMethods(
            ITypeSymbol typeSymbol,
            Maybe<ITypeSymbol> downUntilBefore = default)
        {

            var myMethods = GetMethods(typeSymbol);

            foreach (var myMethod in myMethods)
                yield return myMethod;

            var current = typeSymbol.BaseType;

            while (current != null)
            {
                if (downUntilBefore.HasValue)
                    if (current.Equals(downUntilBefore.GetValue()))
                        break;

                foreach (var method in GetAllMethods(current))
                    yield return method;

                current = current.BaseType;
            }
        }

        public static IMethodSymbol[] GetAllMethods(INamedTypeSymbol symbol)
        {
            if (Utils.GetFullMetaDataName(symbol).Equals(typeof(object).FullName))
                return new IMethodSymbol[0];

            return
                symbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Concat(GetAllMethods(symbol.BaseType))
                    .ToArray();
        }

        public static IEnumerable<IMethodSymbol> GetMethods(ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers().OfType<IMethodSymbol>();
        }

        public static bool IsDownCast(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            if (destinationType.TypeKind == TypeKind.Interface)
            {
                return sourceType.AllInterfaces.Contains(destinationType);
            }

            var current = sourceType.BaseType;

            while (current != null)
            {
                if (current.Equals(destinationType))
                    return true;

                current = current.BaseType;
            }

            return false;
        }

        public static ImmutableArray<INamedTypeSymbol> GetAllInterfaceIncludingSelfIfIsInterface(ITypeSymbol type)
        {
            var allInterfaces = type.AllInterfaces;

            if (type.TypeKind == TypeKind.Interface)
                return allInterfaces.Add((INamedTypeSymbol)type);

            return allInterfaces;
        }

        public static bool SymbolHasIsPureAttribute(ISymbol symbol)
        {
            return Utils.GetAllAttributes(symbol).Any(x => Utils.IsIsPureAttribute(x.AttributeClass.Name));
        }

        public static bool SymbolHasIsPureExceptLocallyAttribute(ISymbol symbol)
        {
            return Utils.GetAllAttributes(symbol).Any(x => Utils.IsIsPureExceptLocallyAttribute(x.AttributeClass.Name));
        }

        public static bool SymbolHasIsPureExceptReadLocallyAttribute(ISymbol symbol)
        {
            return Utils.GetAllAttributes(symbol).Any(x => Utils.IsIsPureExceptReadLocallyAttribute(x.AttributeClass.Name));
        }

        public static bool SymbolHasAssumeIsPureAttribute(ISymbol symbol)
        {
            return Utils.GetAllAttributes(symbol).Any(x => x.AttributeClass.Name == "AssumeIsPureAttribute");
        }

        public static IMethodSymbol FindMostDerivedMethod(IMethodSymbol[] allMethods, IMethodSymbol method)
        {
            var current = method;

            while (true)
            {
                var next = allMethods
                    .FirstOrDefault(x => x.OverriddenMethod != null && x.OverriddenMethod.Equals(current));

                if (next == null)
                    return current;

                current = next;
            }
        }

        public static IMethodSymbol[] RemoveOverriddenMethods(IMethodSymbol[] methods)
        {
            HashSet<IMethodSymbol> set = new HashSet<IMethodSymbol>(methods);

            foreach (var method in methods)
            {
                if (method.OverriddenMethod != null)
                    set.Remove(method.OverriddenMethod);
            }

            return set.ToArray();
        }

        public static bool IsAccessOnNewlyCreatedObject(
            Dictionary<string, HashSet<string>> dictionary,
            SemanticModel semanticModel,
            ExpressionSyntax node)
        {
            bool IsOnNewlyCreatedObject(ExpressionSyntax exp)
            {
                if (exp is MemberAccessExpressionSyntax memberAccess1)
                {
                    return IsOnNewlyCreatedObject(memberAccess1.Expression);
                }
                if (exp is ElementAccessExpressionSyntax elementAccess1)
                {
                    return IsOnNewlyCreatedObject(elementAccess1.Expression);
                }
                return Utils.IsNewlyCreatedObject(semanticModel, exp, dictionary);
            }

            if (node.Parent is MemberAccessExpressionSyntax memberAccess)
            {
                return IsOnNewlyCreatedObject(memberAccess);
            }

            if (node is ElementAccessExpressionSyntax elementAccess)
            {
                return IsOnNewlyCreatedObject(elementAccess);
            }

            return false;
        }
    }
}