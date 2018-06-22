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
    public class Visitor : CSharpSyntaxWalker
    {

        public List<(SyntaxNode node, string message)> impurities = new List<(SyntaxNode node, string message)>();

        private readonly Func<string, bool> isIsPureAttribute;
        private readonly bool exceptLocally;

        private readonly SemanticModel semanticModel;
        private readonly INamedTypeSymbol objectType;
        private readonly Dictionary<string, HashSet<string>> knownPureMethods;
        private readonly Dictionary<string, HashSet<string>> knownPureExceptLocallyMethods;
        private readonly Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods;
        private readonly HashSet<INamedTypeSymbol> knownPureTypes;

        public Visitor(SemanticModel semanticModel, Func<string, bool> isIsPureAttribute, bool exceptLocally, Dictionary<string, HashSet<string>> knownReturnsNewObjectMethods)
        {
            this.semanticModel = semanticModel;
            this.isIsPureAttribute = isIsPureAttribute;
            this.exceptLocally = exceptLocally;
            this.knownReturnsNewObjectMethods = knownReturnsNewObjectMethods;

            objectType = semanticModel.Compilation.GetTypeByMetadataName(typeof(object).FullName);

            knownPureMethods = GetKnownPureMethods();
            knownPureExceptLocallyMethods = GetKnownPureExceptLocallyMethods();
            knownPureTypes = GetKnownPureTypes();
        }

        public override void VisitCastExpression(CastExpressionSyntax node)
        {
            if (semanticModel.GetSymbolInfo(node.Type).Symbol is ITypeSymbol destinationType &&
                semanticModel.GetTypeInfo(node.Expression).Type is ITypeSymbol sourceType)
            {
                if (IsImpureCast(sourceType, destinationType))
                {
                    impurities.Add((node, "Cast is impure"));
                }
            }

            base.VisitCastExpression(node);
        }

        public IEnumerable<IMethodSymbol> GetAllMethods(
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

        private static IEnumerable<IMethodSymbol> GetMethods(ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers().OfType<IMethodSymbol>();
        }

        private bool IsDownCast(ITypeSymbol sourceType, ITypeSymbol destinationType)
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

        private ImmutableArray<INamedTypeSymbol> GetAllInterfaceIncludingSelfIfIsInterface(ITypeSymbol type)
        {
            var allInterfaces = type.AllInterfaces;

            if (type.TypeKind == TypeKind.Interface)
                return allInterfaces.Add((INamedTypeSymbol)type);

            return allInterfaces;
        }

        private bool IsImpureCast(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            if (sourceType.Equals(destinationType))
                return false;

            var allDestinationMethods = GetAllMethods(destinationType).ToArray();

            if (IsDownCast(sourceType, destinationType))
            {
                var methodsOfInterfacesImplementedByDestionationType =
                    GetAllInterfaceIncludingSelfIfIsInterface(destinationType)
                        .SelectMany(i => i.GetMembers().OfType<IMethodSymbol>())
                        .ToArray();

                var allPureOverridableMethodsOnDestionationOrItsBaseTypes =
                    allDestinationMethods
                        .Where(x => x.IsAbstract || (x.IsVirtual && !x.IsOverride))
                        .Where(method => IsMethodPure(method))
                        .ToArray();

                var sourceMethodsDownUntilBeforeDestionation = GetAllMethods(sourceType, Maybe<ITypeSymbol>.OfValue(destinationType));

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
                        .Select(x => FindMostDerivedMethod(allDestinationMethods, x))
                        .ToArray();

                if (interfaceMethodImplementations.Any(x => IsMethodPure(x) && !x.IsSealed))
                    return true;

                var mostDerivedOverriddenMethods = RemoveOverriddenMethods(allDestinationMethods)
                    .Where(x => x.IsOverride || x.IsVirtual)
                    .ToArray();

                if (mostDerivedOverriddenMethods.Any(x => IsMethodPure(x) && !x.IsSealed))
                {
                    return true;
                }
            }

            return false;
        }

        private IMethodSymbol FindMostDerivedMethod(IMethodSymbol[] allMethods, IMethodSymbol method)
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

        private IMethodSymbol[] RemoveOverriddenMethods(IMethodSymbol[] methods)
        {
            HashSet<IMethodSymbol> set = new HashSet<IMethodSymbol>(methods);

            foreach (var method in methods)
            {
                if (method.OverriddenMethod != null)
                    set.Remove(method.OverriddenMethod);
            }

            return set.ToArray();
        }


        public override void DefaultVisit(SyntaxNode node)
        {
            if (ContainsImpureCast(node))
            {
                impurities.Add((node, "Cast is impure"));
            }

            base.DefaultVisit(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            if (semanticModel.GetSymbolInfo(node.Type).Symbol is INamedTypeSymbol symbol)
            {
                if (!IsTypePureForConstruction(symbol))
                {
                    impurities.Add((node, "Constructed object is not pure"));
                }
            }

            base.VisitObjectCreationExpression(node);
        }

        private IMethodSymbol[] GetAllMethods(INamedTypeSymbol symbol)
        {

            if (symbol.Equals(objectType))
                return new IMethodSymbol[0];

            return
                symbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Concat(GetAllMethods(symbol.BaseType))
                    .ToArray();
        }

        private bool IsTypePureForConstruction(INamedTypeSymbol symbol)
        {
            if (SymbolHasAssumeIsPureAttribute(symbol))
                return true;

            if (!
                GetAllMethods(symbol)
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

        private bool IsParameterBasedAccess(MemberAccessExpressionSyntax node)
        {
            var accessedSymbol = semanticModel.GetSymbolInfo(node.Expression).Symbol;

            if (accessedSymbol is IParameterSymbol)
                return true;

            if (node.Expression is MemberAccessExpressionSyntax parentExpression)
                return IsParameterBasedAccess(parentExpression);

            return false;
        }

        private bool IsParameterBasedAccess(IdentifierNameSyntax node)
        {
            return node.Parent is MemberAccessExpressionSyntax memberAccess && IsParameterBasedAccess(memberAccess);
        }

        public bool ContainsImpureCast(SyntaxNode node)
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

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbol = semanticModel.GetSymbolInfo(node);

            if (symbol.Symbol is IFieldSymbol field)
            {
                ProcessFieldSymbol(node, field);
            }

            if (symbol.Symbol is IPropertySymbol property)
            {
                ProcessPropertySymbol(node, property);
            }

            if (symbol.Symbol is IMethodSymbol method)
            {
                ProcessMethodSymbol(node, method);
            }

            if (symbol.Symbol is IEventSymbol)
            {
                impurities.Add((node, "Event access"));
            }

            base.VisitIdentifierName(node);
        }

        private void ProcessFieldSymbol(IdentifierNameSyntax node, IFieldSymbol fieldSymbol)
        {
            if (!(fieldSymbol.IsReadOnly || fieldSymbol.IsConst))
            {
                if (!IsAccessOnNewlyCreatedObject(node))
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

                    if (exceptLocally)
                    {
                        var methodOrPropertyWhereIdentifierIsUsed =
                            node.Ancestors()
                                .OfType<MethodDeclarationSyntax>()
                                .Cast<MemberDeclarationSyntax>()
                                .Concat(node.Ancestors()
                                    .OfType<PropertyDeclarationSyntax>())
                                .FirstOrNoValue();

                        bool IsAccessingLocalField(MemberDeclarationSyntax m)
                        {
                            var methodSymbol = semanticModel.GetDeclaredSymbol(m);

                            var currentType = methodSymbol.ContainingType;

                            return fieldSymbol.ContainingType == currentType && !fieldSymbol.IsStatic;
                        }

                        accessingLocalFieldLegally =
                            methodOrPropertyWhereIdentifierIsUsed.ChainValue(IsAccessingLocalField).ValueOr(false);
                    }

                    if (!accessingFieldFromMatchingConstructor && !accessingLocalFieldLegally)
                    {
                        var usage = Utils.GetUsage(node);

                        if (usage.IsWrite())
                        {
                            impurities.Add((node, "Write access to field"));
                        }
                        else
                        {
                            if (!IsParameterBasedAccess(node))
                            {
                                impurities.Add(
                                    (node, "Read access to non-readonly and non-const and non-input parameter based field"));
                            }
                        }
                    }
                }
            }
        }

        private void ProcessPropertySymbol(IdentifierNameSyntax node, IPropertySymbol propertySymbol)
        {
            var usage = Utils.GetUsage(node);

            var method = usage.IsWrite() ? propertySymbol.SetMethod : propertySymbol.GetMethod;

            if (method != null)
            {
                ProcessMethodSymbol(node, method);
            }
        }

        private void ProcessMethodSymbol(IdentifierNameSyntax node, IMethodSymbol method)
        {
            bool acceptMethodToBePureExceptLocally = false;

            if (exceptLocally)
            {
                if (node.Parent is InvocationExpressionSyntax)
                {
                    acceptMethodToBePureExceptLocally = true;
                }
                else if (node.Parent is MemberAccessExpressionSyntax memberAccess &&
                         memberAccess.Expression.Kind() == SyntaxKind.ThisExpression)
                {
                    if (memberAccess.Parent is InvocationExpressionSyntax)
                    {
                        acceptMethodToBePureExceptLocally = true;
                    }
                    else
                    {
                        var operation = semanticModel.GetOperation(memberAccess);

                        if (operation is IPropertyReferenceOperation propertyReferenceOperation &&
                            propertyReferenceOperation.Instance.Kind == OperationKind.InstanceReference)
                        {
                            acceptMethodToBePureExceptLocally = true;
                        }
                    }
                }
                else
                {
                    var operation = semanticModel.GetOperation(node);

                    if (operation is IPropertyReferenceOperation propertyReferenceOperation &&
                        propertyReferenceOperation.Instance.Kind == OperationKind.InstanceReference)
                    {
                        acceptMethodToBePureExceptLocally = true;
                    }
                }
            }
            else
            {
                if (IsAccessOnNewlyCreatedObject(node))
                {
                    acceptMethodToBePureExceptLocally = true;
                } 
            }

            if (!IsMethodPure(method, acceptMethodToBePureExceptLocally))
            {
                impurities.Add((node, "Method is impure"));
            }
        }

        private bool IsAccessOnNewlyCreatedObject(IdentifierNameSyntax node)
        {
            if (!(node.Parent is MemberAccessExpressionSyntax memberAccess))
                return false;

            return Utils.IsNewlyCreatedObject(semanticModel, memberAccess.Expression, knownReturnsNewObjectMethods);
        }

        private bool IsImpure(SyntaxNode methodLike, bool exceptLocally = false)
        {
            var imp = Utils.GetImpurities(methodLike, semanticModel, knownReturnsNewObjectMethods, exceptLocally);

            return imp.Any();
        }

        private SyntaxNode GetPropertyGetter(IPropertySymbol propertySymbol)
        {
            var localtion = propertySymbol.GetMethod.Locations.First();

            return localtion.SourceTree.GetRoot().FindNode(localtion.SourceSpan);
        }

        private SyntaxNode GetPropertySetter(IPropertySymbol propertySymbol)
        {
            var localtion = propertySymbol.SetMethod.Locations.First();

            return localtion.SourceTree.GetRoot().FindNode(localtion.SourceSpan);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol method)
            {
                if (!IsMethodPure(method))
                {
                    impurities.Add((node, "Operator is impure"));
                }
            }

            base.VisitBinaryExpression(node);
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
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
                        impurities.Add((node, "Operator is impure"));
                    }
                }
            }

            base.VisitAssignmentExpression(node);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {

            var type = semanticModel.GetTypeInfo(node.Expression).Type;

            if (type?.TypeKind == TypeKind.Array)
            {
                var usage = Utils.GetUsage(node.Expression);

                if (usage.IsWrite())
                {
                    impurities.Add((node, "Impure array set"));
                }

                if (usage.IsRead())
                {
                    if (semanticModel.GetSymbolInfo(node.Expression).Symbol is IFieldSymbol field && field.IsStatic)
                    {
                        impurities.Add((node, "Impure array read"));
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
                            impurities.Add((node, "Impure get"));
                    }

                    if (usage.IsWrite())
                    {
                        if (!IsMethodPure(propertySymbol.SetMethod))
                            impurities.Add((node, "Impure set"));
                    }
                }
            }


            base.VisitElementAccessExpression(node);
        }

        private bool IsMethodPure(IMethodSymbol method, bool exceptLocally = false)
        {
            if (method.IsAbstract)
                return true;

            if (SymbolHasAssumeIsPureAttribute(method))
                return true;

            if (SymbolHasAssumeIsPureAttribute(method.ContainingType))
                return true;

            if (exceptLocally)
            {
                if (SymbolHasIsPureExceptLocallyAttribute(method))
                    return true;
            }

            if (SymbolHasIsPureAttribute(method))
                return true;

            if (SymbolHasIsPureAttribute(method.ContainingType))
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
                                || exceptLocally;
                        }
                        if (accessor.Kind() == SyntaxKind.GetAccessorDeclaration)
                        {
                            return property.IsReadOnly || exceptLocally;
                        }
                    }
                }

                var imp = Utils.GetImpurities(methodNode, semanticModel.Compilation.GetSemanticModel(locationSourceTree), knownReturnsNewObjectMethods, exceptLocally);

                if (imp.Any()) return false;
            }
            else
            {
                if (!IsKnownPureMethod(method, exceptLocally)) return false;
            }

            return true;
        }

        public Dictionary<string, HashSet<string>> GetKnownPureMethods()
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

        public Dictionary<string, HashSet<string>> GetKnownPureExceptLocallyMethods()
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

        public HashSet<INamedTypeSymbol> GetKnownPureTypes()
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

        private bool IsKnownPureMethod(IMethodSymbol method, bool exceptLocally = false)
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
            
            if(exceptLocally)
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

        private bool SymbolHasIsPureAttribute(ISymbol symbol)
        {
            return Utils.GetAllAttributes(symbol).Any(x => isIsPureAttribute(x.AttributeClass.Name));
        }

        private bool SymbolHasIsPureExceptLocallyAttribute(ISymbol symbol)
        {
            return Utils.GetAllAttributes(symbol).Any(x => Utils.IsIsPureExceptLocallyAttribute(x.AttributeClass.Name));
        }

        private bool SymbolHasAssumeIsPureAttribute(ISymbol symbol)
        {
            return Utils.GetAllAttributes(symbol).Any(x => x.AttributeClass.Name == "AssumeIsPureAttribute");
        }
    }
}