namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public interface IFactoryThatReturnsNewObject
    {
        [ReturnsNewObject]
        MutableClassWithPureMethodsExceptLocally Create();
    }
}