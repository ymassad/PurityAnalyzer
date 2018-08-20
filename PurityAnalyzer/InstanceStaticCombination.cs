namespace PurityAnalyzer
{
    public abstract class InstanceStaticCombination
    {
        public sealed class Instance : InstanceStaticCombination
        {

        }

        public sealed class Static : InstanceStaticCombination
        {

        }

        public sealed class InstanceAndStatic : InstanceStaticCombination
        {

        }
    }
}