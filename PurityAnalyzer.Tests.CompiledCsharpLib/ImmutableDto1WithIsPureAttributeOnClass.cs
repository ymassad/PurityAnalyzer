namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    [IsPure]
    public class ImmutableDto1WithIsPureAttributeOnClass
    {
        public ImmutableDto1WithIsPureAttributeOnClass(int prop1, int field1)
        {
            Prop1 = prop1;
            Field1 = field1;
        }

       
        public int Prop1 { get; }

        public readonly int Field1;
    }
}