using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PurityAnalyzer
{
    public class Visitor : CSharpSyntaxWalker
    {

        public List<(SyntaxNode node, string message)> impurities = new List<(SyntaxNode node, string message)>();

        private readonly Func<string, bool> isIsPureAttribute;

        private readonly SemanticModel semanticModel;
        private readonly INamedTypeSymbol objectType;
        private Dictionary<INamedTypeSymbol, HashSet<string>> knownPureMethods;
        private HashSet<INamedTypeSymbol> knownPureTypes;

        public Visitor(SemanticModel semanticModel, Func<string, bool> isIsPureAttribute)
        {
            this.semanticModel = semanticModel;
            this.isIsPureAttribute = isIsPureAttribute;

            objectType = semanticModel.Compilation.GetTypeByMetadataName(typeof(object).FullName);

            knownPureMethods = GetKnownPureMethods();

            knownPureTypes = GetKnownPureTypes();
        }

        private IdentifierUsage GetUsage(SyntaxNode identifier)
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


        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var symbol = semanticModel.GetSymbolInfo(node.Type).Symbol as INamedTypeSymbol;

            if (symbol != null)
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

            var allInterfaceMethods =
                symbol.AllInterfaces.SelectMany(i => i.GetMembers().OfType<IMethodSymbol>());


            var interfaceImplementationMethods =
                new HashSet<ISymbol>(
                    allInterfaceMethods.Select(x => symbol.FindImplementationForInterfaceMember(x)));
            
            if (!
                GetAllMethods(symbol)
                    .Where(x =>
                        x.MethodKind == MethodKind.Constructor ||
                        x.MethodKind == MethodKind.StaticConstructor ||
                        x.IsOverride ||
                        interfaceImplementationMethods.Contains(x))
                    .All(IsMethodPure))
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
                    .Any(x => Utils.AnyImpurePropertyInitializer(x, semanticModel));
        }

        private bool AnyImpureFieldInitializer(INamedTypeSymbol symbol)
        {
            return
                symbol.Locations.Select(x => x.SourceTree.GetRoot().FindNode(x.SourceSpan))
                    .OfType<TypeDeclarationSyntax>()
                    .Any(x => Utils.AnyImpureFieldInitializer(x, semanticModel));
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

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbol = semanticModel.GetSymbolInfo(node);

            if (symbol.Symbol is IFieldSymbol field)
            {
                if (!SymbolHasIsPureAttribute(field))
                {
                    if (!(field.IsReadOnly || field.IsConst))
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

                            if (field.ContainingType == currentType && field.IsStatic == constructorSymbol.IsStatic)
                            {
                                accessingFieldFromMatchingConstructor = true;
                            }
                        }

                        if (!accessingFieldFromMatchingConstructor)
                        {
                            var usage = GetUsage(node);

                            if (usage.IsWrite())
                            {
                                impurities.Add((node, "Write access to field"));
                            }
                            else
                            {
                                if (!IsParameterBasedAccess(node))
                                {
                                    impurities.Add((node, "Read access to non-readonly and non-const and non-input parameter based field"));
                                }
                            }
                        }
                    }
                }
            }

            if (symbol.Symbol is IPropertySymbol property)
            {
                if (!SymbolHasIsPureAttribute(property) && !SymbolHasIsPureAttribute(property.ContainingSymbol))
                {
                    if (property.IsCompiled())
                    {
                        if (!property.IsReadOnly || !IsKnownPureMethod(property.GetMethod))
                        {
                            impurities.Add((node, "Property access on type that is not in code and that does not have the Pure attribute"));
                        }
                    }
                    else if (!property.IsReadOnly)
                    {
                        var usage = GetUsage(node);

                        if (usage.IsWrite())
                        {
                            impurities.Add((node, "Write property access"));
                        }
                        else
                        {
                            if (!IsParameterBasedAccess(node))
                            {
                                impurities.Add((node, "Non input based property read"));
                            }
                            else if(IsImpure(GetPropertyGetter(property)))
                            {
                                impurities.Add((node, "Impure property getter"));
                            }
                        }
                    }
                    else
                    {
                        if (IsImpure(GetPropertyGetter(property)))
                            impurities.Add((node, "Property getter is impure"));
                    }
                }
            }

            if (symbol.Symbol is IMethodSymbol method)
            {
                if (!IsMethodPure(method))
                {
                    impurities.Add((node, "Method is impure"));
                }
            }

            if (symbol.Symbol is IEventSymbol)
            {
                impurities.Add((node, "Event access"));
            }

            base.VisitIdentifierName(node);
        }

        private bool IsImpure(SyntaxNode methodLike)
        {
            var imp = Utils.GetImpurities(methodLike, semanticModel);

            return imp.Any();
        }

        private SyntaxNode GetPropertyGetter(IPropertySymbol propertySymbol)
        {
            var localtion = propertySymbol.GetMethod.Locations.First();

            return localtion.SourceTree.GetRoot().FindNode(localtion.SourceSpan);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {

            var type = semanticModel.GetTypeInfo(node.Expression).Type;

            if (type?.TypeKind == TypeKind.Array)
            {
                var usage = GetUsage(node.Expression);

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
                    var usage = GetUsage(node.Expression);

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

        private bool IsMethodPure(IMethodSymbol method)
        {
            if (SymbolHasAssumeIsPureAttribute(method))
                return true;

            if (SymbolHasAssumeIsPureAttribute(method.ContainingType))
                return true;

            if (!SymbolHasIsPureAttribute(method))
            {
                if (method.IsInCode())
                {
                    if (method.IsImplicitlyDeclared)
                        return true;

                    var localtion = method.Locations.First();

                    var methodNode = localtion.SourceTree.GetRoot().FindNode(localtion.SourceSpan);

                    var imp = Utils.GetImpurities(methodNode, semanticModel);

                    if (imp.Any()) return false;
                }
                else
                {
                    if (!IsKnownPureMethod(method)) return false;
                }
            }

            return true;
        }

        public Dictionary<INamedTypeSymbol, HashSet<string>> GetKnownPureMethods()
        {
            return Resources.PureMethods.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(','))
                .Select(x => x.ThrowIf(v => v.Length != 2, "Invalid pure method line"))
                .Select(x => new {Type = x[0], Method = x[1].Trim()})
                .GroupBy(x => x.Type, x => x.Method)
                .ToDictionary(
                    x => semanticModel.Compilation.GetTypeByMetadataName(x.Key),
                    x => new HashSet<string>(x));
        }

        public HashSet<INamedTypeSymbol> GetKnownPureTypes()
        {
            var pureTypes =
                Resources.PureTypes.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            return new HashSet<INamedTypeSymbol>(pureTypes.Select(x => semanticModel.Compilation.GetTypeByMetadataName(x)));
        }

        private bool IsKnownPureMethod(IMethodSymbol method)
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

            if (knownPureMethods.TryGetValue(method.ContainingType, out var methods) &&
                methods.Contains(method.Name))
            {
                return true;
            }

            return false;
        }

        private bool SymbolHasIsPureAttribute(ISymbol symbol)
        {
            return symbol.GetAttributes().Any(x => isIsPureAttribute(x.AttributeClass.Name));
        }

        private bool SymbolHasAssumeIsPureAttribute(ISymbol symbol)
        {
            return symbol.GetAttributes().Any(x => x.AttributeClass.Name == "AssumeIsPureAttribute");
        }
    }
}