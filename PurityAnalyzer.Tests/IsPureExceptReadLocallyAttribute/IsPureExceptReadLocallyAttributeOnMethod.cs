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


        [Test]
        public void MethodThatSetsALocalAutomaticPropertyIsNotPureExceptReadLocally()
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
        PureProperty = 1;
        return 1;
    }

    private int PureProperty {get; set;}

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatGetsALocalAutomaticPropertyIsPureExceptReadLocally()
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

    private int PureProperty {get; set;}

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatGetsALocalAutomaticPropertyOnAnObjectOfADifferentTypeStoredInInstanceMutableFieldIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class2
{
    public int PureProperty {get; set;}
}

public class Class1
{
    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return instance.PureProperty;
    }

    public Class2 instance = new Class2();

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatGetsALocalAutomaticPropertyOnAnObjectOfADifferentTypeStoredInStaticMutableFieldIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class2
{
    public int PureProperty {get; set;}
}

public class Class1
{
    [IsPureExceptReadLocally]
    public int DoSomething()
    {
        return instance.PureProperty;
    }

    public static Class2 instance = new Class2();

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void CastingWhereSourceIsPureExceptReadLocallyAndTargetIsPure_CastFromLocalMutableStateAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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
        public void CastingWhereSourceIsPureExceptReadLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingAutomaticPropertyAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingAutomaticPropertyAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingAutomaticPropertyAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingReadonlyAutomaticPropertyAndPassResultToPureMethodThatReturnsAnIntegerAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingWhereSourceIsPureExceptLocallyAndTargetIsPure_CastFromLocalMutableStateProvidedByAccessingReadonlyAutomaticPropertyAndPassResultToPureMethodThatReturnsInputAndThenReturnThatAfterStoringItInAVariable_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureExceptReadLocally : Attribute
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

    [IsPureExceptReadLocally]
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




    }
}
