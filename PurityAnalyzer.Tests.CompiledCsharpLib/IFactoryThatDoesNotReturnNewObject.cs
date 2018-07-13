namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public interface IFactoryThatDoesNotReturnNewObject
    {
        MutableClassWithPureMethodsExceptLocally Create();
    }
}