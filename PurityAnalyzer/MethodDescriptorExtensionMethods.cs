using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PurityAnalyzer
{
    [CreateMatchMethods(typeof(MethodDescriptor))]
    public static class MethodDescriptorExtensionMethods
    {
        public static TResult Match<TResult>(this PurityAnalyzer.MethodDescriptor instance, System.Func<PurityAnalyzer.MethodDescriptor.ByNameAndParameterTypes, TResult> byNameAndParameterTypesCase, System.Func<PurityAnalyzer.MethodDescriptor.ByName, TResult> byNameCase)
        {
            if (instance is PurityAnalyzer.MethodDescriptor.ByNameAndParameterTypes byNameAndParameterTypes)
                return byNameAndParameterTypesCase(byNameAndParameterTypes);
            if (instance is PurityAnalyzer.MethodDescriptor.ByName byName)
                return byNameCase(byName);
            throw new System.Exception("Invalid MethodDescriptor type");
        }

        public static void Match(this PurityAnalyzer.MethodDescriptor instance, System.Action<PurityAnalyzer.MethodDescriptor.ByNameAndParameterTypes> byNameAndParameterTypesCase, System.Action<PurityAnalyzer.MethodDescriptor.ByName> byNameCase)
        {
            if (instance is PurityAnalyzer.MethodDescriptor.ByNameAndParameterTypes byNameAndParameterTypes)
            {
                byNameAndParameterTypesCase(byNameAndParameterTypes);
                return;
            }

            if (instance is PurityAnalyzer.MethodDescriptor.ByName byName)
            {
                byNameCase(byName);
                return;
            }

            throw new System.Exception("Invalid MethodDescriptor type");
        }

        public static bool Matches(this MethodDescriptor methodDescriptor, IMethodSymbol methodSymbol)
        {
            return methodDescriptor.Match(
                byNameCase: x => methodSymbol.Name.Equals(x.Name),
                byNameAndParameterTypesCase: x =>
                    methodSymbol.Name.Equals(x.Name) &&
                    methodSymbol.Parameters.Select(p => Utils.GetFullMetaDataName(p.Type)).SequenceEqual(x.ParameterTypeNames));
        }

        public static bool AnyMatches(this IEnumerable<MethodDescriptor> methods, IMethodSymbol method)
        {
            return methods.Any(x => x.Matches(method));
        }
    }



}