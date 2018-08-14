using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Operations;

namespace PurityAnalyzer
{
    public class ImpuritiesFinder
    {
        private readonly PurityType purityType;

        private readonly SemanticModel semanticModel;
        private readonly INamedTypeSymbol objectType;
        private readonly Dictionary<string, HashSet<MethodDescriptor>> knownPureMethods;
        private readonly Dictionary<string, HashSet<MethodDescriptor>> knownPureExceptLocallyMethods;
        private readonly Dictionary<string, HashSet<MethodDescriptor>> knownPureExceptReadLocallyMethods;
        private readonly Dictionary<string, HashSet<MethodDescriptor>> knownReturnsNewObjectMethods;
        private readonly HashSet<INamedTypeSymbol> knownPureTypes;
        private readonly IPropertySymbol arrayIListItemProperty;
        private readonly INamedTypeSymbol genericIenumeratorType;
        private readonly INamedTypeSymbol ienumeratorType;
        private readonly INamedTypeSymbol boolType;
        private readonly INamedTypeSymbol idisposableType;
        private readonly IMethodSymbol genericGetEnumeratorMethod;
        private readonly INamedTypeSymbol genericIenumerableType;
        private readonly INamedTypeSymbol genericListType;

        public ImpuritiesFinder(SemanticModel semanticModel, PurityType purityType, Dictionary<string, HashSet<MethodDescriptor>> knownReturnsNewObjectMethods)
        {
            this.semanticModel = semanticModel;
            this.purityType = purityType;
            this.knownReturnsNewObjectMethods = knownReturnsNewObjectMethods;

            objectType = semanticModel.Compilation.GetTypeByMetadataName(typeof(object).FullName);

            var arrayType = semanticModel.Compilation.GetTypeByMetadataName(typeof(Array).FullName);

            arrayIListItemProperty =
                arrayType.GetMembers("System.Collections.IList.Item")
                    .OfType<IPropertySymbol>()
                    .Single();

            ienumeratorType = semanticModel.Compilation.GetTypeByMetadataName(typeof(IEnumerator).FullName);
            genericIenumeratorType = semanticModel.Compilation.GetTypeByMetadataName(typeof(IEnumerator<>).FullName);
            boolType = semanticModel.Compilation.GetTypeByMetadataName(typeof(bool).FullName);
            idisposableType = semanticModel.Compilation.GetTypeByMetadataName(typeof(IDisposable).FullName);

            genericIenumerableType = semanticModel.Compilation.GetTypeByMetadataName(typeof(IEnumerable<>).FullName);
            genericGetEnumeratorMethod = genericIenumerableType.GetMembers("GetEnumerator").OfType<IMethodSymbol>().Single(x => x.ReturnType.OriginalDefinition?.Equals(genericIenumeratorType) ?? false);

            genericListType = semanticModel.Compilation.GetTypeByMetadataName(typeof(List<>).FullName);

            knownPureMethods = GetKnownPureMethods();
            knownPureExceptLocallyMethods = GetKnownPureExceptLocallyMethods();
            knownPureExceptReadLocallyMethods = GetKnownPureExceptReadLocallyMethods();
            knownPureTypes = GetKnownPureTypes();
        }

        public IEnumerable<Impurity> GetImpurities(SyntaxNode node, RecursiveState recursiveState)
        {
            var allNodes = node.DescendantNodesAndSelf();

            foreach (var subNode in allNodes)
            {
                if (ContainsImpureCast(subNode, recursiveState) is CastPurityResult.Impure impure)
                {
                    yield return new Impurity(subNode, "Cast is impure" + Environment.NewLine + impure.Reason);
                }

                if (subNode is CastExpressionSyntax castExpression)
                {
                    foreach (var impurity in GetImpurities1(castExpression, recursiveState))
                        yield return impurity;
                }
                else if (subNode is ObjectCreationExpressionSyntax objectCreation)
                {
                    foreach (var impurity in GetImpurities2(objectCreation, recursiveState))
                        yield return impurity;
                }
                else if (subNode is IdentifierNameSyntax identifierName)
                {
                    foreach (var impurity in GetImpurities3(identifierName, recursiveState))
                        yield return impurity;
                }
                else if (subNode is BinaryExpressionSyntax binaryExpression)
                {
                    foreach (var impurity in GetImpurities4(binaryExpression, recursiveState))
                        yield return impurity;
                }
                else if (subNode is PrefixUnaryExpressionSyntax prefixUnaryExpression)
                {
                    foreach (var impurity in GetImpurities8(prefixUnaryExpression, recursiveState))
                        yield return impurity;
                }
                else if (subNode is PostfixUnaryExpressionSyntax postfixUnaryExpression)
                {
                    foreach (var impurity in GetImpurities9(postfixUnaryExpression, recursiveState))
                        yield return impurity;
                }
                else if (subNode is AssignmentExpressionSyntax assignmentExpression)
                {
                    foreach (var impurity in GetImpurities5(assignmentExpression, recursiveState))
                        yield return impurity;
                }
                else if (subNode is ElementAccessExpressionSyntax elementAccessExpression)
                {
                    foreach (var impurity in GetImpurities6(elementAccessExpression, recursiveState))
                        yield return impurity;
                }
                else if (subNode is CommonForEachStatementSyntax forEachStatement)
                {
                    foreach (var impurity in GetImpurities7(forEachStatement, recursiveState))
                        yield return impurity;
                }
                else if (subNode is IfStatementSyntax ifStatement)
                {
                    if (semanticModel.GetOperation(ifStatement) is IConditionalOperation conditionalOperation)
                    {
                        foreach (var impurity in GetConditionalOperationImpurities(recursiveState, conditionalOperation, ifStatement))
                            yield return impurity;
                    }
                }
                else if (subNode is ConditionalExpressionSyntax conditional)
                {
                    if (semanticModel.GetOperation(conditional) is IConditionalOperation conditionalOperation)
                    {
                        foreach (var impurity in GetConditionalOperationImpurities(recursiveState, conditionalOperation, conditional))
                            yield return impurity;
                    }
                }
            }
        }

        private IEnumerable<Impurity> GetConditionalOperationImpurities(
            RecursiveState recursiveState,
            IConditionalOperation conditionalOperation1,
            SyntaxNode node)
        {
            if (conditionalOperation1.Condition is IUnaryOperation unaryOperation)
            {
                if (unaryOperation.OperatorKind == UnaryOperatorKind.True)
                {
                    if (!IsMethodPure(unaryOperation.OperatorMethod, recursiveState))
                    {
                        yield return new Impurity(node, "True operator is not pure");
                    }
                }
            }
        }

        private IEnumerable<Impurity> GetImpurities8(PrefixUnaryExpressionSyntax node, RecursiveState recursiveState)
        {
            if (semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol method)
            {
                if (!IsMethodPure(method, recursiveState))
                {
                    yield return new Impurity(node, "Operator is impure");
                }
            }
        }

        private IEnumerable<Impurity> GetImpurities9(PostfixUnaryExpressionSyntax node, RecursiveState recursiveState)
        {
            if (semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol method)
            {
                if (!IsMethodPure(method, recursiveState))
                {
                    yield return new Impurity(node, "Operator is impure");
                }
            }
        }

        public IEnumerable<Impurity> GetImpurities7(CommonForEachStatementSyntax forEachStatement,
            RecursiveState recursiveState)
        {
            var expressionType = semanticModel.GetTypeInfo(forEachStatement.Expression);

            if (expressionType.Type is ITypeSymbol typeSymbol)
            {
                var getEnumeratorMethods = typeSymbol.GetMembers("GetEnumerator").OfType<IMethodSymbol>().ToList();

                var getEnumeratorMethod =
                    new[]
                    {
                        getEnumeratorMethods.Where(x => x.ReturnType.Equals(genericIenumeratorType)),
                        getEnumeratorMethods.Where(x => x.ReturnType.Equals(ienumeratorType)),
                        getEnumeratorMethods
                    }
                        .SelectMany(x => x)
                        .FirstOrNoValue();



                if (getEnumeratorMethod.HasValue)
                {
                    if(!IsMethodPure(getEnumeratorMethod.GetValue(), recursiveState))
                        yield return new Impurity(forEachStatement, "GetEnumerator method is impure");


                    var returnType = getEnumeratorMethod.GetValue().ReturnType;

                    var currentPropertyGetter =
                        returnType.GetMembers("Current")
                            .OfType<IPropertySymbol>()
                            .FirstOrNoValue()
                            .ChainValue(x => x.GetMethod);

                    if (currentPropertyGetter.HasValue)
                    {
                        if (!IsMethodPure(currentPropertyGetter.GetValue(), recursiveState))
                            yield return new Impurity(forEachStatement, "Current property is impure");
                    }

                    var moveNextMethod =
                        returnType.GetMembers("MoveNext")
                            .OfType<IMethodSymbol>()
                            .Where(x => x.Parameters.Length == 0)
                            .Where(x => x.TypeParameters.Length == 0)
                            .Where(x => x.ReturnType.Equals(boolType))
                            .FirstOrNoValue();

                    if (moveNextMethod.HasValue)
                    {
                        if (!IsMethodPure(moveNextMethod.GetValue(), recursiveState))
                            yield return new Impurity(forEachStatement, "MoveNext method is impure");
                    }

                    if (returnType.Interfaces.Any(x => x.Equals(idisposableType)))
                    {
                        if (
                            returnType
                                .FindImplementationForInterfaceMember(
                                    idisposableType.GetMembers("Dispose").First()) is IMethodSymbol disposeMethod)
                        {
                            if (!IsMethodPure(disposeMethod, recursiveState))
                                yield return new Impurity(forEachStatement, "Dispose method is impure");
                        }
                    }
                }
            }
        }

        private IEnumerable<Impurity> GetImpurities1(CastExpressionSyntax node,
            RecursiveState recursiveState)
        {
            if (semanticModel.GetSymbolInfo(node.Type).Symbol is ITypeSymbol destinationType &&
                semanticModel.GetTypeInfo(node.Expression).Type is ITypeSymbol sourceType)
            {
                if (IsImpureCast(sourceType, destinationType, recursiveState, node.Expression) is CastPurityResult.Impure impure)
                {
                    yield return new Impurity(node, "Cast is impure" + Environment.NewLine + impure.Reason);
                }
            }
        }

        private bool IsPureData(ITypeSymbol type)
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

        private bool IsInstanceField(SyntaxNode syntaxNode)
        {
            return syntaxNode is IdentifierNameSyntax identifier &&
                   semanticModel.GetSymbolInfo(identifier).Symbol is IFieldSymbol field && !field.IsStatic;
        }

        private CastPurityResult IsImpureCast(ITypeSymbol sourceType, ITypeSymbol destinationType,
            RecursiveState recursiveState, SyntaxNode sourceNode)
        {
            if (sourceType.Equals(destinationType))
                return new CastPurityResult.Pure();

            var allDestinationMethods = Utils.GetAllMethods(destinationType).ToArray();

            var nonInterfaceMethodsToCheck =
                Utils.RemoveOverriddenMethods(allDestinationMethods)
                    .Where(x => x.IsAbstract || x.IsVirtual || x.IsOverride);

            var destInterfaceBasedMethods = 
                    Utils.GetAllInterfaceIncludingSelfIfIsInterface(destinationType)
                        .SelectMany(x => x.GetMembers()
                        .OfType<IMethodSymbol>())
                        .Select(x => destinationType.TypeKind == TypeKind.Interface ? x : destinationType.FindImplementationForInterfaceMember(x))
                        .Where(x => x != null)
                        .OfType<IMethodSymbol>()
                        .Select(x => Utils.FindMostDerivedMethod(allDestinationMethods, x))
                        .ToArray();

            var methodsToCheck = nonInterfaceMethodsToCheck.Union(destInterfaceBasedMethods).ToArray();

            List<IMethodSymbol> problems = new List<IMethodSymbol>();
            
            foreach (var destMethod in methodsToCheck)
            {
                var matchingMethodInSourceType = GetMatchingMethod(destMethod, sourceType);

                if (matchingMethodInSourceType.HasNoValue)
                {
                    if (!destMethod.IsSealed && !destinationType.IsSealed)
                    {
                        if (IsAnyKindOfPure(destMethod, recursiveState))
                        {
                            problems.Add(destMethod);
                        }
                    }
                }
                else
                {
                    var srcPurity = GetMethodPurityType(matchingMethodInSourceType.GetValue(), recursiveState);
                    var destPurity = GetMethodPurityType(destMethod, recursiveState);

                    bool IsSourceNodeANewObject()
                    {
                        return Utils.IsNewlyCreatedObject(semanticModel, sourceNode, knownReturnsNewObjectMethods);
                    }

                    bool IsSourceNodeAParameter()
                    {
                        return IsParameter(sourceNode);
                    }

                    bool IsSourceNodeAccessToLocalState()
                    {
                        if (IsInstanceField(sourceNode))
                            return true;

                        if (IsAPropertyWithAnInstancePureExceptReadLocallyGetter(sourceNode, recursiveState))
                            return true;

                        if (sourceNode is MemberAccessExpressionSyntax memberAccess &&
                            memberAccess.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                        {
                            if (memberAccess.Name is IdentifierNameSyntax identifier)
                            {
                                if (IsThisExpressionOrASeriesOfFieldAccesses(memberAccess.Expression))
                                {
                                    if (IsInstanceField(identifier))
                                    {

                                        return true;
                                    }
                                    else
                                    {
                                        if (IsAPropertyWithAnInstancePureExceptReadLocallyGetter(identifier, recursiveState))
                                            return true;
                                    }
                                }

                                

                            }
                        }

                        return false;
                    }

                    bool IsOkAsReturnTypeFromMethodThatTakesInCastedNewObjectOrParameter(ITypeSymbol type)
                    {
                        return type.SpecialType == SpecialType.System_Void || IsPureData(type);
                    }

                    

                    bool IsSourceNodeOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods()
                    {
                        bool IsOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods(SyntaxNode node)
                        {
                            if (node.Parent is ArgumentSyntax argument
                                && argument.Parent is ArgumentListSyntax argumentList
                                && argumentList.Parent is InvocationExpressionSyntax invocation)
                            {
                                if (semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol)
                                {
                                    if (IsAtLeastPureExceptReadLocally(methodSymbol, recursiveState))
                                        return
                                            IsOkAsReturnTypeFromMethodThatTakesInCastedNewObjectOrParameter(methodSymbol.ReturnType) ||
                                            IsOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods(invocation);
                                }

                                return false;
                            }
                            else if (node.Parent is AssignmentExpressionSyntax assignmentExpression &&
                                     assignmentExpression.Kind() == SyntaxKind.SimpleAssignmentExpression &&
                                     assignmentExpression.Left is IdentifierNameSyntax identifier &&
                                     semanticModel.GetSymbolInfo(identifier).Symbol is ILocalSymbol localSymbol)
                            {
                                var scope = node
                                    .Ancestors().First(x => x is BlockSyntax || x is ArrowExpressionClauseSyntax);

                                var usagesOfVariable = scope.DescendantNodes()
                                    .OfType<IdentifierNameSyntax>()
                                    .Where(x => !x.Equals(identifier))
                                    .Where(x => x.Identifier.Text == identifier.Identifier.Text)
                                    .Where(x => semanticModel.GetSymbolInfo(x).Symbol is ILocalSymbol sym &&
                                                sym.Equals(localSymbol))
                                    .ToImmutableArray();

                                return usagesOfVariable.All(IsOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods);
                            }
                            else if (node.Parent is EqualsValueClauseSyntax equalsValueClause
                                     && equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator)
                            {
                                var variableName = variableDeclarator.Identifier.Text;

                                var scope = node
                                    .Ancestors().First(x => x is BlockSyntax || x is ArrowExpressionClauseSyntax);

                                var usagesOfVariable = scope.DescendantNodes()
                                    .OfType<IdentifierNameSyntax>()
                                    .Where(x => x.Identifier.Text == variableName)
                                    .Select(x => new
                                    {
                                        Identifier = x,
                                        Symbol = semanticModel.GetSymbolInfo(x).Symbol as ILocalSymbol
                                    })
                                    .Where(x => x.Symbol != null)
                                    .Where(x => x.Symbol.DeclaringSyntaxReferences.Length == 1 && x.Symbol.DeclaringSyntaxReferences[0].GetSyntax() is VariableDeclaratorSyntax variableDeclarator1 && variableDeclarator1.Equals(variableDeclarator))
                                    .Select(x => x.Identifier)
                                    .ToImmutableArray();

                                return usagesOfVariable.All(IsOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods);
                            }
                            else if (node.Parent is MemberAccessExpressionSyntax memberAccess
                                     && memberAccess.Kind() == SyntaxKind.SimpleMemberAccessExpression
                                     && memberAccess.Parent is InvocationExpressionSyntax invocationExpression)
                            {
                                if (semanticModel.GetSymbolInfo(memberAccess.Name).Symbol is IMethodSymbol methodSymbol)
                                {
                                    if (IsAtLeastPureExceptReadLocally(methodSymbol, recursiveState))
                                        return IsOkAsReturnTypeFromMethodThatTakesInCastedNewObjectOrParameter(methodSymbol.ReturnType) ||
                                               IsOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods(invocationExpression);
                                }

                                return false;
                            }
                            else if (node.Parent is ReturnStatementSyntax returnStatement)
                            {
                                return GetLambdaExpressionWhereReturnIsUsed(returnStatement)
                                    .ChainValue(IsOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods)
                                    .ValueOr(false);

                            }
                            else if (node.Parent is LambdaExpressionSyntax lambdaExpressionSyntax)
                            {
                                return IsOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods(lambdaExpressionSyntax);
                            }

                            return true;
                        }

                        if (sourceNode.Parent is CastExpressionSyntax castExpression)
                        {
                            return IsOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods(castExpression);
                        }

                        return IsOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods(sourceNode);
                    }

                    Maybe<LambdaExpressionSyntax> GetLambdaExpressionWhereReturnIsUsed(
                        ReturnStatementSyntax returnStatement)
                    {
                        return returnStatement.Parent.TryCast().To<BlockSyntax>()
                            .ChainValue(x => x.Parent.TryCast().To<LambdaExpressionSyntax>());
                    }

                    bool CurrentMemberOrAccessorDeclarationSetsAnyField()
                    {
                        var currentDeclaration = GetCurrentMemberOrAccessorDeclaration(sourceNode);

                        var identifiers = currentDeclaration.DescendantNodes().OfType<IdentifierNameSyntax>().ToArray();

                        return identifiers
                            .Select(x => new {Identifier = x, Field = semanticModel.GetSymbolInfo(x).Symbol as IFieldSymbol})
                            .Where(x => x.Field != null)
                            .Any(x => IsFieldWrite(x.Identifier, x.Field));
                    }

                    bool legalCastFromNewObjectOrParameter = false;


                    if (srcPurity.HasValueEquals(PurityType.PureExceptReadLocally) &&
                        destPurity.HasValueEquals(PurityType.Pure))
                    {
                        if(!CurrentMemberOrAccessorDeclarationSetsAnyField())
                        {
                            if (IsSourceNodeANewObject() || IsSourceNodeAParameter() || (IsSourceNodeAccessToLocalState() && (purityType == PurityType.PureExceptReadLocally || purityType == PurityType.PureExceptLocally)))
                            {
                                if (IsSourceNodeOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods())
                                    legalCastFromNewObjectOrParameter = true;
                            }
                        }
                    }
                    else if (srcPurity.HasValueEquals(PurityType.PureExceptLocally) &&
                             destPurity.HasValueIn(PurityType.Pure, PurityType.PureExceptReadLocally))
                    {
                        if (!CurrentMemberOrAccessorDeclarationSetsAnyField())
                        {
                            if (IsSourceNodeANewObject() || (IsSourceNodeAccessToLocalState() && purityType == PurityType.PureExceptLocally ))
                            {
                                if (IsSourceNodeOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods())
                                    legalCastFromNewObjectOrParameter = true;
                            }
                        }
                    }

                    if(!legalCastFromNewObjectOrParameter)
                    {
                        if (!IsGreaterOrEqaulPurity(
                            srcPurity,
                            destPurity))
                        {
                            problems.Add(destMethod);
                        }
                    }
                }
            }

            if (problems.Any())
                return new CastPurityResult.Impure(
                    "Error casting. Relevent methods: " + Environment.NewLine +
                    string.Join(Environment.NewLine, problems.Select(x => x.Name)));

            return new CastPurityResult.Pure();

            Maybe<IMethodSymbol> GetMatchingMethod(IMethodSymbol method, ITypeSymbol type)
            {
                //Arrays do not implement IEnumerable<T> except for at runtime
                //To fix this problem, I use the implementation of the interface method of List<T>
                if (type is IArrayTypeSymbol arrayType)
                {
                    if (method.ContainingType.IsGenericType &&
                        method.ContainingType.OriginalDefinition.Equals(genericIenumerableType) &&
                        method.OriginalDefinition.Equals(genericGetEnumeratorMethod))
                    {
                        return GetMatchingMethod(method, genericListType.Construct(arrayType.ElementType));
                    }
                }

                if (method.ContainingType.TypeKind == TypeKind.Interface)
                {
                    return (type.FindImplementationForInterfaceMember(method) as IMethodSymbol).ToMaybe();
                }

                var typeMostDerivedMethods = Utils.RemoveOverriddenMethods(Utils.GetAllMethods(type).ToArray());

                foreach (var typeMethod in typeMostDerivedMethods)
                {
                    if (typeMethod.Equals(method))
                        return typeMethod.ToMaybe();

                    if (UltimatlyOverrides(typeMethod, method))
                        return typeMethod.ToMaybe();

                    if (UltimatlyOverrides(method, typeMethod))
                        return typeMethod.ToMaybe();
                }

                return Maybe.NoValue;
            }

            bool UltimatlyOverrides(IMethodSymbol method, IMethodSymbol overridden)
            {
                if (method.OverriddenMethod == null)
                    return false;

                return method.OverriddenMethod.Equals(overridden) ||
                       UltimatlyOverrides(method.OverriddenMethod, overridden);
            }
        }

        private bool IsAPropertyWithAnInstancePureExceptReadLocallyGetter(SyntaxNode syntaxNode,
            RecursiveState recursiveState)
        {
            if(syntaxNode is IdentifierNameSyntax identifier)
            {
                if (semanticModel.GetSymbolInfo(identifier).Symbol is IPropertySymbol
                        propertySymbol && !propertySymbol.IsStatic && propertySymbol.GetMethod != null)
                {
                    if (IsMethodPure(propertySymbol.GetMethod,
                        recursiveState,
                        PurityType.PureExceptReadLocally))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static SyntaxNode GetCurrentMemberOrAccessorDeclaration(SyntaxNode node)
        {
            return node.AncestorsAndSelf().First(x =>
                x is AccessorDeclarationSyntax
                || x is MethodDeclarationSyntax
                || x is PropertyDeclarationSyntax
                || x is IndexerDeclarationSyntax);
        }

        public static bool IsGreaterOrEqaulPurity(Maybe<PurityType> type, Maybe<PurityType> than)
        {
            if (type.HasNoValue && than.HasNoValue)
                return true;

            if (type.HasValue && than.HasNoValue)
                return true;

            if (type.HasNoValue && than.HasValue)
                return false;

            return IsGreaterOrEqaulPurity(type.GetValue(), than.GetValue());
        }

        public static bool IsGreaterOrEqaulPurity(PurityType type, PurityType than)
        {
            if (type == PurityType.Pure)
                return true;

            if (type == PurityType.PureExceptReadLocally)
                return than == PurityType.PureExceptReadLocally || than == PurityType.PureExceptLocally;

            return than == PurityType.PureExceptLocally;
        }

        private IEnumerable<Impurity> GetImpurities2(ObjectCreationExpressionSyntax node,
            RecursiveState recursiveState)
        {
            if (semanticModel.GetSymbolInfo(node.Type).Symbol is INamedTypeSymbol symbol)
            {
                if (!IsTypePureForConstruction(symbol, recursiveState))
                {
                    yield return new Impurity(node, "Constructed object is not pure");
                }
            }
        }

        private bool IsTypePureForConstruction(INamedTypeSymbol symbol, RecursiveState recursiveState)
        {
            if (recursiveState.ConstructedTypesInStack.Contains(symbol))
                return true;

            var modifiedRecursiveState = recursiveState.AddConstructedType(symbol);

            if (Utils.SymbolHasAssumeIsPureAttribute(symbol))
                return true;

            if (!Utils.GetAllMethods(symbol)
                    .Where(x =>
                        x.MethodKind == MethodKind.Constructor ||
                        x.MethodKind == MethodKind.StaticConstructor)
                    .All(method => IsMethodPure(method, modifiedRecursiveState)))
                return false;

            if (symbol.IsInCode())
            {
                var semanticModelForType =
                    semanticModel.Compilation.GetSemanticModel(symbol.Locations.First().SourceTree);

                if (AnyImpureFieldInitializer(
                    symbol,
                    semanticModelForType,
                    modifiedRecursiveState))
                    return false;

                if (AnyImpurePropertyInitializer(
                    symbol,
                    semanticModelForType,
                    modifiedRecursiveState))
                    return false;
            }

            var baseType = symbol.BaseType;

            if (baseType != null)
            {
                while (!baseType.Equals(objectType))
                {
                    if (baseType.IsInCode())
                    {
                        var semanticModelForType =
                            semanticModel.Compilation.GetSemanticModel(baseType.Locations.First().SourceTree);

                        if (AnyImpureFieldInitializer(
                            baseType,
                            semanticModelForType,
                            modifiedRecursiveState))
                            return false;

                        if (AnyImpurePropertyInitializer(
                            baseType,
                            semanticModelForType,
                            modifiedRecursiveState))
                            return false;
                    }

                    baseType = baseType.BaseType;
                }
            }

            return true;
        }

        private bool AnyImpurePropertyInitializer(INamedTypeSymbol symbol,
            SemanticModel semanticModel,
            RecursiveState recursiveState)
        {
            return
                symbol.Locations
                    .Select(x => x.SourceTree.GetRoot().FindNode(x.SourceSpan))
                    .OfType<TypeDeclarationSyntax>()
                    .Any(x => Utils.AnyImpurePropertyInitializer(x, semanticModel, knownReturnsNewObjectMethods, recursiveState));
        }

        private bool AnyImpureFieldInitializer(
            INamedTypeSymbol symbol,
            SemanticModel semanticModel,
            RecursiveState recursiveState)
        {
            return
                symbol.Locations.Select(x => x.SourceTree.GetRoot().FindNode(x.SourceSpan))
                    .OfType<TypeDeclarationSyntax>()
                    .Any(x => Utils.AnyImpureFieldInitializer(x, semanticModel, knownReturnsNewObjectMethods, recursiveState));
        }

        private bool IsParameter(SyntaxNode node)
        {
            var accessedSymbol = semanticModel.GetSymbolInfo(node).Symbol;

            if (accessedSymbol is IParameterSymbol)
                return true;

            if (node is MemberAccessExpressionSyntax parentExpression)
                return IsParameter(parentExpression.Expression);

            if (!(node is IdentifierNameSyntax identifier))
                return false;

            var identifierSymbol = semanticModel.GetSymbolInfo(identifier).Symbol;

            if (!(identifierSymbol is ILocalSymbol local))
                return false;

            var method = node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrNoValue();

            if (method.HasNoValue)
                return false;

            return Utils.FindValuesAssignedToVariable(
                semanticModel, local, method.GetValue().Body)
                .All(IsParameter);
        }

        private bool IsParameterBasedAccess(ExpressionSyntax node)
        {
            if (node.Parent is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name == node
                && IsParameter(memberAccess.Expression))
                return true;

            if (node is ElementAccessExpressionSyntax elementAccess
                && IsParameter(elementAccess.Expression))
                return true;

            return false;
        }

        private CastPurityResult ContainsImpureCast(SyntaxNode node, RecursiveState recursiveState)
        {
            var typeInfo = semanticModel.GetTypeInfo(node);

            if (typeInfo.Type != null && typeInfo.ConvertedType != null &&
                !typeInfo.Type.Equals(typeInfo.ConvertedType))
            {
                var sourceType = typeInfo.Type;
                var destinationType = typeInfo.ConvertedType;

                return IsImpureCast(sourceType, destinationType, recursiveState, node);

            }

            return new CastPurityResult.Pure();
        }

        private IEnumerable<Impurity> GetImpurities3(IdentifierNameSyntax node,
            RecursiveState recursiveState)
        {
            var symbol = semanticModel.GetSymbolInfo(node);

            if (symbol.Symbol is IFieldSymbol field)
            {
                return GetImpuritiesForFieldAccess(node, field);
            }

            if (symbol.Symbol is IPropertySymbol property)
            {
                return GetImpuritiesForPropertyAccess(node, property, recursiveState);
            }

            if (symbol.Symbol is IMethodSymbol method)
            {
                return GetImpuritiesForMethodAccess(node, method, recursiveState);
            }

            if (symbol.Symbol is IEventSymbol)
            {
                return new []{new Impurity(node, "Event access")};
            }

            return Enumerable.Empty<Impurity>();
        }

        private IEnumerable<Impurity> GetImpuritiesForFieldAccess(IdentifierNameSyntax node, IFieldSymbol fieldSymbol)
        {
            if (!(fieldSymbol.IsReadOnly || fieldSymbol.IsConst))
            {
                if (!Utils.IsAccessOnNewlyCreatedObject(knownReturnsNewObjectMethods, semanticModel, node))
                {
                    var constructorWhereIdentifierIsUsed =
                        node.Ancestors()
                            .OfType<ConstructorDeclarationSyntax>()
                            .FirstOrDefault();

                    bool accessingFieldFromMatchingConstructor = false;


                    if (constructorWhereIdentifierIsUsed != null)
                    {
                        var constructorSymbol = semanticModel.GetDeclaredSymbol(constructorWhereIdentifierIsUsed);

                        var currentType = constructorSymbol.ContainingType;

                        if (fieldSymbol.ContainingType == currentType && fieldSymbol.IsStatic == constructorSymbol.IsStatic)
                        {
                            accessingFieldFromMatchingConstructor = true;
                        }
                    }

                    bool accessingLocalFieldLegally = false;

                    if (purityType == PurityType.PureExceptLocally || purityType == PurityType.PureExceptReadLocally)
                    {
                        var isAccessingLocalData =
                            IsDirectAccessToInstanceFieldOrAccessOfThisOrASeriesOfFieldAccesses(
                                node, fieldSymbol);

                        accessingLocalFieldLegally =
                            isAccessingLocalData
                            && (purityType == PurityType.PureExceptLocally || !Utils.GetUsage(node).IsWrite());
                    }

                    if (!accessingFieldFromMatchingConstructor && !accessingLocalFieldLegally)
                    {
                        var usage = Utils.GetUsage(node);

                        if (usage.IsWrite())
                        {
                             yield return new Impurity(node, "Write access to field");
                        }
                        else
                        {
                            if (!IsParameterBasedAccess(node))
                            {
                                yield return
                                    new Impurity(node, "Read access to non-readonly and non-const and non-input parameter based field");
                            }
                        }
                    }
                }
            }
        }

        private bool IsFieldWrite(IdentifierNameSyntax node, IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.IsReadOnly || fieldSymbol.IsConst)
                return false;

            return Utils.GetUsage(node).IsWrite();
        }


        private bool IsAccessingLocalField(IFieldSymbol fieldSymbol, MemberDeclarationSyntax memberThatIsTryingToAccessField)
        {
            var memberSymbol = semanticModel.GetDeclaredSymbol(memberThatIsTryingToAccessField);

            return IsAccessingLocalField(fieldSymbol, memberSymbol);
        }

        private static bool IsAccessingLocalField(IFieldSymbol fieldSymbol, ISymbol memberThatIsTryingToAccessField)
        {
            var currentType = memberThatIsTryingToAccessField.ContainingType;

            return fieldSymbol.ContainingType == currentType && !fieldSymbol.IsStatic;
        }

        private IEnumerable<Impurity> GetImpuritiesForPropertyAccess(ExpressionSyntax node,
            IPropertySymbol propertySymbol, RecursiveState recursiveState)
        {
            var usage = Utils.GetUsage(node);

            var method = usage.IsWrite() ? propertySymbol.SetMethod : propertySymbol.GetMethod;

            if (method != null)
            {
                return GetImpuritiesForMethodAccess(node, method, recursiveState);
            }

            return Enumerable.Empty<Impurity>();
        }

        private IEnumerable<Impurity> GetImpuritiesForMethodAccess(ExpressionSyntax node, IMethodSymbol method,
            RecursiveState recursiveState)
        {
            bool IsAccessingLocalMethod(ExpressionSyntax node1)
            {
                if (node1.Parent is InvocationExpressionSyntax) //local method
                {
                    return true;
                }

                if (node1.Parent is MemberAccessExpressionSyntax memberAccess)
                {
                    return this.IsThisExpressionOrASeriesOfFieldAccesses(memberAccess.Expression);
                }
                else if (node1 is ElementAccessExpressionSyntax)
                {
                    return true;
                }
                else
                {
                    var operation = semanticModel.GetOperation(node1);

                    if (operation is IPropertyReferenceOperation propertyReferenceOperation &&
                        propertyReferenceOperation.Instance.Kind == OperationKind.InstanceReference)
                    {
                        return true;
                    }
                }

                return false;
            }

            PurityType acceptedPurityType = PurityType.Pure;

            if (purityType == PurityType.PureExceptLocally || purityType == PurityType.PureExceptReadLocally)
            {
                if (IsAccessingLocalMethod(node))
                    acceptedPurityType = purityType;
            }
            else
            {
                if (Utils.IsAccessOnNewlyCreatedObject(knownReturnsNewObjectMethods, semanticModel, node))
                {
                    acceptedPurityType = PurityType.PureExceptLocally;
                }
                else if (IsParameterBasedAccess(node))
                {
                    acceptedPurityType = PurityType.PureExceptReadLocally;
                }
            }

            if (!IsMethodPure(method, recursiveState, acceptedPurityType))
            {
                yield return new Impurity(node, "Method is impure");
            }
        }

        private bool IsDirectAccessToInstanceFieldOrAccessOfThisOrASeriesOfFieldAccesses(IdentifierNameSyntax node1, IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.IsStatic)
                return false;

            if (!(node1.Parent is MemberAccessExpressionSyntax memberAccess))
            {
                return true;
            }

            return IsThisExpressionOrASeriesOfFieldAccesses(memberAccess.Expression);
        }

        private bool IsThisExpressionOrASeriesOfFieldAccesses(ExpressionSyntax node1)
        {

            if (node1.Kind() == SyntaxKind.ThisExpression)
            {
                return true;
            }

            if (node1 is IdentifierNameSyntax identifier)
            {
                return semanticModel.GetSymbolInfo(identifier).Symbol is IFieldSymbol field1 && !field1.IsStatic;
            }

            if (node1 is MemberAccessExpressionSyntax memberAccess)
            {
                return semanticModel.GetSymbolInfo(memberAccess.Name).Symbol is IFieldSymbol field && !field.IsStatic &&
                       IsThisExpressionOrASeriesOfFieldAccesses(memberAccess.Expression);
            }

            return false;
        }

        private IEnumerable<Impurity> GetImpurities4(
            BinaryExpressionSyntax node,
            RecursiveState recursiveState)
        {
            if (node.OperatorToken.Kind() ==  SyntaxKind.AmpersandAmpersandToken)
            {
                var leftNodeType = semanticModel.GetTypeInfo(node.Left).Type;

                var falseOperator = leftNodeType.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(x => x.MethodKind == MethodKind.UserDefinedOperator)
                    .Where(x => x.Name == "op_False")
                    .FirstOrNoValue();

                if (falseOperator.HasValue)
                {
                    if (!IsMethodPure(falseOperator.GetValue(), recursiveState))
                    {
                        yield return new Impurity(node, "False operator is impure");
                    }
                }
            }

            if (node.OperatorToken.Kind() == SyntaxKind.BarBarToken)
            {
                var leftNodeType = semanticModel.GetTypeInfo(node.Left).Type;

                var falseOperator = leftNodeType.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(x => x.MethodKind == MethodKind.UserDefinedOperator)
                    .Where(x => x.Name == "op_True")
                    .FirstOrNoValue();

                if (falseOperator.HasValue)
                {
                    if (!IsMethodPure(falseOperator.GetValue(), recursiveState))
                    {
                        yield return new Impurity(node, "True operator is impure");
                    }
                }
            }

            if (semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol method)
            {
                if (!IsMethodPure(method, recursiveState))
                {
                    yield return new Impurity(node, "Operator is impure");
                }
            }
        }

        private IEnumerable<Impurity> GetImpurities5(AssignmentExpressionSyntax node,
            RecursiveState recursiveState)
        {
            var kind = node.Kind();

            if (kind == SyntaxKind.AddAssignmentExpression ||
                kind == SyntaxKind.SubtractAssignmentExpression ||
                kind == SyntaxKind.AndAssignmentExpression ||
                kind == SyntaxKind.DivideAssignmentExpression ||
                kind == SyntaxKind.ExclusiveOrAssignmentExpression ||
                kind == SyntaxKind.ModuloAssignmentExpression ||
                kind == SyntaxKind.OrAssignmentExpression ||
                kind == SyntaxKind.MultiplyAssignmentExpression ||
                kind == SyntaxKind.LeftShiftAssignmentExpression ||
                kind == SyntaxKind.RightShiftAssignmentExpression)
            {
                if (semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol method)
                {
                    if (!IsMethodPure(method, recursiveState))
                    {
                        yield return new Impurity(node, "Operator is impure");
                    }
                }
            }
        }

        private IEnumerable<Impurity> GetImpurities6(ElementAccessExpressionSyntax node,
            RecursiveState recursiveState)
        {
            var type = semanticModel.GetTypeInfo(node.Expression).Type;

            if (type?.TypeKind == TypeKind.Array)
            {
                //Arrays element access in .NET is not a property or a method. Here I assume that
                //The explicit property IList.Item is being accessed.
                //This allows me to delegate to the code that handles properties
                foreach (var imp in GetImpuritiesForPropertyAccess(node, arrayIListItemProperty, recursiveState))
                    yield return imp;
            }
            else
            {
                var symbol = semanticModel.GetSymbolInfo(node).Symbol;

                if (symbol is IPropertySymbol propertySymbol)
                {
                    foreach (var imp in GetImpuritiesForPropertyAccess(node, propertySymbol, recursiveState))
                        yield return imp;
                }
            }
        }

        private Maybe<PurityType> GetMethodPurityType(
            IMethodSymbol method,
            RecursiveState recursiveState)
        {
            if (IsMethodPure(method, recursiveState, PurityType.Pure))
                return PurityType.Pure;

            if (IsMethodPure(method, recursiveState, PurityType.PureExceptReadLocally))
                return PurityType.PureExceptReadLocally;

            if (IsMethodPure(method, recursiveState, PurityType.PureExceptLocally))
                return PurityType.PureExceptLocally;

            return Maybe.NoValue;
        }

        private bool IsAnyKindOfPure(IMethodSymbol method,
            RecursiveState recursiveState)
        {
            return IsMethodPure(method, recursiveState, PurityType.PureExceptLocally);
        }

        private bool IsAtLeastPureExceptReadLocally(IMethodSymbol method,
            RecursiveState recursiveState)
        {
            return IsMethodPure(method, recursiveState, PurityType.PureExceptReadLocally);
        }

        private bool IsMethodPure(
            IMethodSymbol method,
            RecursiveState recursiveState,
            PurityType purityType = PurityType.Pure)
        {
            if (recursiveState.MethodsInStack.Contains((method, purityType)))
                return true;

            if (method.IsAbstract)
                return true;

            if (Utils.SymbolHasAssumeIsPureAttribute(method))
                return true;

            if (Utils.SymbolHasAssumeIsPureAttribute(method.ContainingType))
                return true;

            if (purityType == PurityType.PureExceptReadLocally || purityType == PurityType.PureExceptLocally)
            {
                if (Utils.SymbolHasIsPureExceptReadLocallyAttribute(method))
                    return true;
            }

            if (purityType == PurityType.PureExceptLocally)
            {
                if (Utils.SymbolHasIsPureExceptLocallyAttribute(method))
                    return true;
            }

            if (Utils.SymbolHasIsPureAttribute(method))
                return true;

            if (Utils.SymbolHasIsPureAttribute(method.ContainingType))
                return true;

            if (method.IsInCode())
            {
                if (method.IsImplicitlyDeclared)
                    return true;

                var location = method.Locations.First();

                var locationSourceTree = location.SourceTree;

                var methodNode = locationSourceTree.GetRoot().FindNode(location.SourceSpan);

                if (methodNode is AccessorDeclarationSyntax accessor)
                {
                    if (accessor.Body == null && accessor.ExpressionBody == null) //Auto property
                    {
                        var property = (IPropertySymbol)method.AssociatedSymbol;


                        if (accessor.Kind() == SyntaxKind.SetAccessorDeclaration)
                        {
                            return
                                property.IsReadOnly //This happens in constructors
                                || purityType == PurityType.PureExceptLocally;
                        }
                        if (accessor.Kind() == SyntaxKind.GetAccessorDeclaration)
                        {
                            return property.IsReadOnly || purityType == PurityType.PureExceptLocally || purityType == PurityType.PureExceptReadLocally;
                        }
                    }
                }

                var imp = Utils.GetImpurities(
                    methodNode,
                    semanticModel.Compilation.GetSemanticModel(locationSourceTree),
                    knownReturnsNewObjectMethods,
                    recursiveState.AddMethod(method, purityType),
                    purityType);

                if (imp.Any()) return false;
            }
            else
            {
                if (!IsKnownPureMethod(method, purityType)) return false;
            }

            return true;
        }

        private Dictionary<string, HashSet<MethodDescriptor>> GetKnownPureMethods()
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

        private Dictionary<string, HashSet<MethodDescriptor>> GetKnownPureExceptLocallyMethods()
        {
            var pureMethodsExceptLocallyFileContents =
                Resources.PureExceptLocallyMethods
                    +Environment.NewLine
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

        private Dictionary<string, HashSet<MethodDescriptor>> GetKnownPureExceptReadLocallyMethods()
        {
            var pureMethodsExceptLocallyFileContents =
                Resources.PureExceptReadLocallyMethods
                    +Environment.NewLine
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

        private HashSet<INamedTypeSymbol> GetKnownPureTypes()
        {
            var pureTypesFileContents =
                Resources.PureTypes
                + Environment.NewLine
                + PurityAnalyzerAnalyzer
                    .CustomPureTypesFilename.ChainValue(File.ReadAllText)
                    .ValueOr("");

            var pureTypes =
                pureTypesFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            return new HashSet<INamedTypeSymbol>(pureTypes.Select(x => semanticModel.Compilation.GetTypeByMetadataName(x)));
        }

        private bool IsKnownPureMethod(IMethodSymbol method, PurityType purityType = PurityType.Pure)
        {
            if (method.ContainingType.TypeKind == TypeKind.Delegate)
                return true;

            if (method.ContainingType.IsGenericType)
            {
                if (knownPureTypes.Contains(method.ContainingType.ConstructedFrom))
                    return true;
            }
            else
            {
                if (knownPureTypes.Contains(method.ContainingType))
                    return true;
            }


            if (knownPureMethods.TryGetValue(Utils.GetFullMetaDataName(method.ContainingType), out var pureMethods) &&
                pureMethods.AnyMatches(method))
            {
                return true;
            }

            if (purityType == PurityType.PureExceptReadLocally || purityType == PurityType.PureExceptLocally)
            {
                if (knownPureExceptReadLocallyMethods.TryGetValue(
                        Utils.GetFullMetaDataName(method.ContainingType),
                        out var pureExceptReadLocallyMethods) &&
                    pureExceptReadLocallyMethods.AnyMatches(method))
                {
                    return true;
                }
            }

            if(purityType == PurityType.PureExceptLocally)
            {
                if (knownPureExceptLocallyMethods.TryGetValue(
                        Utils.GetFullMetaDataName(method.ContainingType),
                        out var pureExceptLocallyMethods) &&
                    pureExceptLocallyMethods.AnyMatches(method))
                {
                    return true;
                }
            }




            return false;
        }
    }
}