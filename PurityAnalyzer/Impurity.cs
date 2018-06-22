using Microsoft.CodeAnalysis;

namespace PurityAnalyzer
{
    public class Impurity
    {
        public Impurity(SyntaxNode node, string message)
        {
            Node = node;
            Message = message;
        }

        public SyntaxNode Node { get; }

        public string Message { get; }
    }
}