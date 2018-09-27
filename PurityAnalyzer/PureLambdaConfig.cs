using Microsoft.CodeAnalysis;

namespace PurityAnalyzer
{
    public sealed class PureLambdaConfig
    {
        public PureLambdaConfig(SyntaxNode lambdaScope, string pureLambdaMethodFullClassName, string pureLambdaMethodName)
        {
            LambdaScope = lambdaScope;
            PureLambdaMethodFullClassName = pureLambdaMethodFullClassName;
            PureLambdaMethodName = pureLambdaMethodName;
        }

        public SyntaxNode LambdaScope { get; }

        public string PureLambdaMethodFullClassName { get; }

        public string PureLambdaMethodName { get; }
    }
}