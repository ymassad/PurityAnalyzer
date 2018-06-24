using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
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
        private readonly Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods;
        private readonly HashSet<INamedTypeSymbol> knownPureTypes;

        public ImpuritiesFinder(SemanticModel semanticModel, PurityType purityType, Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods)
        {
            this.semanticModel = semanticModel;
            this.purityType = purityType;
            this.knownReturnsNewObjectMethods = knownReturnsNewObjectMethods;

            objectType = semanticModel.Compilation.GetTypeByMetadataName(typeof(object).FullName);

            knownPureMethods = GetKnownPureMethods();
            knownPureExceptLocallyMethods = GetKnownPureExceptLocallyMethods();
            knownPureTypes = GetKnownPureTypes();
        }

        public IEnumerable<Impurity> GetImpurities(SyntaxNode node)
        {
            var allNodes = node.DescendantNodesAndSelf();

            foreach (var subNode in allNodes)
            {
                if (ContainsImpureCast(subNode))
                {
                    yield return new Impurity(node, "Cast is impure");
                }

                if (subNode is CastExpressionSyntax castExpression)
                {
                    foreach (var impurity in GetImpurities(castExpression))
                        yield return impurity;
                }
                else if (subNode is ObjectCreationExpressionSyntax objectCreation)
                {
                    foreach (var impurity in GetImpurities(objectCreation))
                        yield return impurity;
                }
                else if (subNode is IdentifierNameSyntax identifierName)
                {
                    foreach (var impurity in GetImpurities(identifierName))
                        yield return impurity;
                }
                else if (subNode is BinaryExpressionSyntax binaryExpression)
                {
                    foreach (var impurity in GetImpurities(binaryExpression))
                        yield return impurity;
                }
                else if (subNode is AssignmentExpressionSyntax assignmentExpression)
                {
                    foreach (var impurity in GetImpurities(assignmentExpression))
                        yield return impurity;
                }
                else if (subNode is ElementAccessExpressionSyntax elementAccessExpression)
                {
                    foreach (var impurity in GetImpurities(elementAccessExpression))
                        yield return impurity;
                }
            }
        }

        private IEnumerable<Impurity> GetImpurities(CastExpressionSyntax node)
        {
            if (semanticModel.GetSymbolInfo(node.Type).Symbol is ITypeSymbol destinationType &&
                semanticModel.GetTypeInfo(node.Expression).Type is ITypeSymbol sourceType)
            {
                if (IsImpureCast(sourceType, destinationType))
                {
                    yield return new Impurity(node, "Cast is impure");
                }
            }
        }

        private bool IsImpureCast(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            if (sourceType.Equals(destinationType))
                return false;

            var allDestinationMethods = Utils.GetAllMethods(destinationType).ToArray();

            if (Utils.IsDownCast(sourceType, destinationType))
            {
                var methodsOfInterfacesImplementedByDestionationType = Utils.GetAllInterfaceIncludingSelfIfIsInterface(destinationType)
                        .SelectMany(i => i.GetMembers().OfType<IMethodSymbol>())
                        .ToArray();

                var allPureOverridableMethodsOnDestionationOrItsBaseTypes =
                    allDestinationMethods
                        .Where(x => x.IsAbstract || (x.IsVirtual && !x.IsOverride))
                        .Where(method => IsMethodPure(method))
                        .ToArray();

                var sourceMethodsDownUntilBeforeDestionation = Utils.GetAllMethods(sourceType, Maybe<ITypeSymbol>.OfValue(destinationType));

                var sourceMethodsThatOverrideSomePureDestionationBaseMethod = sourceMethodsDownUntilBeforeDestionation.Where(x =>
                    x.IsOverride && x.OverriddenMethod != null && allPureOverridableMethodsOnDestionationOrItsBaseTypes.Contains(x.OverriddenMethod)).ToArray();

                if (sourceMethodsThatOverrideSomePureDestionationBaseMethod.Any(x => !IsMethodPure(x)))
                {
                    return true;
                }

                var sourceTypeMethodsImplementingMethodsDefinedInDestionationInterfaces =
                    methodsOfInterfacesImplementedByDestionationType.Select(sourceType.FindImplementationForInterfaceMember).OfType<IMethodSymbol>();

                if (sourceTypeMethodsImplementingMethodsDefinedInDestionationInterfaces.Any(x => !IsMethodPure(x)))
                    return true;
            }
            else
            {
                if (destinationType.TypeKind == TypeKind.Interface)
                    return true;

                if (destinationType.GetMembers().OfType<IMethodSymbol>().Any(x => x.IsAbstract))
                    return true;

                if (destinationType.IsSealed)
                    return false;

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

                if (interfaceMethodImplementations.Any(x => IsMethodPure(x) && !x.IsSealed))
                    return true;

                var mostDerivedOverriddenMethods = Utils.RemoveOverriddenMethods(allDestinationMethods)
                    .Where(x => x.IsOverride || x.IsVirtual)
                    .ToArray();

                if (mostDerivedOverriddenMethods.Any(x => IsMethodPure(x) && !x.IsSealed))
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<Impurity> GetImpurities(ObjectCreationExpressionSyntax node)
        {
            if (semanticModel.GetSymbolInfo(node.Type).Symbol is INamedTypeSymbol symbol)
            {
                if (!IsTypePureForConstruction(symbol))
                {
                    yield return new Impurity(node, "Constructed object is not pure");
                }
            }
        }

        private bool IsTypePureForConstruction(INamedTypeSymbol symbol)
        {
            if (Utils.SymbolHasAssumeIsPureAttribute(symbol))
                return true;

            if (!Utils.GetAllMethods(symbol)
                    .Where(x =>
                        x.MethodKind == MethodKind.Constructor ||
                        x.MethodKind == MethodKind.StaticConstructor)
                    .All(method => IsMethodPure(method)))
                return false;

            if (symbol.IsInCode())
            {
                if (AnyImpureFieldInitializer(symbol))
                    return false;

                if (AnyImpurePropertyInitializer(symbol))
                    return false;
            }

            var baseType = symbol.BaseType;

            while (!baseType.Equals(objectType))
            {
                if (baseType.IsInCode())
                {
                    if (AnyImpureFieldInitializer(baseType))
                        return false;

                    if (AnyImpurePropertyInitializer(baseType))
                        return false;
                }

                baseType = baseType.BaseType;
            }

            return true;
        }

        private bool AnyImpurePropertyInitializer(INamedTypeSymbol symbol)
        {
            return
                symbol.Locations
                    .Select(x => x.SourceTree.GetRoot().FindNode(x.SourceSpan))
                    .OfType<TypeDeclarationSyntax>()
                    .Any(x => Utils.AnyImpurePropertyInitializer(x, semanticModel, knownReturnsNewObjectMethods));
        }

        private bool AnyImpureFieldInitializer(INamedTypeSymbol symbol)
        {
            return
                symbol.Locations.Select(x => x.SourceTree.GetRoot().FindNode(x.SourceSpan))
                    .OfType<TypeDeclarationSyntax>()
                    .Any(x => Utils.AnyImpureFieldInitializer(x, semanticModel, knownReturnsNewObjectMethods));
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

        private bool IsParameterBasedAccess(IdentifierNameSyntax node)
        {
            return node.Parent is MemberAccessExpressionSyntax memberAccess
                   && memberAccess.Name == node
                   && IsParameter(memberAccess.Expression);
        }

        private bool ContainsImpureCast(SyntaxNode node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node);

            if (typeInfo.Type != null && typeInfo.ConvertedType != null &&
                !typeInfo.Type.Equals(typeInfo.ConvertedType))
            {
                var sourceType = typeInfo.Type;
                var destinationType = typeInfo.ConvertedType;

                if (IsImpureCast(sourceType, destinationType))
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<Impurity> GetImpurities(IdentifierNameSyntax node)
        {
            var symbol = semanticModel.GetSymbolInfo(node);

            if (symbol.Symbol is IFieldSymbol field)
            {
                return GetImpuritiesForFieldAccess(node, field);
            }

            if (symbol.Symbol is IPropertySymbol property)
            {
                return GetImpuritiesForPropertyAccess(node, property);
            }

            if (symbol.Symbol is IMethodSymbol method)
            {
                return GetImpuritiesForMethodAccess(node, method);
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
                        var methodOrPropertyWhereIdentifierIsUsed =
                            node.Ancestors()
                                .OfType<MethodDeclarationSyntax>()
                                .Cast<MemberDeclarationSyntax>()
                                .Concat(node.Ancestors()
                                    .OfType<PropertyDeclarationSyntax>())
                                .FirstOrNoValue();

                        bool IsAccessingLocalField(MemberDeclarationSyntax member)
                        {
                            var memberSymbol = semanticModel.GetDeclaredSymbol(member);

                            var currentType = memberSymbol.ContainingType;

                            return fieldSymbol.ContainingType == currentType && !fieldSymbol.IsStatic;
                        }

                        accessingLocalFieldLegally =
                            methodOrPropertyWhereIdentifierIsUsed
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

        private IEnumerable<Impurity> GetImpuritiesForPropertyAccess(IdentifierNameSyntax node, IPropertySymbol propertySymbol)
        {
            var usage = Utils.GetUsage(node);

            var method = usage.IsWrite() ? propertySymbol.SetMethod : propertySymbol.GetMethod;

            if (method != null)
            {
                return GetImpuritiesForMethodAccess(node, method);
            }

            return Enumerable.Empty<Impurity>();
        }

        private IEnumerable<Impurity> GetImpuritiesForMethodAccess(IdentifierNameSyntax node, IMethodSymbol method)
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

            if (!IsMethodPure(method, acceptedPurityType))
            {
                yield return new Impurity(node, "Method is impure");
            }
        }

        private IEnumerable<Impurity> GetImpurities(BinaryExpressionSyntax node)
        {
            if (semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol method)
            {
                if (!IsMethodPure(method))
                {
                    yield return new Impurity(node, "Operator is impure");
                }
            }
        }

        private IEnumerable<Impurity> GetImpurities(AssignmentExpressionSyntax node)
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
                    if (!IsMethodPure(method))
                    {
                        yield return new Impurity(node, "Operator is impure");
                    }
                }
            }
        }

        private IEnumerable<Impurity> GetImpurities(ElementAccessExpressionSyntax node)
        {
            var type = semanticModel.GetTypeInfo(node.Expression).Type;

            if (type?.TypeKind == TypeKind.Array)
            {
                var usage = Utils.GetUsage(node.Expression);

                if (usage.IsWrite())
                {
                    yield return new Impurity(node, "Impure array set");
                }

                if (usage.IsRead())
                {
                    if (semanticModel.GetSymbolInfo(node.Expression).Symbol is IFieldSymbol field && field.IsStatic)
                    {
                        yield return new Impurity(node, "Impure array read");
                    }
                }
            }
            else
            {
                var symbol = semanticModel.GetSymbolInfo(node).Symbol;

                if (symbol is IPropertySymbol propertySymbol)
                {
                    var usage = Utils.GetUsage(node.Expression);

                    if (usage.IsRead())
                    {
                        if (!IsMethodPure(propertySymbol.GetMethod))
                            yield return new Impurity(node, "Impure get");
                    }

                    if (usage.IsWrite())
                    {
                        if (!IsMethodPure(propertySymbol.SetMethod))
                            yield return new Impurity(node, "Impure set");
                    }
                }
            }
        }

        private bool IsMethodPure(IMethodSymbol method, PurityType purityType = PurityType.Pure)
        {
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