using Microsoft.CodeAnalysis;

namespace PurityAnalyzer
{
    public static class OperationExtensions
    {
        public static bool IsWrite(this IOperation operation)
        {
            switch (operation.Kind)
            {
                case OperationKind.SimpleAssignment:
                case OperationKind.Increment:
                case OperationKind.Decrement:
                case OperationKind.CompoundAssignment:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsRead(this IOperation operation)
        {
            switch (operation.Kind)
            {
                case OperationKind.SimpleAssignment:
                    return false;
                default:
                    return true;
            }
        }
    }
}