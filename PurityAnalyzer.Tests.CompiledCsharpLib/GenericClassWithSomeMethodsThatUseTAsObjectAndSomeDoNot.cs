namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public static class GenericClassWithSomeMethodsThatUseTAsObjectAndSomeDoNot<T>
    {
        [IsPure]
        public static string MethodThatUsesTAsObject(T input) => input.ToString();

        [IsPure]
        [DoesNotUseClassTypeParameterAsObject(nameof(T))]
        public static string MethodThatDoesNotUseTAsObject(T input) => "";
    }
}