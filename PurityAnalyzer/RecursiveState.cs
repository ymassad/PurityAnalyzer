using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace PurityAnalyzer
{
    public sealed class RecursiveState
    {
        public RecursiveState(ImmutableArray<IMethodSymbol> recursiveState)
        {
            MethodsInStack = recursiveState;
        }

        public ImmutableArray<IMethodSymbol> MethodsInStack { get; }

        public RecursiveState AddMethod(IMethodSymbol method) => new RecursiveState(MethodsInStack.Add(method));

        public static RecursiveState Empty { get; } = new RecursiveState(ImmutableArray<IMethodSymbol>.Empty);
    }
}