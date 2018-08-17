using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace PurityAnalyzer
{
    public sealed class RecursiveIsNewlyCreatedObjectState
    {
        public RecursiveIsNewlyCreatedObjectState(ImmutableArray<ILocalSymbol> variablesUnderTest)
        {
            VariablesUnderTest = variablesUnderTest;
        }

        public ImmutableArray<ILocalSymbol> VariablesUnderTest { get; }

        public RecursiveIsNewlyCreatedObjectState Add(ILocalSymbol variable) => new RecursiveIsNewlyCreatedObjectState(VariablesUnderTest.Add(variable));

        public static RecursiveIsNewlyCreatedObjectState Empty() => new RecursiveIsNewlyCreatedObjectState( ImmutableArray<ILocalSymbol>.Empty);
    }
}