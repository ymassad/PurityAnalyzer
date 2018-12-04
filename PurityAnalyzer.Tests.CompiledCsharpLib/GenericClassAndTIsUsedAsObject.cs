namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public static class GenericClassAndTIsUsedAsObject<T>
    {
        [IsPure]
        public static void MethodThatUsesTAsObject(T input)
        {
            var s = input.ToString();
        }
    }
}