using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureExceptReadLocallyAttribute
{
    [TestFixture]
    public class IsPureExceptReadLocallyAttributeOnMethod
    {
        [Test]
        public void MethodThatReadsAConstantFieldIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    const int c = 1;

    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void IsPureExceptReadLocallyAttributeCannotBeAppliedOnStaticMethods()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
    public static int DoSomething()
    {
        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatReadsAReadOnlyFieldIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    readonly int c = 1;

    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsALocalReadWriteFieldIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    int c = 1;

    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesALocalReadWriteFieldIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    int c = 1;

    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        c = 2;
        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnInputParameterIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class AnotherClass
{
    public int c = 1;
}

public class Class1
{
    [IsPureExceptReadLocally]
    public int DoSomething(AnotherClass another)
    {
        return another.c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnInputParameterIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class AnotherClass
{
    public int c = 1;
}


public class Class1
{
    [IsPureExceptReadLocally]
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
        public void MethodThatReadsAStaticReadWriteFieldIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    static int c = 1;

    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReadsAStaticReadOnlyFieldIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    static readonly int c = 1;

    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAnImpureMethodIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
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
        public void MethodThatCallsAnPureMethodIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
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
        public void MethodThatCallsAMethodThatSetsLocalFieldIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return PureMethodExceptLocally();
    }

    int localState = 0;

    private int PureMethodExceptLocally() => localState++;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAMethodThatGetsLocalFieldIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return PureMethodExceptLocally();
    }

    int localState = 0;

    private int PureMethodExceptLocally() => localState;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void MethodThatCallsAMethodThatSetsLocalFieldViaThisIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return this.PureMethodExceptLocally();
    }

    int localState = 0;

    private int PureMethodExceptLocally() => localState++;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAMethodThatGetsLocalFieldViaThisIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return this.PureMethodExceptLocally();
    }

    int localState = 0;

    private int PureMethodExceptLocally() => localState;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        
        [Test]
        public void MethodThatCallsAnImpurePropertyGetterIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
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
        public void MethodThatCallsAnPurePropertyGetterIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
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
        public void MethodThatCallsAPropertyGetterThatSetsLocalFieldIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return PurePropertyExceptLocally;
    }

    int localState = 0;

    private int PurePropertyExceptLocally => localState++;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPropertyGetterThatGetsLocalFieldIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return PurePropertyExceptLocally;
    }

    int localState = 0;

    private int PurePropertyExceptLocally => localState;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void MethodThatCallsAPropertyGetterThatSetsLocalFieldViaThisIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return this.PurePropertyExceptLocally;
    }

    int localState = 0;

    private int PurePropertyExceptLocally => localState++;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsAPropertyGetterThatGetsLocalFieldViaThisIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return this.PurePropertyExceptLocally;
    }

    int localState = 0;

    private int PurePropertyExceptLocally => localState;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

    }
}
