using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

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

        public static bool IsNotUsedAsObjectAttribute(AttributeSyntax attribute)
        {
            return attribute.Name is IdentifierNameSyntax name && IsNotUsedAsObjectAttribute(name.Identifier.Text);
        }

        public static bool IsNotUsedAsObjectAttribute(string attributeName)
        {
            return attributeName == "NotUsedAsObject" || attributeName == "NotUsedAsObject" + "Attribute";
        }

        public static bool IsDoesNotUseClassTypeParameterAsObjectAttribute(AttributeSyntax attribute, out IdentifierNameSyntax typeParameterIdentifier)
        {
            typeParameterIdentifier = null;

            if (!(attribute.Name is IdentifierNameSyntax name)) return false;

            if (!IsDoesNotUseClassTypeParameterAsObjectAttribute(name.Identifier.Text)) return false;

            if (attribute.ArgumentList.Arguments.Count != 1)
                return false;

            var argument = attribute.ArgumentList.Arguments[0];
            if (!(argument.Expression is InvocationExpressionSyntax invocation))
                return false;

            if (!(invocation.Expression is IdentifierNameSyntax identifier))
                return false;

            if (identifier.Identifier.Text != "nameof")
                return false;

            if (!(invocation.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax typeParameterIdentifier1))
                return false;

            typeParameterIdentifier = typeParameterIdentifier1;

            return true;
        }



        public static bool IsDoesNotUseClassTypeParameterAsObjectAttribute(string attributeName)
        {
            return attributeName == "DoesNotUseClassTypeParameterAsObject" || attributeName == "DoesNotUseClassTypeParameterAsObject" + "Attribute";
        }


        public static IEnumerable<Impurity> GetImpurities(
            SyntaxNode methodDeclaration,
            SemanticModel semanticModel,
            KnownSymbols knownSymbols,
            RecursiveState recursiveState,
            PurityType purityType = PurityType.Pure,
            Maybe<PureLambdaConfig> pureLambdaConfig = default)
        {
            var impuritiesFinder = new ImpuritiesFinder(semanticModel, purityType, knownSymbols);

            return impuritiesFinder.GetImpurities(methodDeclaration, recursiveState, pureLambdaConfig);
        }

        public static bool AnyImpurePropertyInitializer(TypeDeclarationSyntax typeDeclaration,
            SemanticModel semanticModel,
            KnownSymbols knownSymbols,
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
                if (Utils.GetImpurities(var, semanticModel, knownSymbols, recursiveState).Any()) return true;
            }

            return false;
        }

        public static bool AnyImpureFieldInitializer(
            TypeDeclarationSyntax typeDeclaration,
            SemanticModel semanticModel,
            KnownSymbols knownSymbols,
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
                if (Utils.GetImpurities(var, semanticModel, knownSymbols, recursiveState).Any()) return true;
            }

            return false;
        }

        public static bool IsNewlyCreatedObject(SemanticModel semanticModel,
            SyntaxNode expression,
            KnownSymbols knownSymbols,
            RecursiveIsNewlyCreatedObjectState recursiveState, RecursiveState recursiveState1)
        {
            if (expression is LiteralExpressionSyntax)
            {
                return true;
            }

            if (expression is TupleExpressionSyntax tuple)
            {
                return tuple.Arguments
                    .Select(x => x.Expression)
                    .All(x => IsNewlyCreatedObject(semanticModel, x, knownSymbols, recursiveState, recursiveState1));
            }


            if (expression is ObjectCreationExpressionSyntax objectCreationExpression)
            {
                if (objectCreationExpression
                        .ArgumentList != null)
                {
                    if (objectCreationExpression
                        .ArgumentList
                        .Arguments.Any(arg =>
                            !IsNewlyCreatedObject(semanticModel, arg.Expression, knownSymbols, recursiveState, recursiveState1)))
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
                            !IsNewlyCreatedObject(semanticModel, x.Right, knownSymbols, recursiveState, recursiveState1)))
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
                            !IsNewlyCreatedObject(semanticModel, x, knownSymbols, recursiveState, recursiveState1)))
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
                            !IsNewlyCreatedObject(semanticModel, x, knownSymbols, recursiveState, recursiveState1)))
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
                        !IsNewlyCreatedObject(semanticModel, x, knownSymbols, recursiveState, recursiveState1)))
                {
                    return false;
                }
                return true;
            }

            if (expression is InvocationExpressionSyntax invocationExpression)
            {
                if (semanticModel.GetSymbolInfo(invocationExpression.Expression).Symbol is IMethodSymbol invokedMethod)
                {
                    if (MethodReturnsNewObject(semanticModel, knownSymbols, recursiveState1, invokedMethod))
                        return true;
                }
            }

            if (expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                if (semanticModel.GetSymbolInfo(memberAccessExpression.Name).Symbol is IPropertySymbol propertySymbol && !GetUsage(memberAccessExpression.Name).IsWrite())
                {
                    if (MethodReturnsNewObject(semanticModel, knownSymbols, recursiveState1, propertySymbol.GetMethod))
                        return true;
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
                        GetBodyOfMethodThatContainsExpression(expression);

                    if (methodBody.HasNoValue)
                        return false;

                    var valuesInjectedIntoObject =
                        GetValuesPossiblyInjectedInto(semanticModel, local, methodBody.GetValue(), knownSymbols, recursiveState1);

                    if (!valuesInjectedIntoObject.All(x =>
                    {
                        var typeSymbol = semanticModel.GetTypeInfo(x).Type;

                        if (typeSymbol != null && IsImmutablePureData(typeSymbol))
                            return true;

                        return IsNewlyCreatedObject(semanticModel, x, knownSymbols, recursiveState.Add(local),
                                   recursiveState1);
                    }))
                        return false;

                    return FindValuesAssignedToVariable(semanticModel, local, methodBody.GetValue()).All(x =>
                        IsNewlyCreatedObject(semanticModel, x, knownSymbols, recursiveState.Add(local), recursiveState1));
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

        public static Maybe<BlockSyntax> GetBodyOfMethodThatContainsExpression(SyntaxNode expression)
        {
            return expression
                .Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrNoValue()
                .ChainValue(x => x.Body)
                .ValueOrMaybe(() =>
                    expression.Ancestors().OfType<AccessorDeclarationSyntax>().FirstOrNoValue()
                        .ChainValue(x => x.Body));
        }

        private static bool MethodReturnsNewObject(
            SemanticModel semanticModel,
            KnownSymbols knownSymbols,
            RecursiveState recursiveState,
            IMethodSymbol invokedMethod)
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
                        Utils.GetSemanticModel(semanticModel, methodNode.SyntaxTree),
                        knownSymbols, recursiveState))
                        return true;
                }
                else if (node.Parent.Parent is PropertyDeclarationSyntax propertyDeclaration)
                {
                    if (ReturnsNewObject(
                        propertyDeclaration,
                        Utils.GetSemanticModel(semanticModel, propertyDeclaration.SyntaxTree),
                        knownSymbols, recursiveState))
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

                if (knownSymbols.KnownReturnsNewObjectMethods.TryGetValue(
                        Utils.GetFullMetaDataName(invokedMethod.ContainingType), out var methods) &&
                    methods.AnyMatches(invokedMethod))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<ExpressionSyntax> GetValuesPossiblyInjectedInto(SemanticModel semanticModel,
            ILocalSymbol variable,
            SyntaxNode containingBlockNode, KnownSymbols knownSymbols, RecursiveState recursiveState)
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
                    if (semanticModel.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol invokedMethod)
                    {
                        var purityType = ImpuritiesFinder.GetMethodPurityType(semanticModel, knownSymbols,
                            invokedMethod, recursiveState);

                        if (purityType.HasValueAnd(x => x.Equals(PurityType.PureExceptLocally)))
                        {
                            result.AddRange(invocation.ArgumentList.Arguments.Select(x => x.Expression));
                        }
                    }
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

        public static bool ReturnsNewObject(BaseMethodDeclarationSyntax methodDeclaration,
            SemanticModel semanticModel,
            KnownSymbols knownSymbols, RecursiveState recursiveState1)
        {
            return !GetNonNewObjectReturnsForMethod(methodDeclaration, semanticModel, knownSymbols, recursiveState1).Any();
        }

        public static bool ReturnsNewObject(PropertyDeclarationSyntax propertyDeclaration,
            SemanticModel semanticModel,
            KnownSymbols knownSymbols,
            RecursiveState recursiveState1)
        {
            return !GetNonNewObjectReturnsForPropertyGet(propertyDeclaration, semanticModel, knownSymbols, recursiveState1).Any();
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
            KnownSymbols knownSymbols, RecursiveState recursiveState1)
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
                if (!Utils.IsNewlyCreatedObject(semanticModel, expression, knownSymbols, RecursiveIsNewlyCreatedObjectState.Empty(), recursiveState1))
                {
                    yield return expression;
                }
            }
        }

        public static IEnumerable<ExpressionSyntax> GetNonNewObjectReturnsForPropertyGet(
            PropertyDeclarationSyntax propertyDeclaration,
            SemanticModel semanticModel,
            KnownSymbols knownSymbols,
            RecursiveState recursiveState1)
        {
            var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);

            if (propertySymbol.GetMethod == null)
                yield break;

            if (!propertySymbol.ReturnsByRef && IsCompleteValueType(propertySymbol.Type))
                yield break;

            List<ExpressionSyntax> returnExpressions;

            if (propertyDeclaration.AccessorList != null)
            {
                var getAccessor =
                    propertyDeclaration.AccessorList.Accessors.FirstOrNoValue(x =>
                        x.Keyword.Kind() == SyntaxKind.GetKeyword);

                if (getAccessor.HasNoValue)
                    yield break;

                var getAccessorValue = getAccessor.GetValue();

                returnExpressions =
                    getAccessorValue.ExpressionBody != null
                        ? new List<ExpressionSyntax>() { getAccessorValue.ExpressionBody.Expression }
                        : getAccessor.GetValue()
                            .DescendantNodes()
                            .OfType<ReturnStatementSyntax>()
                            .Select(x => x.Expression)
                            .ToList();

            }
            else if (propertyDeclaration.ExpressionBody != null)
            {
                returnExpressions = new List<ExpressionSyntax>() {propertyDeclaration.ExpressionBody.Expression};
            }
            else yield break;

            foreach (var expression in returnExpressions)
            {
                if (!Utils.IsNewlyCreatedObject(semanticModel, expression, knownSymbols, RecursiveIsNewlyCreatedObjectState.Empty(), recursiveState1))
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
            if (typeSymbol.TypeKind == TypeKind.TypeParameter)
                return typeSymbol.MetadataName;

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
            Compilation compilation,
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

            if (typeSymbol.TypeKind == TypeKind.Interface)
            {
                foreach (var objectMethod in compilation.ObjectType.GetMembers().OfType<IMethodSymbol>())
                    yield return objectMethod;
            }
        }

        private static IMethodSymbol[] GetAllMethods(INamedTypeSymbol symbol)
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

        public static bool IsAccessOnNewlyCreatedObject(KnownSymbols knownSymbols,
            SemanticModel semanticModel,
            SyntaxNode node, RecursiveState recursiveState1)
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
                return Utils.IsNewlyCreatedObject(semanticModel, exp, knownSymbols, RecursiveIsNewlyCreatedObjectState.Empty(), recursiveState1);
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

        public static (string Type, MethodDescriptor Method, string[] typeParams) ParseMethodDescriptorAndTypeParametersLine(string line)
        {
            var seperatedByColumn = line.Split(new []{":"}, StringSplitOptions.RemoveEmptyEntries);

            (var type, var method) = ParseMethodDescriptorLine(seperatedByColumn[0]);

            var typeParams = seperatedByColumn[1].Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

            return (type, method, typeParams);
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

        public static Dictionary<string, HashSet<MethodDescriptor>> GetKnownPureMethods()
        {
            var pureMethodsFileContents =
                Resources.PureMethods
                + Environment.NewLine
                + PurityAnalyzerAnalyzer
                    .CustomPureMethodsFilename.ChainValue(File.ReadAllText)
                    .ValueOr("");

            return pureMethodsFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Utils.ParseMethodDescriptorLine)
                .GroupBy(x => x.Type, x => x.Method)
                .ToDictionary(
                    x => x.Key,
                    x => new HashSet<MethodDescriptor>(x));
        }

        public static Dictionary<string, HashSet<MethodDescriptor>> GetKnownPureExceptLocallyMethods()
        {
            var pureMethodsExceptLocallyFileContents =
                Resources.PureExceptLocallyMethods
                + Environment.NewLine
                + PurityAnalyzerAnalyzer
                    .CustomPureExceptLocallyMethodsFilename.ChainValue(File.ReadAllText)
                    .ValueOr("");

            return pureMethodsExceptLocallyFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Utils.ParseMethodDescriptorLine)
                .GroupBy(x => x.Type, x => x.Method)
                .ToDictionary(
                    x => x.Key,
                    x => new HashSet<MethodDescriptor>(x));
        }

        public static Dictionary<string, HashSet<MethodDescriptor>> GetKnownPureExceptReadLocallyMethods()
        {
            var pureMethodsExceptLocallyFileContents =
                Resources.PureExceptReadLocallyMethods
                + Environment.NewLine
                + PurityAnalyzerAnalyzer
                    .CustomPureExceptReadLocallyMethodsFilename.ChainValue(File.ReadAllText)
                    .ValueOr("");

            return pureMethodsExceptLocallyFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Utils.ParseMethodDescriptorLine)
                .GroupBy(x => x.Type, x => x.Method)
                .ToDictionary(
                    x => x.Key,
                    x => new HashSet<MethodDescriptor>(x));
        }

        public static HashSet<INamedTypeSymbol> GetKnownPureTypes(SemanticModel semanticModel1)
        {
            var pureTypesFileContents =
                Resources.PureTypes
                + Environment.NewLine
                + PurityAnalyzerAnalyzer
                    .CustomPureTypesFilename.ChainValue(File.ReadAllText)
                    .ValueOr("");

            var pureTypes =
                pureTypesFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            return new HashSet<INamedTypeSymbol>(pureTypes.Select(x => semanticModel1.Compilation.GetTypeByMetadataName(x)));
        }

        public static SemanticModel GetSemanticModel(
            SemanticModel currentSemanticModel,
            SyntaxTree tree)
        {
            if (currentSemanticModel.SyntaxTree.Equals(tree))
                return currentSemanticModel;

            var currentCompilation = currentSemanticModel.Compilation;

            if (currentCompilation.ContainsSyntaxTree(tree))
                return currentCompilation.GetSemanticModel(tree);

            return PurityAnalyzerAnalyzer.GetSemanticModelForSyntaxTreeAsync(tree).Result;
        }

        public static bool IsPureData(ITypeSymbol type)
        {
            var pureTypes = new[]
            {
                SpecialType.System_Boolean,
                SpecialType.System_Byte,
                SpecialType.System_Char,
                SpecialType.System_DateTime,
                SpecialType.System_Decimal,
                SpecialType.System_Double,
                SpecialType.System_Int16,
                SpecialType.System_Int32,
                SpecialType.System_Int64,
                SpecialType.System_UInt16,
                SpecialType.System_UInt32,
                SpecialType.System_Int64,
                SpecialType.System_String,
                SpecialType.System_SByte,
                SpecialType.System_Single
            };

            if (pureTypes.Contains(type.SpecialType))
                return true;

            if (type is IArrayTypeSymbol array)
                return IsPureData(array.ElementType);

            return false;

            //TODO: maybe I should have a special attribute for pure data
            //TODO: include KeyValuePair, Tuple, ValueTuple
            //TODO: include ImmutableArray and ImmutableList
        }

        public static bool IsImmutablePureData(ITypeSymbol type)
        {
            var pureTypes = new[]
            {
                SpecialType.System_Boolean,
                SpecialType.System_Byte,
                SpecialType.System_Char,
                SpecialType.System_DateTime,
                SpecialType.System_Decimal,
                SpecialType.System_Double,
                SpecialType.System_Int16,
                SpecialType.System_Int32,
                SpecialType.System_Int64,
                SpecialType.System_UInt16,
                SpecialType.System_UInt32,
                SpecialType.System_Int64,
                SpecialType.System_String,
                SpecialType.System_SByte,
                SpecialType.System_Single
            };

            if (pureTypes.Contains(type.SpecialType))
                return true;

            return false;

            //TODO: maybe I should have a special attribute for pure immutable data
            //TODO: include ImmutableArray and ImmutableList
        }

        public static PurityType
            ChangeAcceptedPurityTypeBasedOnWhetherExpressionRepresentsAccessOnNewObjectOrParameterBasedAccess(
                PurityType currentAcceptedPurityType,
                ExpressionSyntax node,
                KnownSymbols knownSymbols,
                SemanticModel semanticModel,
                RecursiveState recursiveState)
        {
            if (Utils.IsAccessOnNewlyCreatedObject(knownSymbols, semanticModel, node, recursiveState))
            {
                return PurityType.PureExceptLocally;
            }
            else if (ImpuritiesFinder.IsParameterBasedAccess(semanticModel, node))
            {
                return PurityType.PureExceptReadLocally;
            }

            return currentAcceptedPurityType;
        }

        public static PurityType
            ChangeAcceptedPurityTypeBasedOnWhetherExpressionRepresentsANewObjectOrParameterBasedExpression(
                PurityType currentAcceptedPurityType,
                ExpressionSyntax node,
                KnownSymbols knownSymbols,
                SemanticModel semanticModel,
                RecursiveState recursiveState)
        {
            if (Utils.IsNewlyCreatedObject(semanticModel, node, knownSymbols, RecursiveIsNewlyCreatedObjectState.Empty(), recursiveState))
            {
                return PurityType.PureExceptLocally;
            }
            else if (ImpuritiesFinder.IsParameter(semanticModel, node))
            {
                return PurityType.PureExceptReadLocally;
            }

            return currentAcceptedPurityType;
        }

        public static bool IsPureLambdaMethodInvocation(
            SemanticModel semanticModel,
            string pureLambdaMethodFullClassName,
            string pureLambdaMethodName,
            InvocationExpressionSyntax expression)
        {
            if (!(semanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol method))
                return false;

            if (method.Name != pureLambdaMethodName)
                return false;

            if (Utils.GetFullMetaDataName(method.ContainingType) != pureLambdaMethodFullClassName)
                return false;
            return true;
        }

        public static List<(SyntaxNode node, ITypeSymbol from, ITypeSymbol to)> GetConversions(
            SyntaxNode scope,
            SemanticModel semanticModel)
        {
            return scope.DescendantNodes()
                .Select(x => (node: x, typeInfo: semanticModel.GetTypeInfo(x)))
                .Where(
                    x => x.typeInfo.Type != null && x.typeInfo.ConvertedType != null && !x.typeInfo.Type.Equals(x.typeInfo.ConvertedType))

                .Select(x => (node: x.node, from: x.typeInfo.Type, to: x.typeInfo.ConvertedType))
                .Concat(
                    scope.DescendantNodes().OfType<CastExpressionSyntax>()
                        .Select(x => (node: (SyntaxNode)x, from: semanticModel.GetTypeInfo(x.Expression).Type,
                            to: semanticModel.GetTypeInfo(x.Type).Type))
                        .Where(x => x.@from != null && x.to != null && !x.@from.Equals(x.to)))
                .ToList();
        }

        public static Dictionary<string, Dictionary<MethodDescriptor, string[]>> GetKnownNotUsedAsObjectTypeParameters()
        {
            var notUsedAsObjectsTypeParametersFileContents =
                Resources.NotUsedAsObjectsTypeParameters;

            return notUsedAsObjectsTypeParametersFileContents
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Utils.ParseMethodDescriptorAndTypeParametersLine)
                .GroupBy(x => x.Type, x => x)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToDictionary(y => y.Method, y=> y.typeParams));
        }
    }
}