namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public static class ClassWithGenericMethods
    {
        [IsPure]
        public static void MethodThatUsesTAsObject<T>(T input)
        {
            var s = input.ToString();
        }

        [IsPure]
        public static void MethodThatDoesNotUseTAsObject<[NotUsedAsObject] T>(T input)
        {
            
        }
    }
}