using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace PurityAnalyzer
{
    public sealed class RecursiveState
    {
        public RecursiveState(
            ImmutableArray<(IMethodSymbol method, PurityType purityType)> methodsInStack,
            ImmutableArray<INamedTypeSymbol> constructedTypesInStack)
        {
            MethodsInStack = methodsInStack;
            ConstructedTypesInStack = constructedTypesInStack;
        }

        public ImmutableArray<(IMethodSymbol method, PurityType purityType)> MethodsInStack { get; }

        public ImmutableArray<INamedTypeSymbol> ConstructedTypesInStack { get; }

        public RecursiveState AddMethod(IMethodSymbol method, PurityType purityType) => new RecursiveState(MethodsInStack.Add((method, purityType)), ConstructedTypesInStack);

        public RecursiveState AddConstructedType(INamedTypeSymbol type) => new RecursiveState(MethodsInStack, ConstructedTypesInStack.Add(type));
        
        public static RecursiveState Empty { get; } = new RecursiveState(ImmutableArray<(IMethodSymbol method, PurityType purityType)>.Empty, ImmutableArray<INamedTypeSymbol>.Empty);
    }
}