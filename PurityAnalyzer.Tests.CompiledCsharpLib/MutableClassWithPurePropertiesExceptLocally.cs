namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public class MutableClassWithPurePropertiesExceptLocally
    {
        public int Field1;

        [IsPureExceptLocally]
        public int PureExceptLocallyProperty
        {
            get => Field1;
            set => Field1 = value;
        }

        public int PureExceptLocallyPropertyGetterAndSetter
        {
            [IsPureExceptLocally]
            get => Field1;
            [IsPureExceptLocally]
            set => Field1 = value;
        }

        [IsPure]
        public MutableClassWithPurePropertiesExceptLocally()
        {
            
        }
    }
}