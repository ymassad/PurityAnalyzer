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

    }
}
