namespace PurityAnalyzer
{
    public static class IdentifierUsageExtensionMethods
    {
        public static bool IsRead(this IdentifierUsage usage)
        {
            return usage.Match(readFromCaseCase: _ => true, writtenToCaseCase: _ => false, readFromAndWrittenToCaseCase: _ => true);
        }

        public static bool IsWrite(this IdentifierUsage usage)
        {
            return usage.Match(readFromCaseCase: _ => false, writtenToCaseCase: _ => true, readFromAndWrittenToCaseCase: _ => true);
        }
    }
}