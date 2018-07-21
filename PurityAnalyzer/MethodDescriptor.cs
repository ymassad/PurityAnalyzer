using System.Collections.Immutable;

namespace PurityAnalyzer
{
    public abstract class MethodDescriptor
    {
        private MethodDescriptor()
        {
        }

        public sealed class ByName : MethodDescriptor
        {
            public ByName(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        public sealed class ByNameAndParameterTypes : MethodDescriptor
        {
            public ByNameAndParameterTypes(string name, ImmutableArray<string> parameterTypeNames)
            {
                Name = name;
                ParameterTypeNames = parameterTypeNames;
            }

            public string Name { get; }

            public ImmutableArray<string> ParameterTypeNames { get; }
        }
    }
}