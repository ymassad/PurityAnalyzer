using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PurityAnalyzer
{
    [CreateMatchMethods(typeof(InstanceStaticCombination))]
    public static class InstanceStaticCombinationExtensionMethods
    {
        public static TResult Match<TResult>(this PurityAnalyzer.InstanceStaticCombination instance, System.Func<PurityAnalyzer.InstanceStaticCombination.InstanceAndStatic, TResult> instanceAndStaticCase, System.Func<PurityAnalyzer.InstanceStaticCombination.Instance, TResult> instanceCase, System.Func<PurityAnalyzer.InstanceStaticCombination.Static, TResult> staticCase)
        {
            if (instance is PurityAnalyzer.InstanceStaticCombination.InstanceAndStatic instanceAndStatic)
                return instanceAndStaticCase(instanceAndStatic);
            if (instance is PurityAnalyzer.InstanceStaticCombination.Instance instance1)
                return instanceCase(instance1);
            if (instance is PurityAnalyzer.InstanceStaticCombination.Static static1)
                return staticCase(static1);
            throw new System.Exception("Invalid InstanceStaticCombination type");
        }

        public static void Match(this PurityAnalyzer.InstanceStaticCombination instance, System.Action<PurityAnalyzer.InstanceStaticCombination.InstanceAndStatic> instanceAndStaticCase, System.Action<PurityAnalyzer.InstanceStaticCombination.Instance> instanceCase, System.Action<PurityAnalyzer.InstanceStaticCombination.Static> staticCase)
        {
            if (instance is PurityAnalyzer.InstanceStaticCombination.InstanceAndStatic instanceAndStatic)
            {
                instanceAndStaticCase(instanceAndStatic);
                return;
            }

            if (instance is PurityAnalyzer.InstanceStaticCombination.Instance instance1)
            {
                instanceCase(instance1);
                return;
            }

            if (instance is PurityAnalyzer.InstanceStaticCombination.Static static1)
            {
                staticCase(static1);
                return;
            }

            throw new System.Exception("Invalid InstanceStaticCombination type");
        }

        public static bool Matches(this InstanceStaticCombination combination, FieldDeclarationSyntax field)
        {
            return combination.Match(
                instanceAndStaticCase: _ => true,
                instanceCase: _ => !field.IsStatic(),
                staticCase: _ => field.IsStatic());
        }

        public static bool Matches(this InstanceStaticCombination combination, PropertyDeclarationSyntax property)
        {
            return combination.Match(
                instanceAndStaticCase: _ => true,
                instanceCase: _ => !property.IsStatic(),
                staticCase: _ => property.IsStatic());
        }
    }
}