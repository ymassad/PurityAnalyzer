namespace PurityAnalyzer
{
    public abstract class CastPurityResult
    {
        private CastPurityResult()
        {
        }

        public sealed class Pure : CastPurityResult
        {
        }

        public sealed class Impure : CastPurityResult
        {
            public Impure(string reason)
            {
                Reason = reason;
            }

            public string Reason { get; }
        }
    }
}