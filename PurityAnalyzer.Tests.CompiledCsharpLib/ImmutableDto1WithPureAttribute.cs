namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public class ImmutableDto1WithIsPureAttributeOnProperty
    {
        public ImmutableDto1WithIsPureAttributeOnProperty(int prop1, int field1)
        {
            Prop1 = prop1;
            Field1 = field1;
        }

        [IsPure]
        public int Prop1 { get; }

        public readonly int Field1;
    }
}