namespace PurityAnalyzer
{
    public abstract class IdentifierUsage
    {
        private IdentifierUsage()
        {
        }

        public class ReadFromCase : IdentifierUsage
        {

        }

        public class WrittenToCase : IdentifierUsage
        {

        }

        public class ReadFromAndWrittenToCase : IdentifierUsage
        {

        }

        public static IdentifierUsage ReadFrom() => new ReadFromCase();
        public static IdentifierUsage WrittenTo() => new WrittenToCase();
        public static IdentifierUsage ReadFromAndWrittenTo() => new ReadFromAndWrittenToCase();
    }
}