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
    public class KnownSymbols
    {
        public KnownSymbols(
            Dictionary<string, HashSet<MethodDescriptor>> knownPureMethods,
            Dictionary<string, HashSet<MethodDescriptor>> knownPureExceptLocallyMethods,
            Dictionary<string, HashSet<MethodDescriptor>> knownPureExceptReadLocallyMethods,
            Dictionary<string, HashSet<MethodDescriptor>> knownReturnsNewObjectMethods,
            HashSet<INamedTypeSymbol> knownPureTypes,
            Dictionary<string, Dictionary<MethodDescriptor, string[]>> knownNotUsedAsObjectMethodTypeParameters,
            Dictionary<string, string[]> knownNotUsedAsObjectClassTypeParameters,
            Dictionary<string, HashSet<MethodDescriptor>> knownPureOnInvariantIFormatProviderMethods)
        {
            KnownPureMethods = knownPureMethods;
            KnownPureExceptLocallyMethods = knownPureExceptLocallyMethods;
            KnownPureExceptReadLocallyMethods = knownPureExceptReadLocallyMethods;
            KnownReturnsNewObjectMethods = knownReturnsNewObjectMethods;
            KnownPureTypes = knownPureTypes;
            KnownNotUsedAsObjectMethodTypeParameters = knownNotUsedAsObjectMethodTypeParameters;
            KnownNotUsedAsObjectClassTypeParameters = knownNotUsedAsObjectClassTypeParameters;
            KnownPureOnInvariantIFormatProviderMethods = knownPureOnInvariantIFormatProviderMethods;
        }

        public Dictionary<string, HashSet<MethodDescriptor>> KnownPureMethods { get; }
        public Dictionary<string, HashSet<MethodDescriptor>> KnownPureExceptLocallyMethods { get; }
        public Dictionary<string, HashSet<MethodDescriptor>> KnownPureExceptReadLocallyMethods { get; }
        public Dictionary<string, HashSet<MethodDescriptor>> KnownReturnsNewObjectMethods { get; }
        public HashSet<INamedTypeSymbol> KnownPureTypes { get; }
        public Dictionary<string, Dictionary<MethodDescriptor, string[]>> KnownNotUsedAsObjectMethodTypeParameters { get; }
        public Dictionary<string, string[]> KnownNotUsedAsObjectClassTypeParameters { get; }

        public Dictionary<string, HashSet<MethodDescriptor>> KnownPureOnInvariantIFormatProviderMethods { get; }
    }

    public class ImpuritiesFinder
    {
        private readonly PurityType purityType;

        private readonly SemanticModel semanticModel;
        private readonly INamedTypeSymbol objectType;
        private readonly IPropertySymbol arrayIListItemProperty;
        private readonly INamedTypeSymbol genericIenumeratorType;
        private readonly INamedTypeSymbol ienumeratorType;
        private readonly INamedTypeSymbol boolType;
        private readonly INamedTypeSymbol idisposableType;
        private readonly IMethodSymbol genericGetEnumeratorMethod;
        private readonly INamedTypeSymbol genericIenumerableType;
        private readonly INamedTypeSymbol genericListType;
        private readonly KnownSymbols knownSymbols;
        private IMethodSymbol[] objectMethodsRelevantToNotUsedAsObject;
        private IMethodSymbol objectToStringMethod;
        private INamedTypeSymbol iformattableType;
        private IMethodSymbol iformattableTypeToStringMethod;
        protected internal Maybe<IMethodSymbol> formattableStringInvariantMethod;

        public ImpuritiesFinder(
            SemanticModel semanticModel,
            PurityType purityType,
            KnownSymbols knownSymbols)
        {
            this.semanticModel = semanticModel;
            this.purityType = purityType;

            
            objectType = semanticModel.Compilation.ObjectType;

            objectToStringMethod = objectType
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Single(x => x.Name == "ToString" && x.Parameters.Length == 0);

            iformattableType = semanticModel.Compilation.GetTypeByMetadataName(typeof(IFormattable).FullName);

            iformattableTypeToStringMethod = iformattableType
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Single(x => x.Name == "ToString" && x.Parameters.Length == 2);

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

            objectMethodsRelevantToNotUsedAsObject = TypeParametersUsedAsObjectsModule.GetObjectMethodsRelevantToCastingFromGenericTypeParameters(semanticModel);

            formattableStringInvariantMethod =
                semanticModel.Compilation.GetTypeByMetadataName(typeof(FormattableString).FullName)
                .ToMaybe() //FormattableString does not exist in all runtimes. E.g. .NET framework < 4.6
                .ChainValue(x => x.GetMembers("Invariant").OfType<IMethodSymbol>())
                .ChainValue(x => x.FirstOrNoValue());

            this.knownSymbols = knownSymbols;
        }

        public IEnumerable<Impurity> GetImpurities(
            SyntaxNode node,
            RecursiveState recursiveState,
            Maybe<PureLambdaConfig> pureLambdaConfig)
        {
            var allNodes = node.DescendantNodesAndSelf();

            foreach (var subNode in allNodes)
            {
                if(!IsAnInterpolatedStringExpressionThatIsPassedToFormattableStringInvariant(subNode))
                {
                    if (ContainsImpureCast(subNode, recursiveState) is CastPurityResult.Impure impure)
                    {
                        yield return new Impurity(subNode, "Cast is impure" + Environment.NewLine + impure.Reason);
                    }
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
                else if (subNode is SimpleNameSyntax identifierName)
                {
                    foreach (var impurity in GetImpurities3(identifierName, recursiveState))
                        yield return impurity;

                    if(pureLambdaConfig.HasValue)
                    {
                        var potentialIssue =
                            GetPotentialLambdaRelatedIssue(
                                identifierName,
                                pureLambdaConfig.GetValue());

                        if (potentialIssue.HasValue)
                            yield return potentialIssue.GetValue();
                    }
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

                var operation = semanticModel.GetOperation(subNode);

                if (operation is IInvocationOperation invocationOperation && invocationOperation.TargetMethod.ContainingType.TypeKind != TypeKind.Delegate)
                {
                    foreach (var impurity in HandleTypeParametersForMethodInvocation(
                        invocationOperation,
                        recursiveState))
                        yield return impurity;

                }
                else if (operation is IInterpolatedStringOperation interpolatedStringOperation)
                {
                    if (semanticModel.GetTypeInfo(subNode).ConvertedType.SpecialType == SpecialType.System_String)
                    {
                        foreach (var impurity in DetectImpuritiesInInterpolatedExpressions(
                            interpolatedStringOperation,
                            recursiveState,
                            false))
                            yield return impurity;
                    }
                }
            }
        }

        

        private IEnumerable<Impurity> DetectImpuritiesInInterpolatedExpressions(
            IInterpolatedStringOperation interpolatedStringOperation,
            RecursiveState recursiveState,
            bool acceptIFormattableToStringMethodsThatArePureOnInvariantIFormatProvider)
        {
            foreach (var interpolatedExpression in interpolatedStringOperation.Parts.OfType<IInterpolationOperation>())
            {
                var type = interpolatedExpression.Expression.Type;

                var implementsIFormattable = type.AllInterfaces.Contains(iformattableType);

                if (!implementsIFormattable && type.IsReferenceType && !type.IsSealed)
                {
                    yield return new Impurity(interpolatedStringOperation.Syntax, "Cannot guarantee that the actually type of the expression does not contain an impure IFormattable.ToString method on this non-sealed class");
                    continue;
                }

                if (implementsIFormattable)
                {
                    var toStringMethodOnType = GetMatchingMethod(iformattableTypeToStringMethod, type)
                        .ValueOrThrow("Unexpected: cannot find ToString method on type");

                    if (acceptIFormattableToStringMethodsThatArePureOnInvariantIFormatProvider)
                    {
                        if (!knownSymbols.KnownPureOnInvariantIFormatProviderMethods
                                .TryGetValue(
                                    Utils.GetFullMetaDataName(toStringMethodOnType.ContainingType),
                                    out var methods) || !methods.AnyMatches(toStringMethodOnType))
                        {
                            if (!IsMethodPure(knownSymbols, semanticModel, toStringMethodOnType, recursiveState))
                            {
                                yield return new Impurity(interpolatedStringOperation.Syntax,
                                    "ToString method is impure");
                            }
                        }
                    }
                    else
                    {
                        if (!IsMethodPure(knownSymbols, semanticModel, toStringMethodOnType, recursiveState))
                        {
                            yield return new Impurity(interpolatedStringOperation.Syntax, "ToString method is impure");
                        }
                    }

                }
                else
                {
                    var toStringMethodOnType = GetMatchingMethod(objectToStringMethod, type)
                        .ValueOrThrow("Unexpected: cannot find ToString method on type");

                    if (!IsMethodPure(knownSymbols, semanticModel, toStringMethodOnType, recursiveState))
                    {
                        yield return new Impurity(interpolatedStringOperation.Syntax, "ToString method is impure");
                    }
                }
 

            }
        }

        private Maybe<Impurity> GetPotentialLambdaRelatedIssue(
            SimpleNameSyntax identifierName,
            PureLambdaConfig pureLambdaConfig)
        {
            var scope = pureLambdaConfig.LambdaScope;

            var symbol = semanticModel.GetSymbolInfo(identifierName).Symbol;

            if (symbol is ILocalSymbol variable)
            {
                foreach (var variableReference in variable.DeclaringSyntaxReferences)
                {
                    var variableSyntax = variableReference.GetSyntax();

                    if (!scope.Contains(variableSyntax))
                    {
                        if (!Utils.GetUsage(identifierName).IsWrite())
                        {
                            var methodBody =
                                Utils.GetBodyOfMethodThatContainsExpression(variableSyntax);
                            if (methodBody.HasValue)
                            {
                                var valuesAssignedToVariable = Utils.FindValuesAssignedToVariable(semanticModel, variable, methodBody.GetValue());

                                if (valuesAssignedToVariable.All(x =>
                                    x is InvocationExpressionSyntax invocation &&
                                    Utils.IsPureLambdaMethodInvocation(
                                        semanticModel, pureLambdaConfig.PureLambdaMethodFullClassName,
                                        pureLambdaConfig.PureLambdaMethodName, invocation)))
                                {
                                    continue;
                                }
                            }
                        }

                        return new Impurity(identifierName, "Unacceptable access to variable");
                    }
                }
            }
            else if (symbol is IParameterSymbol parameter)
            {
                if (parameter.DeclaringSyntaxReferences.Any(x => !scope.Contains(x.GetSyntax())))
                {
                    return new Impurity(identifierName, "Unacceptable access to parameter");
                }
            }

            return Maybe.NoValue;
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
                    if (!IsMethodPure(knownSymbols, semanticModel, unaryOperation.OperatorMethod, recursiveState))
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
                if (!IsMethodPure(knownSymbols, semanticModel, method, recursiveState))
                {
                    yield return new Impurity(node, "Operator is impure");
                }
            }
        }

        private IEnumerable<Impurity> GetImpurities9(PostfixUnaryExpressionSyntax node, RecursiveState recursiveState)
        {
            if (semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol method)
            {
                if (!IsMethodPure(knownSymbols, semanticModel, method, recursiveState))
                {
                    yield return new Impurity(node, "Operator is impure");
                }
            }
        }

        public IEnumerable<Impurity> GetImpurities7(
            CommonForEachStatementSyntax forEachStatement,
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
                    if (!IsMethodPure(knownSymbols, semanticModel, getEnumeratorMethod.GetValue(), recursiveState))
                        yield return new Impurity(forEachStatement, "GetEnumerator method is impure");


                    var returnType = getEnumeratorMethod.GetValue().ReturnType;

                    var acceptedPurityType =
                        Utils
                            .ChangeAcceptedPurityTypeBasedOnWhetherExpressionRepresentsANewObjectOrParameterBasedExpression(
                                PurityType.Pure, forEachStatement.Expression, knownSymbols, semanticModel, recursiveState);

                    var currentPropertyGetter =
                        returnType.GetMembers("Current")
                            .OfType<IPropertySymbol>()
                            .FirstOrNoValue()
                            .ChainValue(x => x.GetMethod);

                    if (currentPropertyGetter.HasValue)
                    {
                        if (!IsMethodPure(knownSymbols, semanticModel, currentPropertyGetter.GetValue(), recursiveState, acceptedPurityType))
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
                        if (!IsMethodPure(knownSymbols, semanticModel, moveNextMethod.GetValue(), recursiveState,acceptedPurityType))
                            yield return new Impurity(forEachStatement, "MoveNext method is impure");
                    }

                    if (returnType.Interfaces.Any(x => x.Equals(idisposableType)))
                    {
                        if (
                            returnType
                                .FindImplementationForInterfaceMember(
                                    idisposableType.GetMembers("Dispose").First()) is IMethodSymbol disposeMethod)
                        {
                            if (!IsMethodPure(knownSymbols, semanticModel, disposeMethod, recursiveState, acceptedPurityType))
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

            var allDestinationMethods = Utils.GetAllMethods(destinationType, semanticModel.Compilation).ToArray();

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
                        if (IsAnyKindOfPure(destMethod, recursiveState)
                            || IsGenericMethodThatDoesNotUseAnyMethodTypeParametersAsObject(destMethod)
                            || DoesMethodContainingTypesHaveAnyTypeParametersButMethodDoesNotUseThemAsObject(destMethod))
                        {
                            problems.Add(destMethod);
                        }
                    }
                }
                else
                {
                    //TODO: handle class-level type parameters
                    if (IsThereAnIssueCastingAsItRelatesToUsingTypeParametersAsObject(
                        matchingMethodInSourceType.GetValue(), destMethod))
                    {
                        problems.Add(destMethod);
                    }

                    var srcPurity = GetMethodPurityType(semanticModel, knownSymbols, matchingMethodInSourceType.GetValue(), recursiveState);
                    var destPurity = GetMethodPurityType(semanticModel, knownSymbols, destMethod, recursiveState);

                    bool IsSourceNodeANewObject()
                    {
                        return Utils.IsNewlyCreatedObject(semanticModel, sourceNode, knownSymbols, RecursiveIsNewlyCreatedObjectState.Empty(), recursiveState);
                    }

                    bool IsSourceNodeAParameter()
                    {
                        return IsParameter(semanticModel, sourceNode);
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
                        return type.SpecialType == SpecialType.System_Void || Utils.IsPureData(type);
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
                                var scope = node
                                    .Ancestors().First(x => x is BlockSyntax || x is ArrowExpressionClauseSyntax);

                                var usagesOfVariable = GetUsagesOfVariable(variableDeclarator, scope);

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
                            .Select(x => new { Identifier = x, Field = semanticModel.GetSymbolInfo(x).Symbol as IFieldSymbol })
                            .Where(x => x.Field != null)
                            .Any(x => IsFieldWrite(x.Identifier, x.Field));
                    }

                    bool legalCastFromNewObjectOrParameter = false;


                    if (srcPurity.HasValueEquals(PurityType.PureExceptReadLocally) &&
                        destPurity.HasValueEquals(PurityType.Pure))
                    {
                        if (!CurrentMemberOrAccessorDeclarationSetsAnyField())
                        {
                            if (IsSourceNodeANewObject() || IsSourceNodeAParameter() || (IsSourceNodeAccessToLocalState() && (purityType == PurityType.PureExceptReadLocally || purityType == PurityType.PureExceptLocally)))
                            {
                                //TODO: OR a PureExceptLocally method that is invoked on a new obejct. See the List.AddRange(new array) test
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
                            if (IsSourceNodeANewObject() || (IsSourceNodeAccessToLocalState() && purityType == PurityType.PureExceptLocally))
                            {
                                if (IsSourceNodeOnlyUsedAsArgumentToPureOrPureExceptReadLocallyMethods())
                                    legalCastFromNewObjectOrParameter = true;
                            }
                        }
                    }

                    if (!legalCastFromNewObjectOrParameter)
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
                    "Error casting" + Environment.NewLine +
                    "From: " + Utils.GetFullMetaDataName(sourceType) + Environment.NewLine +
                    "To: " + Utils.GetFullMetaDataName(destinationType) + Environment.NewLine +
                    "Relevant methods: " + Environment.NewLine +
                    string.Join(Environment.NewLine, problems.Select(x => Utils.GetFullMetaDataName(x.ContainingType) + "." + x.Name)));

            return new CastPurityResult.Pure();
        }

        private Maybe<IMethodSymbol> GetMatchingMethod(IMethodSymbol method, ITypeSymbol type)
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

            if (type is ITypeParameterSymbol typeParameter)
            {
                if (typeParameter.ConstraintTypes.IsEmpty)
                {
                    return GetMatchingMethod(method, objectType);
                }
                else
                {
                    //TODO: should I also include "object"? Only when !NotUsedAsObject?. Seems not. In one if the tests, the constraint is a class and therefore the GetMatchingMethod called below will include methods form Object
                    return typeParameter.ConstraintTypes.Select(x => GetMatchingMethod(method, x))
                        .GetItemsWithValues().FirstOrNoValue();
                }
            }

            if (method.ContainingType.TypeKind == TypeKind.Interface)
            {
                return (type.FindImplementationForInterfaceMember(method) as IMethodSymbol).ToMaybe();
            }

            if (method.ContainingType.Equals(objectType) && type.TypeKind == TypeKind.Interface)
            {
                return method.ToMaybe();
            }

            var typeMostDerivedMethods =
                Utils.RemoveOverriddenMethods(Utils.GetAllMethods(type, semanticModel.Compilation).ToArray());

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

            bool UltimatlyOverrides(IMethodSymbol methodToCheck, IMethodSymbol overridden)
            {
                if (methodToCheck.OverriddenMethod == null)
                    return false;

                return methodToCheck.OverriddenMethod.Equals(overridden) ||
                       UltimatlyOverrides(methodToCheck.OverriddenMethod, overridden);
            }
        }

        private bool IsGenericMethodThatDoesNotUseAnyMethodTypeParametersAsObject(IMethodSymbol destMethod)
        {
            return (destMethod.IsGenericMethod && !DoesMethodUseAnyMethodTypeParameterAsObject(destMethod));
        }

        private bool DoesMethodUseAnyMethodTypeParameterAsObject(IMethodSymbol method)
        {
            foreach (var typeParameter in method.TypeParameters)
            {
                var methodUsesTypeParameterAsObject = TypeParametersUsedAsObjectsModule.DoesMethodUseTAsObject(
                    method,
                    semanticModel,
                    typeParameter, objectMethodsRelevantToNotUsedAsObject, knownSymbols, RecursiveStateForNotUsedAsObject.Empty);

                if (methodUsesTypeParameterAsObject)
                    return true;
            }

            return false;
        }

        private bool DoesMethodContainingTypesHaveAnyTypeParametersButMethodDoesNotUseThemAsObject(IMethodSymbol method)
        {
            var typeParameters =
                TypeParametersUsedAsObjectsModule.GetTypeParametersAndMatchingArgumentsForClass(method
                    .ContainingType).ToList();

            if (typeParameters.Count == 0)
                return false;

            foreach (var typeParameter in typeParameters)
            {
                var methodUsesTypeParameterAsObject = TypeParametersUsedAsObjectsModule.DoesMethodUseTAsObject(
                    method,
                    semanticModel,
                    typeParameter.typeParameter, objectMethodsRelevantToNotUsedAsObject, knownSymbols, RecursiveStateForNotUsedAsObject.Empty);

                if (methodUsesTypeParameterAsObject)
                    return false;

            }

            return true;
        }


        private bool IsThereAnIssueCastingAsItRelatesToUsingTypeParametersAsObject(IMethodSymbol srcMethod,
            IMethodSymbol destMethod)
        {
            for (int i = 0; i < destMethod.TypeParameters.Length ; i++)
            {
                var dstTypeParameter = destMethod.TypeParameters[i];

                var srcTypeParameter = srcMethod.TypeParameters[i];

                var srcMethodUsesTypeParameterAsObject = TypeParametersUsedAsObjectsModule.DoesMethodUseTAsObject(
                    srcMethod,
                    semanticModel,
                    srcTypeParameter, objectMethodsRelevantToNotUsedAsObject, knownSymbols, RecursiveStateForNotUsedAsObject.Empty);

                if (srcMethodUsesTypeParameterAsObject)
                {
                    var dstMethodUsesTypeParameterAsObject = TypeParametersUsedAsObjectsModule.DoesMethodUseTAsObject(
                        destMethod,
                        semanticModel,
                        dstTypeParameter, objectMethodsRelevantToNotUsedAsObject, knownSymbols, RecursiveStateForNotUsedAsObject.Empty);

                    if (!dstMethodUsesTypeParameterAsObject)
                    {
                        return true;
                    }
                }

            }

            return false;
        }

        private ImmutableArray<IdentifierNameSyntax> GetUsagesOfVariable(VariableDeclaratorSyntax variableDeclarator, SyntaxNode scope)
        {
            var variableName = variableDeclarator.Identifier.Text;

            return scope.DescendantNodes()
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
        }

        private bool IsAPropertyWithAnInstancePureExceptReadLocallyGetter(SyntaxNode syntaxNode,
            RecursiveState recursiveState)
        {
            if (syntaxNode is IdentifierNameSyntax identifier)
            {
                if (semanticModel.GetSymbolInfo(identifier).Symbol is IPropertySymbol
                        propertySymbol && !propertySymbol.IsStatic && propertySymbol.GetMethod != null)
                {
                    if (IsMethodPure(knownSymbols, semanticModel, propertySymbol.GetMethod,
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
            if (semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol symbol)
            {
                if (!IsMethodPure(knownSymbols, semanticModel, symbol, recursiveState))
                {
                    yield return new Impurity(node, "Constructor is not pure");
                }
            }
        }

        public static bool AnyImpurePropertyInitializer(
            KnownSymbols knownSymbols,
            INamedTypeSymbol symbol,
            SemanticModel semanticModel,
            RecursiveState recursiveState,
            InstanceStaticCombination instanceStaticCombination)
        {
            return
                symbol.Locations
                    .Select(x => x.SourceTree.GetRoot().FindNode(x.SourceSpan))
                    .OfType<TypeDeclarationSyntax>()
                    .Any(x => Utils.AnyImpurePropertyInitializer(
                        x,
                        semanticModel,
                        knownSymbols,
                        recursiveState,
                        instanceStaticCombination));
        }

        public static bool AnyImpureFieldInitializer(
            KnownSymbols knownSymbols,
            INamedTypeSymbol symbol,
            SemanticModel semanticModel,
            RecursiveState recursiveState,
            InstanceStaticCombination instanceStaticCombination)
        {
            return
                symbol.Locations.Select(x => x.SourceTree.GetRoot().FindNode(x.SourceSpan))
                    .OfType<TypeDeclarationSyntax>()
                    .Any(x => Utils.AnyImpureFieldInitializer(
                        x,
                        semanticModel,
                        knownSymbols,
                        recursiveState,
                        instanceStaticCombination));
        }

        public static bool IsParameter(SemanticModel semanticModel1, SyntaxNode node)
        {
            var accessedSymbol = semanticModel1.GetSymbolInfo(node).Symbol;

            if (accessedSymbol is IParameterSymbol)
                return true;

            if (node is MemberAccessExpressionSyntax parentExpression)
                return IsParameter(semanticModel1, parentExpression.Expression);

            if (!(node is IdentifierNameSyntax identifier))
                return false;

            var identifierSymbol = semanticModel1.GetSymbolInfo(identifier).Symbol;

            if (!(identifierSymbol is ILocalSymbol local))
                return false;

            var method = node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrNoValue();

            if (method.HasNoValue)
                return false;

            return Utils.FindValuesAssignedToVariable(
                semanticModel1, local, method.GetValue().Body)
                .All(node1 => IsParameter(semanticModel1, node1));
        }

        public static bool IsParameterBasedAccess(SemanticModel semanticModel1, ExpressionSyntax node)
        {
            if (node.Parent is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name == node
                && IsParameter(semanticModel1, memberAccess.Expression))
                return true;

            if (node is ElementAccessExpressionSyntax elementAccess
                && IsParameter(semanticModel1, elementAccess.Expression))
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

        private IEnumerable<Impurity> GetImpurities3(SimpleNameSyntax node,
            RecursiveState recursiveState)
        {
            var symbol = semanticModel.GetSymbolInfo(node);

            if (symbol.Symbol is IFieldSymbol field)
            {
                return GetImpuritiesForFieldAccess(node, field, recursiveState);
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
                return new[] { new Impurity(node, "Event access") };
            }

            return Enumerable.Empty<Impurity>();
        }

        private IEnumerable<Impurity> GetImpuritiesForFieldAccess(SimpleNameSyntax node, IFieldSymbol fieldSymbol,
            RecursiveState recursiveState1)
        {
            if (!(fieldSymbol.IsReadOnly || fieldSymbol.IsConst))
            {
                if (!Utils.IsAccessOnNewlyCreatedObject(knownSymbols, semanticModel, node, recursiveState1))
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
                            if (!IsParameterBasedAccess(semanticModel, node))
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

        private IEnumerable<Impurity> GetImpuritiesForMethodAccess(
            ExpressionSyntax node,
            IMethodSymbol method,
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

            if ((purityType == PurityType.PureExceptLocally || purityType == PurityType.PureExceptReadLocally)
                && IsAccessingLocalMethod(node))
            {
                acceptedPurityType = purityType;
            }
            else
            {
                acceptedPurityType =
                    Utils
                        .ChangeAcceptedPurityTypeBasedOnWhetherExpressionRepresentsAccessOnNewObjectOrParameterBasedAccess(
                            acceptedPurityType, node, knownSymbols, semanticModel, recursiveState);
            }

            if (formattableStringInvariantMethod.HasValueAnd(invariant => invariant.Equals(method)))
            {
                var invocationExpression = node.Parent as InvocationExpressionSyntax ??
                                           node.Parent.Parent as InvocationExpressionSyntax ??
                                           throw new Exception("Unable to find InvocationExpressionSyntax");

                var parameter = invocationExpression.ArgumentList.Arguments.Single().Expression;

                if (parameter is InterpolatedStringExpressionSyntax pp)
                {
                    var impurities = DetectImpuritiesInInterpolatedExpressions(
                        (IInterpolatedStringOperation) semanticModel.GetOperation(pp),
                        recursiveState,
                        true);

                    foreach (var impurity in impurities)
                        yield return impurity;

                    yield break;
                }
            }

            if (!IsMethodPure(knownSymbols, semanticModel, method, recursiveState, acceptedPurityType))
            {
                yield return new Impurity(node, "Method is impure");
            }
        }

        private bool IsAnInterpolatedStringExpressionThatIsPassedToFormattableStringInvariant(SyntaxNode node)
        {
            if (formattableStringInvariantMethod.HasNoValue)
                return false;

            if (!(node is InterpolatedStringExpressionSyntax interpolatedString))
                return false;

            if (!(interpolatedString.Parent is ArgumentSyntax argument))
                return false;

            if (!(argument.Parent is ArgumentListSyntax argList))
                return false;

            if (!(argList.Parent is InvocationExpressionSyntax invocation))
                return false;

            var invokedMethod =
                ((IInvocationOperation) semanticModel.GetOperation(invocation))
                .TargetMethod;

            return invokedMethod.Equals(formattableStringInvariantMethod.GetValue());
        }

        private IEnumerable<Impurity> HandleTypeParametersForMethodInvocation(
            IInvocationOperation operation,
            RecursiveState recursiveState)
        {
            var method = operation.TargetMethod;

            var node = operation.Syntax;
            
            var typeParametersAndMatchingArguments = TypeParametersUsedAsObjectsModule.GetTypeParametersAndMatchingArguments(method).ToList();


            if (typeParametersAndMatchingArguments.Count > 0)
            {
                var objectMethodsRelevantToCastingFromGenericTypeParameters =
                    TypeParametersUsedAsObjectsModule
                        .GetObjectMethodsRelevantToCastingFromGenericTypeParameters(
                            semanticModel);

                for (int i = 0; i < typeParametersAndMatchingArguments.Count; i++)
                {
                    var param = typeParametersAndMatchingArguments[i].typeParameter;

                    var arg = typeParametersAndMatchingArguments[i].argument;

                    var constraintTypes = param.ConstraintTypes;

                    if (constraintTypes.IsEmpty)
                    {
                        //TODO: are static constructors checked when we have Class1<T>.Method1()?
                        if (TypeParametersUsedAsObjectsModule.DoesMethodUseTAsObject_IncludingStaticConstructorsIfRelevant(
                            method,
                            semanticModel,
                            param,
                            objectMethodsRelevantToCastingFromGenericTypeParameters,
                            knownSymbols,
                            //TODO: should I merge RecursiveStateForNotUsedAsObject and RecursiveState, what test will require this?
                            RecursiveStateForNotUsedAsObject.Empty))
                        {
                            constraintTypes = constraintTypes.Add(objectType);
                        }
                    }

                    //TODO: what if there is a constraint of type say ISomething. What happens if T is marked with [NotUsedAsObject]? Should we take that into account when we check the cast?

                    foreach (var constraintType in constraintTypes)
                    {
                        if (IsImpureCast(arg, constraintType, recursiveState, node) is CastPurityResult.Impure impure)
                        {
                            yield return new Impurity(node,
                                "Cast from generic argument type " + arg.Name + " to constraint type " + constraintType.Name +
                                " is impure" + Environment.NewLine + impure.Reason);
                        }
                    }
                }
            }
        }

        private bool IsDirectAccessToInstanceFieldOrAccessOfThisOrASeriesOfFieldAccesses(SimpleNameSyntax node1, IFieldSymbol fieldSymbol)
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
            if (node.OperatorToken.Kind() == SyntaxKind.AmpersandAmpersandToken)
            {
                var leftNodeType = semanticModel.GetTypeInfo(node.Left).Type;

                var falseOperator = leftNodeType.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(x => x.MethodKind == MethodKind.UserDefinedOperator)
                    .Where(x => x.Name == "op_False")
                    .FirstOrNoValue();

                if (falseOperator.HasValue)
                {
                    if (!IsMethodPure(knownSymbols, semanticModel, falseOperator.GetValue(), recursiveState))
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
                    if (!IsMethodPure(knownSymbols, semanticModel, falseOperator.GetValue(), recursiveState))
                    {
                        yield return new Impurity(node, "True operator is impure");
                    }
                }
            }

            if (semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol method)
            {
                if (!IsMethodPure(knownSymbols, semanticModel, method, recursiveState))
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
                    if (!IsMethodPure(knownSymbols, semanticModel, method, recursiveState))
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

        public static Maybe<PurityType> GetMethodPurityType(
            SemanticModel semanticModel1,
            KnownSymbols knownSymbols1,
            IMethodSymbol method,
            RecursiveState recursiveState)
        {
            if (IsMethodPure(knownSymbols1, semanticModel1, method, recursiveState, PurityType.Pure))
                return PurityType.Pure;

            if (IsMethodPure(knownSymbols1, semanticModel1, method, recursiveState, PurityType.PureExceptReadLocally))
                return PurityType.PureExceptReadLocally;

            if (IsMethodPure(knownSymbols1, semanticModel1, method, recursiveState, PurityType.PureExceptLocally))
                return PurityType.PureExceptLocally;

            return Maybe.NoValue;
        }

        private bool IsAnyKindOfPure(IMethodSymbol method,
            RecursiveState recursiveState)
        {
            return IsMethodPure(knownSymbols, semanticModel, method, recursiveState, PurityType.PureExceptLocally);
        }

        private bool IsAtLeastPureExceptReadLocally(IMethodSymbol method,
            RecursiveState recursiveState)
        {
            return IsMethodPure(knownSymbols, semanticModel, method, recursiveState, PurityType.PureExceptReadLocally);
        }

        public static bool IsMethodPure(
            KnownSymbols knownSymbols1,
            SemanticModel semanticModel1,
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


            if (method.MethodKind == MethodKind.Constructor || method.IsStatic)
            {
                var staticConstructor = method.ContainingType.StaticConstructors.FirstOrNoValue();

                if (staticConstructor.HasValue)
                {
                    if (!IsMethodPure(knownSymbols1, semanticModel1, staticConstructor.GetValue(), recursiveState.AddMethod(method, purityType)))
                    {
                        return false;
                    }
                }
            }

            if (method.IsInCode())
            {
                if (method.MethodKind != MethodKind.Constructor && method.MethodKind != MethodKind.StaticConstructor)
                {
                    if (method.IsImplicitlyDeclared)
                        return true;
                }

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

                if (method.MethodKind != MethodKind.Constructor && method.MethodKind != MethodKind.StaticConstructor)
                {
                    return !AnyImpuritiesInMethod();
                }

                var semanticModelForType =
                    Utils.GetSemanticModel(semanticModel1, method.ContainingType.Locations.First().SourceTree);

                var modifiedRecursiveState = recursiveState.AddMethod(method, purityType);

                if (AnyImpureFieldInitializer(knownSymbols1, method.ContainingType,
                    semanticModelForType,
                    modifiedRecursiveState,
                    method.IsStatic
                        ? (InstanceStaticCombination) new InstanceStaticCombination.Static()
                        : new InstanceStaticCombination.Instance()))
                {
                    return false;
                }

                if (AnyImpurePropertyInitializer(knownSymbols1, method.ContainingType,
                    semanticModelForType,
                    modifiedRecursiveState,
                    method.IsStatic
                        ? (InstanceStaticCombination) new InstanceStaticCombination.Static()
                        : new InstanceStaticCombination.Instance()))
                {
                    return false;
                }

                Maybe<IMethodSymbol> GetBaseParameterLessConstructor()
                {
                    return method.ContainingType.BaseType
                        .ToMaybe()
                        .ChainValue(b => b.Constructors.Where(x => x.Parameters.Length == 0).FirstOrNoValue());
                }

                if (method.IsImplicitlyDeclared)
                {
                    return
                        GetBaseParameterLessConstructor()
                            .ChainValue(x => IsMethodPure(knownSymbols1, semanticModel1, x, modifiedRecursiveState))
                            .ValueOr(true);
                }

                ConstructorDeclarationSyntax constructorDeclaration = (ConstructorDeclarationSyntax) methodNode;

                if (constructorDeclaration.Initializer != null)
                {
                    if (semanticModelForType.GetSymbolInfo(constructorDeclaration.Initializer).Symbol is
                        IMethodSymbol calledConstructor)
                    {
                        if (!IsMethodPure(knownSymbols1, semanticModel1, calledConstructor, modifiedRecursiveState))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    var implicitBaseConstructor = GetBaseParameterLessConstructor();

                    if (implicitBaseConstructor.HasValue)
                    {
                        if (!IsMethodPure(knownSymbols1, semanticModel1, implicitBaseConstructor.GetValue(), modifiedRecursiveState))
                        {
                            return false;
                        }
                    }
                }

                return !AnyImpuritiesInMethod();

                bool AnyImpuritiesInMethod()
                {
                    return Utils.GetImpurities(
                        methodNode,
                        Utils.GetSemanticModel(semanticModel1, locationSourceTree),
                        knownSymbols1,
                        recursiveState.AddMethod(method, purityType),
                        purityType).Any();
                }
            }

            return IsKnownPureMethod(knownSymbols1, method, purityType);
        }

        public static bool IsKnownPureMethod(KnownSymbols knownSymbols1, IMethodSymbol method, PurityType purityType = PurityType.Pure)
        {
            if (method.ContainingType.TypeKind == TypeKind.Enum)
            {
                if (method.Name == "op_Equality" || method.Name == "op_Inequality")
                    return true;
            }

            if (method.ContainingType.TypeKind == TypeKind.Delegate)
                return true;

            if (method.ContainingType.IsGenericType)
            {
                if (knownSymbols1.KnownPureTypes.Contains(method.ContainingType.ConstructedFrom))
                    return true;
            }
            else
            {
                if (knownSymbols1.KnownPureTypes.Contains(method.ContainingType))
                    return true;
            }


            if (knownSymbols1.KnownPureMethods.TryGetValue(Utils.GetFullMetaDataName(method.ContainingType), out var pureMethods) &&
                pureMethods.AnyMatches(method))
            {
                return true;
            }

            if (purityType == PurityType.PureExceptReadLocally || purityType == PurityType.PureExceptLocally)
            {
                if (knownSymbols1.KnownPureExceptReadLocallyMethods.TryGetValue(
                        Utils.GetFullMetaDataName(method.ContainingType),
                        out var pureExceptReadLocallyMethods) &&
                    pureExceptReadLocallyMethods.AnyMatches(method))
                {
                    return true;
                }
            }

            if (purityType == PurityType.PureExceptLocally)
            {
                if (knownSymbols1.KnownPureExceptLocallyMethods.TryGetValue(
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