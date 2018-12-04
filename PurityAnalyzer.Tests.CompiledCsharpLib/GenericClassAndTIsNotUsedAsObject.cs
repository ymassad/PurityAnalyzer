namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public static class GenericClassAndTIsNotUsedAsObject<[NotUsedAsObject]T>
    {
        [IsPure]
        public static void MethodThatDoesNotUseTAsObject(T input)
        {

        }
    }
}