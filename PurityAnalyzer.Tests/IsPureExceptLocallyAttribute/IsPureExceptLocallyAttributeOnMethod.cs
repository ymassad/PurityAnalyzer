using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureExceptLocallyAttribute
{
    [TestFixture]
    public class IsPureExceptLocallyAttributeOnMethod
    {
        [Test]
        public void MethodThatReadsAConstantFieldIsPureExceptLocally()
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
    public int DoSomething()
    {
        return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void IsPureExceptLocallyAttributeCannotBeAppliedOnStaticMethods()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public static int DoSomething()
    {
        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatReadsAReadOnlyFieldIsPureExceptLocally()
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
    public int DoSomething()
    {
        return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsALocalReadWriteFieldIsPureExceptLocally()
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
    public int DoSomething()
    {
        return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesALocalReadWriteFieldIsPureExceptLocally()
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
    public int DoSomething()
    {
        c = 2;
        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnInputParameterIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class AnotherClass
{
    public int c = 1;
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething(AnotherClass another)
    {
        return another.c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnInputParameterIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class AnotherClass
{
    public int c = 1;
}


public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething(AnotherClass another)
    {
        another.c = 2;
        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatReadsAStaticReadWriteFieldIsNotPureExceptLocally()
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
    public int DoSomething()
    {
        return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReadsAStaticReadOnlyFieldIsPureExceptLocally()
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
    public int DoSomething()
    {
        return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAnImpureMethodIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        DoSomethingImpure();
        return 1;
    }

    static int state;

    private void DoSomethingImpure() => state++;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAnPureMethodIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return PureMethod();
    }

    private int PureMethod() => 1;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return PureMethodExceptLocally();
    }

    int localState = 0;

    private int PureMethodExceptLocally() => localState++;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodViaThisIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return this.PureMethodExceptLocally();
    }

    int localState = 0;

    private int PureMethodExceptLocally() => localState++;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnAnObjectOfTheSameTypeStoredInAStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return instance.PureExceptLocally();
    }

    public int PureExceptLocally() => localState++;

    int localState = 0;

    public static Class1 instance = new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnAnObjectOfADifferentTypeStoredInAStaticFieldIsNotPureExceptLocally()
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
    public int DoSomething()
    {
        return instance.PureExceptLocally();
    }

    public static Class2 instance = new Class2();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnAnObjectOfTheSameTypePassedAsParameterIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething(Class1 instance)
    {
        return instance.PureExceptLocally();
    }

    public int PureExceptLocally() => localState++;

    int localState = 0;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnAnObjectOfADifferentTypePassedAsParameterIsNotPureExceptLocally()
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
    public int DoSomething(Class2 instance)
    {
        return instance.PureExceptLocally();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnAnObjectOfTheSameTypeStoredInAReadonlyStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return instance.PureExceptLocally();
    }

    public int PureExceptLocally() => localState++;

    int localState = 0;

    public static readonly Class1 instance = new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnAnObjectOfADifferentTypeStoredInAStaticReadonlyFieldIsNotPureExceptLocally()
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
    public int DoSomething()
    {
        return instance.PureExceptLocally();
    }

    public static readonly Class2 instance = new Class2();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAnImpurePropertyGetterIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return ImpureProperty;
    }

    static int state;

    private int ImpureProperty => state++;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAnPurePropertyGetterIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return PureProperty;
    }

    private int PureProperty => 1;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertyGetterIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return PurePropertyExceptLocally;
    }

    int localState = 0;

    private int PurePropertyExceptLocally => localState++;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertyGetterViaThisIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return this.PurePropertyExceptLocally;
    }

    int localState = 0;

    private int PurePropertyExceptLocally => localState++;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertyGetterOnAnObjectOfTheSameTypeStoredInAStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return instance.PureExceptLocally;
    }

    public int PureExceptLocally => localState++;

    int localState = 0;

    public static Class1 instance = new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertyGetterOnAnObjectOfADifferentTypeStoredInAStaticFieldIsNotPureExceptLocally()
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
    public int DoSomething()
    {
        return instance.PureExceptLocally;
    }

    public static Class2 instance = new Class2();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertyGetterOnAnObjectOfTheSameTypePassedAsParameterIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething(Class1 instance)
    {
        return instance.PureExceptLocally;
    }

    public int PureExceptLocally => localState++;

    int localState = 0;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertyGetterOnAnObjectOfADifferentTypePassedAsParameterIsNotPureExceptLocally()
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
    public int DoSomething(Class2 instance)
    {
        return instance.PureExceptLocally;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertyGetterOnAnObjectOfTheSameTypeStoredInAReadonlyStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return instance.PureExceptLocally;
    }

    public int PureExceptLocally => localState++;

    int localState = 0;

    public static readonly Class1 instance = new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertyGetterOnAnObjectOfADifferentTypeStoredInAStaticReadonlyFieldIsNotPureExceptLocally()
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
    public int DoSomething()
    {
        return instance.PureExceptLocally;
    }

    public static readonly Class2 instance = new Class2();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAnImpurePropertySetterIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        ImpureProperty = 1;
        return 1;
    }

    static int state;

    private int ImpureProperty { set => state = value; }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatSetsALocalAutomaticPropertyIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        PureProperty = 1;
        return 1;
    }

    private int PureProperty {get; set;}

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatGetsALocalAutomaticPropertyIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        return PureProperty;
    }

    private int PureProperty {get; set;}

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        PurePropertyExceptLocally = 1;
        return 1;
    }

    int localState = 0;

    private int PurePropertyExceptLocally {set => localState = value;}

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterViaThisIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        this.PurePropertyExceptLocally = 1;
        return 1;
    }

    int localState = 0;

    private int PurePropertyExceptLocally {set => localState = value;}

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfTheSameTypeStoredInAStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        instance.PureExceptLocally = 1;
        return 1;
    }

    private int PureExceptLocally {set => localState = value;}

    int localState = 0;

    public static Class1 instance = new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfADifferentTypeStoredInAStaticFieldIsNotPureExceptLocally()
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
    public int DoSomething()
    {
        instance.PureExceptLocally = 1;
        return 1;
    }

    public static Class2 instance = new Class2();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfTheSameTypePassedAsParameterIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething(Class1 instance)
    {
        instance.PureExceptLocally = 1;
        return 1;
    }

    private int PureExceptLocally {set => localState = value;}

    int localState = 0;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfADifferentTypePassedAsParameterIsNotPureExceptLocally()
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
    public int DoSomething(Class2 instance)
    {
        instance.PureExceptLocally = 1;
        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfTheSameTypeStoredInAReadonlyStaticFieldIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        instance.PureExceptLocally = 1;
        return 1;
    }

    public int PureExceptLocally {set => localState = value;}

    int localState = 0;

    public static readonly Class1 instance = new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfADifferentTypeStoredInAStaticReadonlyFieldIsNotPureExceptLocally()
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
    public int DoSomething()
    {
        instance.PureExceptLocally = 1;
        return 1;
    }

    public static readonly Class2 instance = new Class2();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReadsLocalMutableArrayElementIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    int[] arr = {1,2,3};

    [IsPureExceptLocally]
    public int DoSomething()
    {
        return arr[0];
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsLocalReadonlyArrayElementIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    readonly int[] arr = {1,2,3};

    [IsPureExceptLocally]
    public int DoSomething()
    {
        return arr[0];
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void MethodThatWritesLocalMutableArrayElementIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    int[] arr = {1,2,3};

    [IsPureExceptLocally]
    public int DoSomething()
    {
        arr[0] = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesLocalReadonlyArrayElementIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class1
{
    readonly int[] arr = {1,2,3};

    [IsPureExceptLocally]
    public int DoSomething()
    {
        arr[0] = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void MethodThatReadsLocalMutableArrayElementIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    int[] arr = {1,2,3};

    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return arr[0];
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsLocalReadonlyArrayElementIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    readonly int[] arr = {1,2,3};

    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return arr[0];
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void MethodThatWritesLocalMutableArrayElementIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    int[] arr = {1,2,3};

    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        arr[0] = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatWritesLocalReadonlyArrayElementIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    readonly int[] arr = {1,2,3};

    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        arr[0] = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfADifferentTypeStoredInAFieldIsPureExceptLocally()
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
    public int DoSomething()
    {
        instance.PureExceptLocally = 1;
        return 1;
    }

    public Class2 instance = new Class2();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfADifferentTypeStoredInAReadOnlyFieldIsPureExceptLocally()
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
    public int DoSomething()
    {
        instance.PureExceptLocally = 1;
        return 1;
    }

    public readonly Class2 instance = new Class2();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfADifferentTypeStoredInAFieldInAnObjectThatIsStoredInAFieldIsPureExceptLocally()
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

public class Class3
{
    public Class2 instance = new Class2();
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        instance.instance.PureExceptLocally = 1;
        return 1;
    }

    public Class3 instance = new Class3();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAnImpurePropertySetterOnAnObjectOfADifferentTypeStoredInAFieldInAnObjectThatIsStoredInAFieldIsImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    public int PureExceptLocally {set => staticState = value;}

    static int staticState = 0;
}

public class Class3
{
    public Class2 instance = new Class2();
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        instance.instance.PureExceptLocally = 1;
        return 1;
    }

    public Class3 instance = new Class3();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfADifferentTypeStoredInAFieldInAnObjectThatIsStoredInAFieldIsPureExceptLocally_FieldIsAccessedViaThis()
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

public class Class3
{
    public Class2 instance = new Class2();
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        this.instance.instance.PureExceptLocally = 1;
        return 1;
    }

    public Class3 instance = new Class3();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAnImpurePropertySetterOnAnObjectOfADifferentTypeStoredInAFieldInAnObjectThatIsStoredInAFieldIsImpure_FieldIsAccessedViaThis()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    public int PureExceptLocally {set => staticState = value;}

    static int staticState = 0;
}

public class Class3
{
    public Class2 instance = new Class2();
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        this.instance.instance.PureExceptLocally = 1;
        return 1;
    }

    public Class3 instance = new Class3();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatModifiesFieldOnAnObjectOfADifferentTypeStoredInAFieldInAnObjectThatIsStoredInAFieldIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    public int localState = 0;
}

public class Class3
{
    public Class2 instance = new Class2();
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        instance.instance.localState = 1;
        return 1;
    }

    public Class3 instance = new Class3();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatModifiesFieldOnAnObjectOfADifferentTypeStoredInAFieldInAnObjectThatIsStoredInAFieldIsPureExceptLocally_FieldIsAccessedViaThis()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    public int localState = 0;
}

public class Class3
{
    public Class2 instance = new Class2();
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        this.instance.instance.localState = 1;
        return 1;
    }

    public Class3 instance = new Class3();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }



        [Test]
        public void MethodThatCallsAPureExceptLocallyPropertySetterOnAnObjectOfADifferentTypeStoredInAFieldAccessedOnParameterOfTheSameTypeIsNotPureExceptLocally()
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
    public int DoSomething(Class1 param)
    {
        param.instance.PureExceptLocally = 1;
        return 1;
    }

    public Class2 instance = new Class2();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatSetsFieldOnAnObjectOfADifferentTypeStoredInAFieldAccessedOnParameterOfTheSameTypeIsNotPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    public int localState = 0;
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething(Class1 param)
    {
        param.instance.localState = 1;
        return 1;
    }

    public Class2 instance = new Class2();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        //Because the current method could store the target in a field
        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromNewObjectAndStoreItInField_MethodIsNotPureExceptLocally()
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
    public int DoSomething()
    {
        Base x = new Derived();

        field = x;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromNewObjectAndStoreItInFieldIndirectlyByPassingItInAndOutOfAnotherMethod_MethodIsNotPureExceptLocally()
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
    public int DoSomething()
    {
        Base x = new Derived();

        field = ReturnParam(x);

        return 1;
    }

    public Base ReturnParam(Base p) => p;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromNewObjectAndStoreItInFieldIndirectlyByPassingItInAndOutOfAnotherMethod_CurrentMethodIsExpressionBodied_MethodIsNotPureExceptLocally()
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
    public void DoSomething() => field = ReturnParam(new Derived());

    public Base ReturnParam(Base p) => p;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        [Test]
        public void CastingWhereSourceIsPureExceptReadLocallyAndTargetIsPure_CastFromLocalMutableStateAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state;
}


public class MyClass
{
    public Derived localDerived;

    [IsPureExceptLocally]
    public int DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInt(x);

        return y;
    }

}

public class Class1
{
    public int ReturnInt(Base x) => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptReadLocallyAndTargetIsPure_CastFromLocalMutableStateAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state;
}


public class MyClass
{
    public Derived localDerived;

    [IsPureExceptLocally]
    public Base DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInput(x);

        return y;
    }

}

public class Class1
{
    public Base ReturnInput(Base x) => x;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Derived localDerived;

    [IsPureExceptLocally]
    public int DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInt(x);

        return y;
    }

}

public class Class1
{
    public int ReturnInt(Base x) => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Derived localDerived;

    [IsPureExceptLocally]
    public Base DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInput(x);

        return y;
    }

}

public class Class1
{
    public Base ReturnInput(Base x) => x;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateViaThisAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Derived localDerived;

    [IsPureExceptLocally]
    public int DoSomething()
    {
        Base x = this.localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInt(x);

        return y;
    }

}

public class Class1
{
    public int ReturnInt(Base x) => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateViaThisAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Derived localDerived;

    [IsPureExceptLocally]
    public Base DoSomething()
    {
        Base x = this.localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInput(x);

        return y;
    }

}

public class Class1
{
    public Base ReturnInput(Base x) => x;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableNestedStateAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public AnotherClass local;

    [IsPureExceptLocally]
    public int DoSomething()
    {
        Base x = local.localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInt(x);

        return y;
    }

}

public class AnotherClass
{
     public Derived localDerived;
}

public class Class1
{
    public int ReturnInt(Base x) => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableNestedStateAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public AnotherClass local;

    [IsPureExceptLocally]
    public Base DoSomething()
    {
        Base x = local.localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInput(x);

        return y;
    }

}

public class AnotherClass
{
     public Derived localDerived;
}


public class Class1
{
    public Base ReturnInput(Base x) => x;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableNestedStateViaThisAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public AnotherClass local;

    [IsPureExceptLocally]
    public int DoSomething()
    {
        Base x = this.local.localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInt(x);

        return y;
    }

}

public class AnotherClass
{
     public Derived localDerived;
}

public class Class1
{
    public int ReturnInt(Base x) => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableNestedStateViaThisAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public AnotherClass local;

    [IsPureExceptLocally]
    public Base DoSomething()
    {
        Base x = this.local.localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInput(x);

        return y;
    }
}

public class AnotherClass
{
     public Derived localDerived;
}

public class Class1
{
    public Base ReturnInput(Base x) => x;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }





        [Test]
        public void CastingWhereSourceIsPureExceptReadLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingAutomaticPropertyAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state;
}


public class MyClass
{
    public Derived localDerived {get;set;}

    [IsPureExceptLocally]
    public int DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInt(x);

        return y;
    }

}

public class Class1
{
    public int ReturnInt(Base x) => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptReadLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingAutomaticPropertyAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state;
}


public class MyClass
{
    public Derived localDerived {get;set;}

    [IsPureExceptLocally]
    public Base DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInput(x);

        return y;
    }

}

public class Class1
{
    public Base ReturnInput(Base x) => x;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void CastingWhereSourceIsPureExceptReadLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingReadonlyAutomaticPropertyAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state;
}


public class MyClass
{
    public Derived localDerived {get;}

    [IsPureExceptLocally]
    public int DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInt(x);

        return y;
    }

}

public class Class1
{
    public int ReturnInt(Base x) => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptReadLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingReadonlyAutomaticPropertyAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state;
}


public class MyClass
{
    public Derived localDerived {get;}

    [IsPureExceptLocally]
    public Base DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInput(x);

        return y;
    }

}

public class Class1
{
    public Base ReturnInput(Base x) => x;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingAutomaticPropertyAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Derived localDerived {get;set;}

    [IsPureExceptLocally]
    public int DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInt(x);

        return y;
    }

}

public class Class1
{
    public int ReturnInt(Base x) => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingAutomaticPropertyAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Derived localDerived {get;set;}

    [IsPureExceptLocally]
    public Base DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInput(x);

        return y;
    }

}

public class Class1
{
    public Base ReturnInput(Base x) => x;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingReadonlyAutomaticPropertyAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Derived localDerived {get;}

    [IsPureExceptLocally]
    public int DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInt(x);

        return y;
    }

}

public class Class1
{
    public int ReturnInt(Base x) => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingReadonlyAutomaticPropertyAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Derived localDerived {get;}

    [IsPureExceptLocally]
    public Base DoSomething()
    {
        Base x = localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInput(x);

        return y;
    }

}

public class Class1
{
    public Base ReturnInput(Base x) => x;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }




        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingAutomaticPropertyOnLocalFieldAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Class2 local = new Class2();    

    [IsPureExceptLocally]
    public int DoSomething()
    {
        Base x = local.localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInt(x);

        return y;
    }

}

public class Class1
{
    public int ReturnInt(Base x) => 1;
}

public class Class2
{
    public Derived localDerived {get;}
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingAutomaticPropertyOnLocalFieldAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Class2 local = new Class2();

    [IsPureExceptLocally]
    public Base DoSomething()
    {
        Base x = local.localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInput(x);

        return y;
    }

}

public class Class1
{
    public Base ReturnInput(Base x) => x;
}

public class Class2
{
    public Derived localDerived {get;}
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingAutomaticPropertyOnLocalFieldViaThisAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Class2 local = new Class2();    

    [IsPureExceptLocally]
    public int DoSomething()
    {
        Base x = this.local.localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInt(x);

        return y;
    }

}

public class Class1
{
    public int ReturnInt(Base x) => 1;
}

public class Class2
{
    public Derived localDerived {get;}
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingAutomaticPropertyOnLocalFieldViaThisAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptLocally : Attribute
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
    public Class2 local = new Class2();

    [IsPureExceptLocally]
    public Base DoSomething()
    {
        Base x = this.local.localDerived;

        Class1 class1 = new Class1();

        var y = class1.ReturnInput(x);

        return y;
    }

}

public class Class1
{
    public Base ReturnInput(Base x) => x;
}

public class Class2
{
    public Derived localDerived {get;}
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewObject_IsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureExceptLocallyAttribute : Attribute
{
}

public class Class2
{
    private int state = 0;

    public void Do() => state++;
}

public class Class1
{
    [IsPureExceptLocally]
    public int DoSomething()
    {
        var class2 = new Class2();

        class2.Do();

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

    }
}
