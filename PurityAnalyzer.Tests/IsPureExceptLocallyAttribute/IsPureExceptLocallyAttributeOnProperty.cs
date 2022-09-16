using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureExceptLocallyAttribute
{
    [TestFixture]
    public class IsPureExceptLocallyAttributeOnProperty
    {
        [Test]
        public void PropertyGetterThatReadsAConstantFieldIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    const int c = 1;

    [IsPureExceptLocally]
    public int DoSomething => c;    
}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void IsPureExceptLocallyAttributeCannotBeAppliedOnStaticProperties()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public static int DoSomething => 1;
}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void PropertyGetterThatReadsAReadOnlyFieldIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    readonly int c = 1;

    [IsPureExceptLocally]
    public int DoSomething => c;
}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatReadsALocalReadWriteFieldIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    int c = 1;

    [IsPureExceptLocally]
    public int DoSomething => c;
}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatWritesALocalReadWriteFieldIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    int c = 1;

    [IsPureExceptLocally]
    public int DoSomething
    {
        get
        {
            c = 2;
            return 1;
        }
    }
}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatReadsAStaticReadWriteFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    static int c = 1;

    [IsPureExceptLocally]
    public int DoSomething => c;
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatReadsAStaticReadOnlyFieldIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    static readonly int c = 1;

    [IsPureExceptLocally]
    public int DoSomething => c;
}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAnImpureMethodIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething
    {
        get
        {
            DoSomethingImpure();
            return 1;
        }
    }

    static int state;

    private void DoSomethingImpure() => state++;

}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAnPureMethodIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => PureMethod();

    private int PureMethod() => 1;

}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyMethodIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => PureMethodExceptLocally();

    int localState = 0;

    private int PureMethodExceptLocally() => localState++;

}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyMethodViaThisIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => this.PureMethodExceptLocally();

    int localState = 0;

    private int PureMethodExceptLocally() => localState++;

}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyMethodOnAnObjectOfTheSameTypeStoredInAStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => instance.PureExceptLocally();

    public int PureExceptLocally() => localState++;

    int localState = 0;

    public static Class1 instance = new Class1();
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyMethodOnAnObjectOfADifferentTypeStoredInAStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    public int PureExceptLocally() => localState++;

    int localState = 0;
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => instance.PureExceptLocally();

    public static Class2 instance = new Class2();
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyMethodOnAnObjectOfTheSameTypeStoredInAReadonlyStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => instance.PureExceptLocally();

    public int PureExceptLocally() => localState++;

    int localState = 0;

    public static readonly Class1 instance = new Class1();
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyMethodOnAnObjectOfADifferentTypeStoredInAStaticReadonlyFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    public int PureExceptLocally() => localState++;

    int localState = 0;
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => instance.PureExceptLocally();

    public static readonly Class2 instance = new Class2();
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAnImpurePropertyGetterIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => ImpureProperty;

    static int state;

    private int ImpureProperty => state++;

}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAnPurePropertyGetterIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => PureProperty;

    private int PureProperty => 1;

}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertyGetterIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => PurePropertyExceptLocally;

    int localState = 0;

    private int PurePropertyExceptLocally => localState++;

}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertyGetterViaThisIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => this.PurePropertyExceptLocally;

    int localState = 0;

    private int PurePropertyExceptLocally => localState++;

}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertyGetterOnAnObjectOfTheSameTypeStoredInAStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => instance.PureExceptLocally;

    public int PureExceptLocally => localState++;

    int localState = 0;

    public static Class1 instance = new Class1();
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertyGetterOnAnObjectOfADifferentTypeStoredInAStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    public int PureExceptLocally => localState++;

    int localState = 0;
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => instance.PureExceptLocally;

    public static Class2 instance = new Class2();
}";

            Utilities.AssertImpure(code);
        }


        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertyGetterOnAnObjectOfTheSameTypeStoredInAReadonlyStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => instance.PureExceptLocally;

    public int PureExceptLocally => localState++;

    int localState = 0;

    public static readonly Class1 instance = new Class1();
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertyGetterOnAnObjectOfADifferentTypeStoredInAStaticReadonlyFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    public int PureExceptLocally => localState++;

    int localState = 0;
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething => instance.PureExceptLocally;

    public static readonly Class2 instance = new Class2();
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAnImpurePropertySetterIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething
    {
        get
        {
            ImpureProperty = 1;
            return 1;
        }
    }

    static int state;

    private int ImpureProperty { set => state = value; }
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatSetsALocalAutomaticPropertyIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething
    {
        get
        {
            PureProperty = 1;
            return 1;
        }
    }

    private int PureProperty {get; set;}

}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertySetterIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething
    {
        get
        {
            PurePropertyExceptLocally = 1;
            return 1;
        }
    }

    int localState = 0;

    private int PurePropertyExceptLocally {set => localState = value;}

}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertySetterViaThisIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething
    {
        get
        {
            this.PurePropertyExceptLocally = 1;
            return 1;
        }
    }

    int localState = 0;

    private int PurePropertyExceptLocally {set => localState = value;}

}";

            Utilities.AssertPure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfTheSameTypeStoredInAStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething
    {
        get
        {
            instance.PureExceptLocally = 1;
            return 1;
        }
    }

    private int PureExceptLocally {set => localState = value;}

    int localState = 0;

    public static Class1 instance = new Class1();
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfADifferentTypeStoredInAStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    public int PureExceptLocally {set => localState = value;}

    int localState = 0;
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething
    {
        get
        {
            instance.PureExceptLocally = 1;
            return 1;
        }
    }

    public static Class2 instance = new Class2();
}";

            Utilities.AssertImpure(code);
        }

  
        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfTheSameTypeStoredInAReadonlyStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething
    {
        get
        {
            instance.PureExceptLocally = 1;
            return 1;
        }
    }

    public int PureExceptLocally {set => localState = value;}

    int localState = 0;

    public static readonly Class1 instance = new Class1();
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void PropertyGetterThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfADifferentTypeStoredInAStaticReadonlyFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    public int PureExceptLocally {set => localState = value;}

    int localState = 0;
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething
    {
        get
        {
            instance.PureExceptLocally = 1;
            return 1;
        }
    }

    public static readonly Class2 instance = new Class2();
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromNewObjectAndStoreItInFieldIndirectlyByPassingItInAndOutOfAnotherMethod_PropertyIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public class MyClass
{
    Base field;

    [IsPureExceptLocally]
    public int Something
    {
        get
        {
            Base x = new Derived();

            field = ReturnParam(x);

            return 1;
        }
    }

    public Base ReturnParam(Base p) => p;

}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromNewObjectAndStoreItInFieldIndirectlyByPassingItInAndOutOfAnotherMethod_CurrentPropertyIsExpressionBodiedSet_PropertyIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public class MyClass
{
    Base field;

    [IsPureExceptLocally]
    public int Something
    {
        set => field = ReturnParam(new Derived());
    }

    public Base ReturnParam(Base p) => p;

}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void TestIsPureExceptLocallyAttributeOnInstanceAutomaticReadWriteProperty()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocallyAttribute]
    public int Prop1 {get; set;}
}";

            Utilities.AssertPure(code);

        }

    }
}
