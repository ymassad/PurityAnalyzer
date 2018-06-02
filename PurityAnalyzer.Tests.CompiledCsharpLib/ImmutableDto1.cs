namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public class ImmutableDto1
    {
        public ImmutableDto1(int prop1, int field1)
        {
            Prop1 = prop1;
            Field1 = field1;
        }

        public int Prop1 { get; }

        public readonly int Field1;
    }
}