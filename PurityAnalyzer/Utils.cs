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

        public static IEnumerable<Impurity> GetImpurities(SyntaxNode methodDeclaration,
            SemanticModel semanticModel,
            Dictionary<string, HashSet<MethodDescriptor>> knownReturnsNewObjectMethods,
            RecursiveState recursiveState, PurityType purityType = PurityType.Pure)
        {
            var impuritiesFinder = new ImpuritiesFinder(semanticModel, purityType, knownReturnsNewObjectMethods);

            return impuritiesFinder.GetImpurities(methodDeclaration, recursiveState);
        }

        public static bool AnyImpurePropertyInitializer(TypeDeclarationSyntax typeDeclaration,
            SemanticModel semanticModel,
            Dictionary<string, HashSet<MethodDescriptor>> knownReturnsNewObjectMethods,
            RecursiveState recursiveState,
            InstanceStaticCombination instanceStaticCombination)
        {
            var props = typeDeclaration
                .Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(instanceStaticCombination.Matches)
                .ToArray();

            foreach (var var in props.Select(x => x.Initializer).Where(i => i != null))
            {
                if (Utils.GetImpurities(var, semanticModel, knownReturnsNewObjectMethods, recursiveState).Any()) return true;
            }

            return false;
        }

        public static bool AnyImpureFieldInitializer(
            TypeDeclarationSyntax typeDeclaration,
            SemanticModel semanticModel,
            Dictionary<string, HashSet<MethodDescriptor>> knownReturnsNewObjectMethods,
            RecursiveState recursiveState,
            InstanceStaticCombination instanceStaticCombination)
        {
            var fields =
                typeDeclaration.Members
                    .OfType<FieldDeclarationSyntax>()
                    .Where(instanceStaticCombination.Matches)
                    .ToArray();

            foreach (var var in fields.SelectMany(x => x.Declaration.Variables))
            {
                if (Utils.GetImpurities(var, semanticModel, knownReturnsNewObjectMethods, recursiveState).Any()) return true;
            }

            return false;
        }

        public static bool IsNewlyCreatedObject(
            SemanticModel semanticModel,
            SyntaxNode expression,
            Dictionary<string, HashSet<MethodDescriptor>> knownReturnsNewObjectMethods,
            RecursiveIsNewlyCreatedObjectState recursiveState)
        {
            if (expression is LiteralExpressionSyntax)
            {
                return true;
            }

            if (expression is TupleExpressionSyntax tuple)
            {
                return tuple.Arguments
                    .Select(x => x.Expression)
                    .All(x => IsNewlyCreatedObject(semanticModel, x, knownReturnsNewObjectMethods, recursiveState));
            }


            if (expression is ObjectCreationExpressionSyntax objectCreationExpression)
            {
                if (objectCreationExpression
                        .ArgumentList != null)
                {
                    if (objectCreationExpression
                        .ArgumentList
                        .Arguments.Any(arg =>
                            !IsNewlyCreatedObject(semanticModel, arg.Expression, knownReturnsNewObjectMethods, recursiveState)))
                    {
                        return false;
                    }
                }


                if (objectCreationExpression.Initializer != null)
                {
                    if (objectCreationExpression
                        .Initializer
                        .Expressions
                        .OfType<AssignmentExpressionSyntax>()
                        .Any(x =>
                            !IsNewlyCreatedObject(semanticModel, x.Right, knownReturnsNewObjectMethods, recursiveState)))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (expression is ArrayCreationExpressionSyntax arrayCreationExpression)
            {
                if (arrayCreationExpression.Initializer != null)
                {
                    if (arrayCreationExpression
                        .Initializer
                        .Expressions
                        .Any(x =>
                            !IsNewlyCreatedObject(semanticModel, x, knownReturnsNewObjectMethods, recursiveState)))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (expression is ImplicitArrayCreationExpressionSyntax arrayCreationExpression1)
            {
                if (arrayCreationExpression1.Initializer != null)
                {
                    if (arrayCreationExpression1
                        .Initializer
                        .Expressions
                        .Any(x =>
                            !IsNewlyCreatedObject(semanticModel, x, knownReturnsNewObjectMethods, recursiveState)))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (expression is InitializerExpressionSyntax initSyntax &&
                initSyntax.Kind() == SyntaxKind.ArrayInitializerExpression)
            {
                if (initSyntax
                    .Expressions
                    .Any(x =>
                        !IsNewlyCreatedObject(semanticModel, x, knownReturnsNewObjectMethods, recursiveState)))
                {
                    return false;
                }
                return true;
            }

            if (expression is InvocationExpressionSyntax invocationExpression)
            {
                if (semanticModel.GetSymbolInfo(invocationExpression.Expression).Symbol is IMethodSymbol invokedMethod)
                {
                    if (invokedMethod.IsInCode() && !invokedMethod.IsAbstract)
                    {
                        var location = invokedMethod.Locations.First();

                        var locationSourceTree = location.SourceTree;

                        var node = locationSourceTree.GetRoot().FindNode(location.SourceSpan);

                        if (node is BaseMethodDeclarationSyntax methodNode)
                        {
                            if (ReturnsNewObject(
                                methodNode,
                                semanticModel.Compilation.GetSemanticModel(methodNode.SyntaxTree),
                                knownReturnsNewObjectMethods))
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
                            methods.AnyMatches(invokedMethod))
                        {
                            return true;
                        }
                    }
                }
            }

            if (expression is IdentifierNameSyntax identifier)
            {
                var identifierSymbol = semanticModel.GetSymbolInfo(identifier).Symbol;

                if (identifierSymbol is ILocalSymbol local)
                {
                    if (!local.IsRef && IsCompleteValueType(local.Type))
                        return true;

                    if (recursiveState.VariablesUnderTest.Contains(local))
                        return true;

                    var methodBody =
                        expression
                            .Ancestors()
                            .OfType<MethodDeclarationSyntax>()
                            .FirstOrNoValue()
                            .ChainValue(x => x.Body)
                            .ValueOrMaybe(() =>
                                expression.Ancestors().OfType<AccessorDeclarationSyntax>().FirstOrNoValue()
                                    .ChainValue(x => x.Body));

                    if (methodBody.HasNoValue)
                        return false;

                    var valuesInjectedIntoObject =
                        GetValuesPossiblyInjectedInto(semanticModel, local, methodBody.GetValue());

                    if (!valuesInjectedIntoObject.All(x =>
                        IsNewlyCreatedObject(semanticModel, x, knownReturnsNewObjectMethods, recursiveState.Add(local))))
                        return false;

                    return FindValuesAssignedToVariable(semanticModel, local, methodBody.GetValue()).All(x =>
                        IsNewlyCreatedObject(semanticModel, x, knownReturnsNewObjectMethods, recursiveState.Add(local)));
                }
            }

            if (semanticModel.GetTypeInfo(expression).Type is ITypeSymbol type)
            {
                if (IsCompleteValueType(type))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<ExpressionSyntax> GetValuesPossiblyInjectedInto(
            SemanticModel semanticModel,
            ILocalSymbol variable,
            SyntaxNode containingBlockNode)
        {
            var usages = containingBlockNode
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(x => x.Identifier.Text == variable.Name)
                .Where(x => semanticModel.GetSymbolInfo(x).Symbol?.Equals(variable) ?? false)
                .Select(x => x.Parent)
                .ToList();

            List<ExpressionSyntax> result = new List<ExpressionSyntax>();

            void HandleMemberAccess(MemberAccessExpressionSyntax usage)
            {
                if (usage.Parent is AssignmentExpressionSyntax assignment)
                {
                    result.Add(assignment.Right);
                }
                else if (usage.Parent is InvocationExpressionSyntax invocation)
                {
                    result.AddRange(invocation.ArgumentList.Arguments.Select(x => x.Expression));
                }
                else if (usage.Parent is MemberAccessExpressionSyntax memberAccess)
                {
                    HandleMemberAccess(memberAccess);
                }
            }

            foreach (var usage in usages)
            {
                if (usage is MemberAccessExpressionSyntax memberAccess)
                {
                    HandleMemberAccess(memberAccess);
                }
                else if (usage is ElementAccessExpressionSyntax elementAccess)
                {
                    if (elementAccess.Parent is AssignmentExpressionSyntax assignment)
                    {
                        result.Add(assignment.Right);
                    }
                }
            }

            return result;
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

        public static bool ReturnsNewObject(BaseMethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, Dictionary<string, HashSet<MethodDescriptor>> knownReturnsNewObjectMethods)
        {
            return !GetNonNewObjectReturnsForMethod(methodDeclaration, semanticModel, knownReturnsNewObjectMethods).Any();
        }

        public static bool IsCompleteValueType(ITypeSymbol type)
        {
            if (!type.IsValueType)
                return false;

            return type.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(x => !x.IsStatic)
                .Select(x => x.Type)
                .All(IsCompleteValueType);
        }

        public static IEnumerable<ExpressionSyntax> GetNonNewObjectReturnsForMethod(
            BaseMethodDeclarationSyntax methodDeclaration,
            SemanticModel semanticModel,
            Dictionary<string, HashSet<MethodDescriptor>> knownReturnsNewObjectMethods)
        {
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);

            if (!methodSymbol.ReturnsByRef && IsCompleteValueType(methodSymbol.ReturnType))
                yield break;

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
                if (!Utils.IsNewlyCreatedObject(semanticModel, expression, knownReturnsNewObjectMethods, RecursiveIsNewlyCreatedObjectState.Empty()))
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
        public static Dictionary<string, HashSet<MethodDescriptor>> GetKnownReturnsNewObjectMethods(SemanticModel semanticModel)
        {
            var returnsNewObjectMethodsFileContents =
                Resources.ReturnsNewObjectMethods
                    + Environment.NewLine
                    + PurityAnalyzerAnalyzer
                        .CustomReturnsNewObjectMethodsFilename.ChainValue(File.ReadAllText)
                        .ValueOr("");

            return returnsNewObjectMethodsFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseMethodDescriptorLine)
                .GroupBy(x => x.Type, x => x.Method)
                .ToDictionary(
                    x => x.Key,
                    x => new HashSet<MethodDescriptor>(x));
        }

        public static string GetFullMetaDataName(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is IArrayTypeSymbol arrayType)
                return GetFullMetaDataName(arrayType.ElementType) + "[]";

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
                else if (operationNode is ArgumentSyntax argument)
                {
                    var kind = argument.RefKindKeyword.Kind();

                    if (kind == SyntaxKind.RefKeyword)
                    {
                        return IdentifierUsage.ReadFromAndWrittenTo();
                    }
                    if (kind == SyntaxKind.OutKeyword)
                    {
                        return IdentifierUsage.WrittenTo();
                    }
                }

                return IdentifierUsage.ReadFrom();
            }

            return GetUsageForOperation(identifier.Parent);
        }

        public static IEnumerable<IMethodSymbol> GetAllMethods(
            ITypeSymbol typeSymbol,
            Maybe<ITypeSymbol> upUntilBefore = default)
        {

            var myMethods = GetMethods(typeSymbol);

            foreach (var myMethod in myMethods)
                yield return myMethod;

            var current = typeSymbol.BaseType;

            while (current != null)
            {
                if (upUntilBefore.HasValue)
                    if (current.Equals(upUntilBefore.GetValue()))
                        break;

                foreach (var method in GetAllMethods(current))
                    yield return method;

                current = current.BaseType;
            }
        }

        public static IMethodSymbol[] GetAllMethods(INamedTypeSymbol symbol)
        {
            return
                symbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Concat(symbol.BaseType == null ? Array.Empty<IMethodSymbol>() : GetAllMethods(symbol.BaseType))
                    .ToArray();
        }

        public static IEnumerable<IMethodSymbol> GetMethods(ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers().OfType<IMethodSymbol>();
        }

        public static bool IsUpCast(ITypeSymbol sourceType, ITypeSymbol destinationType)
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
            Dictionary<string, HashSet<MethodDescriptor>> knownReturnsNewObjectMethods,
            SemanticModel semanticModel,
            SyntaxNode node)
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
                return Utils.IsNewlyCreatedObject(semanticModel, exp, knownReturnsNewObjectMethods, RecursiveIsNewlyCreatedObjectState.Empty());
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

        public static (string Type, MethodDescriptor Method) ParseMethodDescriptorLine(string line)
        {
            var partsSeparatedByComma = line.Split(new []{','}, 2);

            if (partsSeparatedByComma.Length != 2)
                throw new Exception("Invalid method description line");

            MethodDescriptor ParseMethodDescriptor(string str)
            {
                if(!str.Contains("("))
                    return new MethodDescriptor.ByName(str.Trim());

                if(!str.Contains(")"))
                    throw new Exception("Invalid method description line");

                var paramList = str.Substring(str.IndexOf('(') + 1);

                paramList = paramList.Substring(0, paramList.IndexOf(')'));


                return new MethodDescriptor.ByNameAndParameterTypes(
                    str.Substring(0, str.IndexOf('(')).Trim(),
                    paramList.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToImmutableArray());
            }

            return (partsSeparatedByComma[0], ParseMethodDescriptor(partsSeparatedByComma[1]));
        }
    }
}