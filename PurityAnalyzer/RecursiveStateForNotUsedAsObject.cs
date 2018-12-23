using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace PurityAnalyzer
{
    public sealed class RecursiveStateForNotUsedAsObject
    {
        public RecursiveStateForNotUsedAsObject(ImmutableArray<(IMethodSymbol method, ITypeParameterSymbol typeParameter)> itemsInStack)
        {
            ItemsInStack = itemsInStack;
        }

        public ImmutableArray<(IMethodSymbol method, ITypeParameterSymbol typeParameter)> ItemsInStack { get; }

        public static RecursiveStateForNotUsedAsObject Empty { get; } = new RecursiveStateForNotUsedAsObject(ImmutableArray<(IMethodSymbol method, ITypeParameterSymbol typeParameter)>.Empty);

        public RecursiveStateForNotUsedAsObject Add(IMethodSymbol method,  ITypeParameterSymbol typeParameter) => new RecursiveStateForNotUsedAsObject(ItemsInStack.Add((method, typeParameter)));
    }
}