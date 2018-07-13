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
        private readonly Dictionary<string, HashSet<string>> knownPureMethods;
        private readonly Dictionary<string, HashSet<string>> knownPureExceptLocallyMethods;
        private readonly Dictionary<string, HashSet<string>> knownPureExceptReadLocallyMethods;
        private readonly Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods;
        private readonly HashSet<INamedTypeSymbol> knownPureTypes;
        private readonly IPropertySymbol arrayIListItemProperty;
        private readonly INamedTypeSymbol genericIenumeratorType;
        private readonly INamedTypeSymbol ienumeratorType;
        private readonly INamedTypeSymbol boolType;
        private readonly INamedTypeSymbol idisposableType;

        public ImpuritiesFinder(SemanticModel semanticModel, PurityType purityType, Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods)
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
                if (IsImpureCast(sourceType, destinationType, recursiveState) is CastPurityResult.Impure impure)
                {
                    yield return new Impurity(node, "Cast is impure" + Environment.NewLine + impure.Reason);
                }
            }
        }

        private CastPurityResult IsImpureCast(ITypeSymbol sourceType, ITypeSymbol destinationType,
            RecursiveState recursiveState)
        {
            if (sourceType.Equals(destinationType))
                return new CastPurityResult.Pure();

            var allDestinationMethods = Utils.GetAllMethods(destinationType).ToArray();

            if (Utils.IsUpCast(sourceType, destinationType))
            {
                var methodsOfInterfacesImplementedByDestionationType =
                    Utils.GetAllInterfaceIncludingSelfIfIsInterface(destinationType)
                        .SelectMany(i => i.GetMembers().OfType<IMethodSymbol>())
                        .ToArray();
                
                var allPureOverridableMethodsOnDestionationOrItsBaseTypes =
                    allDestinationMethods
                        .Where(x => x.IsAbstract || (x.IsVirtual && !x.IsOverride))
                        .Select(method => new
                        {
                            Method = method,
                            PurityType = GetMethodPurityType (method,recursiveState)
                        })
                        .Where(x => x.PurityType.HasValue)
                        .Select(x => new
                        {
                            x.Method,
                            PurityType = x.PurityType.GetValue()
                        })
                        .ToArray();

                var sourceMethodsUpUntilBeforeDestionation =
                    Utils.GetAllMethods(sourceType, Maybe<ITypeSymbol>.OfValue(destinationType));

                var sourceMethodsThatOverrideSomePureDestionationBaseMethod =
                    sourceMethodsUpUntilBeforeDestionation.Where(x =>
                            x.IsOverride && x.OverriddenMethod != null)
                        .Select(x =>
                            new
                            {
                                Method = x,
                                CorrespondingMethod =
                                    allPureOverridableMethodsOnDestionationOrItsBaseTypes
                                        .FirstOrNoValue(z => z.Method.Equals(x.OverriddenMethod))
                            })
                        .Where(x =>
                            x.CorrespondingMethod.HasValue)
                        .Select(x => new
                        {
                            x.Method,
                            CorrespondingMethod  = x.CorrespondingMethod.GetValue()
                        })
                        .ToArray();

                var impureOnes = sourceMethodsThatOverrideSomePureDestionationBaseMethod
                    .Where(x => GetMethodPurityType(x.Method, recursiveState)
                        .HasNoValueOr(purity => !IsGreaterOrEqaulPurity(purity, x.CorrespondingMethod.PurityType)))
                    .ToArray();

                if (impureOnes.Any())
                {
                    return new CastPurityResult.Impure(
                        "These pure methods at target type have impure overrides on source type: " + Environment.NewLine +
                        string.Join(Environment.NewLine, impureOnes.Select(x => x.Method.Name)));
                }
            

                var sourceTypeMethodsImplementingMethodsDefinedInDestionationInterfaces =
                    methodsOfInterfacesImplementedByDestionationType
                        .Select(sourceType.FindImplementationForInterfaceMember)
                        .OfType<IMethodSymbol>();

                var impureOnes1 = sourceTypeMethodsImplementingMethodsDefinedInDestionationInterfaces
                    .Where(x => !IsMethodPure(x, recursiveState))
                    .ToArray();

                if (impureOnes1.Any())
                    return new CastPurityResult.Impure(
                        "These pure methods at target type have impure overrides on source type: " + Environment.NewLine +
                        string.Join(Environment.NewLine, impureOnes1.Select(x => x.Name)));
            }
            else
            {
                if (destinationType.TypeKind == TypeKind.Interface)
                    return new CastPurityResult.Impure("Downcasting to an interface type");

                if (destinationType.GetMembers().OfType<IMethodSymbol>().Any(x => x.IsAbstract))
                    return new CastPurityResult.Impure("Downcasting to a type with abstract methods");

                if (destinationType.IsSealed)
                    return new CastPurityResult.Pure();

                var methodsOfInterfacesImplementedByDestionationType =
                    destinationType.AllInterfaces
                        .SelectMany(i => i.GetMembers().OfType<IMethodSymbol>())
                        .ToArray();

                var interfaceMethodImplementations =
                    methodsOfInterfacesImplementedByDestionationType
                        .Select(destinationType.FindImplementationForInterfaceMember)
                        .OfType<IMethodSymbol>()
                        .Select(x => Utils.FindMostDerivedMethod(allDestinationMethods, x))
                        .ToArray();

                var pureNonSealedOnes = interfaceMethodImplementations
                    .Where(x => !x.IsSealed)
                    .Where(x => IsMethodPure(x, recursiveState))
                    .ToArray();

                if (pureNonSealedOnes.Any())
                    return new CastPurityResult.Impure(
                        "Downcasting to type implementing interface methods via methods that are pure and non-sealed. Methods: " + Environment.NewLine +
                        string.Join(Environment.NewLine, pureNonSealedOnes.Select(x => x.Name)));

                var mostDerivedOverriddenMethods =
                    Utils.RemoveOverriddenMethods(allDestinationMethods)
                        .Where(x => x.IsOverride || x.IsVirtual)
                        .ToArray();

                var pureNonSealedOnes1 =
                    mostDerivedOverriddenMethods
                        .Where(x => !x.IsSealed)
                        .Where(x => IsMethodPure(x, recursiveState))
                        .ToArray();

                if (pureNonSealedOnes1.Any())
                {
                    return new CastPurityResult.Impure(
                        "Downcasting to type overriding some methods via methods that are pure and non-sealed. Methods: " + Environment.NewLine +
                        string.Join(Environment.NewLine, pureNonSealedOnes1.Select(x => x.Name)));

                }
            }

            return new CastPurityResult.Pure();
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

        private bool IsParameter(ExpressionSyntax node)
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

                return IsImpureCast(sourceType, destinationType, recursiveState);

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
                        var methodOrPropertyOrIndexerWhereIdentifierIsUsed =
                            node.Ancestors()
                                .OfType<MethodDeclarationSyntax>()
                                .Cast<MemberDeclarationSyntax>()
                                .Concat(node.Ancestors()
                                    .OfType<PropertyDeclarationSyntax>())
                                .Concat(node.Ancestors()
                                    .OfType<IndexerDeclarationSyntax>())
                                .FirstOrNoValue();

                        bool IsAccessingLocalField(MemberDeclarationSyntax member)
                        {
                            var memberSymbol = semanticModel.GetDeclaredSymbol(member);

                            var currentType = memberSymbol.ContainingType;

                            return fieldSymbol.ContainingType == currentType && !fieldSymbol.IsStatic;
                        }

                        accessingLocalFieldLegally =
                            methodOrPropertyOrIndexerWhereIdentifierIsUsed
                                .ChainValue(IsAccessingLocalField)
                                .ValueOr(false)
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
            PurityType acceptedPurityType = PurityType.Pure;

            if (purityType == PurityType.PureExceptLocally || purityType == PurityType.PureExceptReadLocally)
            {
                if (node.Parent is InvocationExpressionSyntax)
                {
                    acceptedPurityType = purityType;
                }
                else if (node.Parent is MemberAccessExpressionSyntax memberAccess &&
                         memberAccess.Expression.Kind() == SyntaxKind.ThisExpression)
                {
                    if (memberAccess.Parent is InvocationExpressionSyntax)
                    {
                        acceptedPurityType = purityType;
                    }
                    else
                    {
                        var operation = semanticModel.GetOperation(memberAccess);

                        if (operation is IPropertyReferenceOperation propertyReferenceOperation &&
                            propertyReferenceOperation.Instance.Kind == OperationKind.InstanceReference)
                        {
                            acceptedPurityType = purityType;
                        }
                    }
                }
                else if (node is ElementAccessExpressionSyntax)
                {
                    acceptedPurityType = purityType;
                }
                else
                {
                    var operation = semanticModel.GetOperation(node);

                    if (operation is IPropertyReferenceOperation propertyReferenceOperation &&
                        propertyReferenceOperation.Instance.Kind == OperationKind.InstanceReference)
                    {
                        acceptedPurityType = purityType;
                    }
                }
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

        private IEnumerable<Impurity> GetImpurities4(BinaryExpressionSyntax node,
            RecursiveState recursiveState)
        {
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

        private Dictionary<string, HashSet<string>> GetKnownPureMethods()
        {
            var pureMethodsFileContents =
                Resources.PureMethods
                + Environment.NewLine
                + PurityAnalyzerAnalyzer
                  .CustomPureMethodsFilename.ChainValue(File.ReadAllText)
                  .ValueOr("");

            return pureMethodsFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(','))
                .Select(x => x.ThrowIf(v => v.Length != 2, "Invalid pure method line"))
                .Select(x => new { Type = x[0], Method = x[1].Trim() })
                .GroupBy(x => x.Type, x => x.Method)
                .ToDictionary(
                    x => x.Key,
                    x => new HashSet<string>(x));
        }

        private Dictionary<string, HashSet<string>> GetKnownPureExceptLocallyMethods()
        {
            var pureMethodsExceptLocallyFileContents =
                Resources.PureExceptLocallyMethods
                    +Environment.NewLine
                    + PurityAnalyzerAnalyzer
                        .CustomPureExceptLocallyMethodsFilename.ChainValue(File.ReadAllText)
                        .ValueOr("");

            return pureMethodsExceptLocallyFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(','))
                .Select(x => x.ThrowIf(v => v.Length != 2, "Invalid pure-except-locally method line"))
                .Select(x => new { Type = x[0], Method = x[1].Trim() })
                .GroupBy(x => x.Type, x => x.Method)
                .ToDictionary(
                    x => x.Key,
                    x => new HashSet<string>(x));
        }

        private Dictionary<string, HashSet<string>> GetKnownPureExceptReadLocallyMethods()
        {
            var pureMethodsExceptLocallyFileContents =
                Resources.PureExceptReadLocallyMethods
                    +Environment.NewLine
                    + PurityAnalyzerAnalyzer
                        .CustomPureExceptReadLocallyMethodsFilename.ChainValue(File.ReadAllText)
                        .ValueOr("");

            return pureMethodsExceptLocallyFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(','))
                .Select(x => x.ThrowIf(v => v.Length != 2, "Invalid pure-except-read-locally method line"))
                .Select(x => new { Type = x[0], Method = x[1].Trim() })
                .GroupBy(x => x.Type, x => x.Method)
                .ToDictionary(
                    x => x.Key,
                    x => new HashSet<string>(x));
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
                pureMethods.Contains(method.Name))
            {
                return true;
            }

            if (purityType == PurityType.PureExceptReadLocally || purityType == PurityType.PureExceptLocally)
            {
                if (knownPureExceptReadLocallyMethods.TryGetValue(
                        Utils.GetFullMetaDataName(method.ContainingType),
                        out var pureExceptReadLocallyMethods) &&
                    pureExceptReadLocallyMethods.Contains(method.Name))
                {
                    return true;
                }
            }

            if(purityType == PurityType.PureExceptLocally)
            {
                if (knownPureExceptLocallyMethods.TryGetValue(
                        Utils.GetFullMetaDataName(method.ContainingType),
                        out var pureExceptLocallyMethods) &&
                    pureExceptLocallyMethods.Contains(method.Name))
                {
                    return true;
                }
            }




            return false;
        }
    }
}