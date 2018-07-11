using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace PurityAnalyzer
{
    public sealed class RecursiveState
    {
        public RecursiveState(ImmutableArray<IMethodSymbol> recursiveState, ImmutableArray<INamedTypeSymbol> constructedTypesInStack)
        {
            MethodsInStack = recursiveState;
            ConstructedTypesInStack = constructedTypesInStack;
        }

        public ImmutableArray<IMethodSymbol> MethodsInStack { get; }

        public ImmutableArray<INamedTypeSymbol> ConstructedTypesInStack { get; }

        public RecursiveState AddMethod(IMethodSymbol method) => new RecursiveState(MethodsInStack.Add(method), ConstructedTypesInStack);

        public RecursiveState AddConstructedType(INamedTypeSymbol type) => new RecursiveState(MethodsInStack, ConstructedTypesInStack.Add(type));
        
        public static RecursiveState Empty { get; } = new RecursiveState(ImmutableArray<IMethodSymbol>.Empty, ImmutableArray<INamedTypeSymbol>.Empty);
    }
}