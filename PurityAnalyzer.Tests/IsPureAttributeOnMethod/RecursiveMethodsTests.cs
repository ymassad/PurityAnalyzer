using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class RecursiveMethodsTests
    {
        [Test]
        public void TestSimplePureRecursiveMethod()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(int param)
    {
        if(param == 0)
            return 1;

        return DoSomething(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TestSimpleImpureRecursiveMethod()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    static int state = 0;

    [IsPure]
    public static int DoSomething(int param)
    {
        state++;

        if(param == 0)
            return 1;

        return DoSomething(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void TestPureTwoHopRecursiveMethod()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(int param)
    {
        if(param == 0)
            return 1;

        return DoSomething2(param - 1) + 2;
    }

    public static int DoSomething2(int param)
    {
        if(param == 0)
            return 1;

        return DoSomething(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TestImpureTwoHopRecursiveMethod()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    static int state = 0;

    [IsPure]
    public static int DoSomething(int param)
    {
        state++;

        if(param == 0)
            return 1;

        return DoSomething2(param - 1) + 2;
    }

    public static int DoSomething2(int param)
    {
        if(param == 0)
            return 1;

        return DoSomething(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void TestImpureTwoHopRecursiveMethodWhereTheMethodBodyIsPureButTheOtherMethodIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    static int state = 0;

    [IsPure]
    public static int DoSomething(int param)
    {
        if(param == 0)
            return 1;

        return DoSomething2(param - 1) + 2;
    }

    public static int DoSomething2(int param)
    {
        state++;

        if(param == 0)
            return 1;

        return DoSomething(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void TestPureMethodWithIsPureAttributeCallingAnotherPureMethodThatDoesNotHaveTheIsPureAttributeAndThatCallsItSelf()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        return DoSomething2(1);
    }

    public static int DoSomething2(int param)
    {
        if(param < 0) return 1;

        return DoSomething2(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TestPureMethodWithIsPureAttributeCallingAnotherImpureMethodThatDoesNotHaveTheIsPureAttributeAndThatCallsItSelf()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        return DoSomething2(1);
    }

    static int state = 0;

    public static int DoSomething2(int param)
    {
        if(param < 0) return 1;

        state++;

        return DoSomething2(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void TestPureMethodWithIsPureAttributeCallingAnotherPureMethodThatDoesNotHaveTheIsPureAttributeAndThatCallsItSelfAndThatIsInAnotherFile()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        return Module2.DoSomething2(1);
    }
}";

            var code2 = @"
public static class Module2
{
    public static int DoSomething2(int param)
    {
        if(param < 0) return 1;

        return DoSomething2(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TestPureMethodWithIsPureAttributeCallingAnotherImpureMethodThatDoesNotHaveTheIsPureAttributeAndThatCallsItSelfAndThatIsInAnotherFile()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        return Module2.DoSomething2(1);
    }
}";

            var code2 = @"
public static class Module2
{
    static int state = 0;

    public static int DoSomething2(int param)
    {
        if(param < 0) return 1;

        state++;

        return DoSomething2(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void TestPureMethodWithIsPureAttributeCallingAnotherPureMethodThatDoesNotHaveTheIsPureAttributeAndThatCreatesAnInstanceOfAClassThatHasAFieldInitializerThatCreatesAnInstanceOfTheSameClass()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        return DoSomething2();
    }

    public static int DoSomething2()
    {
        var a = new Class1();
        return 1;
    }
}
public class Class1
{
    private Class1 instance = new Class1();
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TestPureMethodWithIsPureAttributeCallingAnotherPureMethodThatDoesNotHaveTheIsPureAttributeAndThatInvokesAPropertyThatInvokesAnIndexerThatInvokesAPlusOperatorThatThenCallsTheSameMethod()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        return DoSomething2(1);
    }

    public static int DoSomething2(int param)
    {
        if(param < 0) return 1;

        return Prop1;
    }

    public static Module1 operator+(Module1 c1, int c2)
    {
        int a = Module1.DoSomething2(c2);

        return new Module1();
    }
    
    public static Module1 Instance {get;} = new Module1();
    
    public int Any() => 1;

    public int this[int i] => (Instance + 1).Any();

    public static int Prop1 => Instance[0];
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TestPureMethodWithIsPureAttributeCallingAnotherMethodThatDoesNotHaveTheIsPureAttributeAndThatInvokesAPropertyThatInvokesAnIndexerThatInvokesAnImpurePlusOperatorThatThenCallsTheSameMethod()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        return DoSomething2(1);
    }

    public static int DoSomething2(int param)
    {
        if(param < 0) return 1;

        return Prop1;
    }

    static int state = 0;

    public static Module1 operator+(Module1 c1, int c2)
    {
        int a = Module1.DoSomething2(c2);
        
        state++;
        
        return new Module1();
    }
    
    public static Module1 Instance {get;} = new Module1();
    
    public int Any() => 1;

    public int this[int i] => (Instance + 1).Any();

    public static int Prop1 => Instance[0];
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void TestMethodWithIsPureAttributeCallingAnotherMethodThatDoesNotHaveTheIsPureAttributeAndThatCreatesAnInstanceOfAClassAndInvokesAMethodOnThatClassThatModifiesLocalStateButThatAlsoInvokesTheSameMethodOnAnInstanceStoredInAReadonlyStateField_MethodShouldBeImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        return DoSomething2();
    }

    public static int DoSomething2()
    {
        var a = new Class1();
        
        return a.Method1();

        return 1;
    }
}
public class Class1
{
    public int state = 0;

    public static readonly Class1 instance = new Class1();

    public int Method1()
    {
        state++;
        
        return instance.Method1();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        //The recursion here is related to detecting whether i represents a new object
        [Test]
        public void MethodThatReturnsTheResultOfCallingEqualsOnAnIntVariablePassingTheIntVariableItselfAsAParameterIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static bool DoSomething()
    {
        int i = 1;

        return i.Equals(i);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        //The recursion here is related to detecting whether i represents a new object
        [Test]
        public void MethodThatReturnsTheResultOfCallingDefinedEqualsMethodOnAnClassVariablePassingTheClassVariableItselfAsAParameterIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public bool Equals(object obj) => false;
}

public static class Module1
{
    [IsPure]
    public static bool DoSomething()
    {
        Class1 i = new Class1();

        return i.Equals(i);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        //The recursion here is related to detecting whether i represents a new object
        [Test]
        public void MethodThatReturnsTheResultOfCallingDefinedEqualsMethodOnAnClassVariablePassingTheClassVariableItselfAsAParameterInDirectlyByWrappingItInAnotherInstanceIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public bool Equals(object obj) => false;

    public Class1 wrapped;

    public Class1(Class1 c) => wrapped = c;

}

public static class Module1
{
    [IsPure]
    public static bool DoSomething()
    {
        Class1 i = new Class1(null);

        return i.Equals(new Class1(i));
    }

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        //The recursion here is related to detecting whether i represents a new object
        [Test]
        public void MethodThatReturnsTheResultOfCallingDefinedEqualsMethodOnAnClassVariableReferingToParameterPassingTheClassVariableItselfAsAParameterInDirectlyByWrappingItInAnotherInstanceIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public bool Equals(object obj) => false;

    public Class1 wrapped;

    public Class1(Class1 c) => wrapped = c;

}

public static class Module1
{
    [IsPure]
    public static bool DoSomething(Class1 p)
    {
        Class1 i = p;

        return i.Equals(new Class1(i));
    }

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        //The recursion here is related to detecting whether p represents a new object
        [Test]
        public void MethodThatReturnsTheResultOfCallingDefinedEqualsMethodOnAnClassParameterPassingTheClassParameterItselfAsAParameterInDirectlyByWrappingItInAnotherInstanceIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public bool Equals(object obj) => false;

    public Class1 wrapped;

    public Class1(Class1 c) => wrapped = c;
}

public static class Module1
{
    [IsPure]
    public static bool DoSomething(Class1 p)
    {
        return p.Equals(new Class1(p));
    }

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

    }
}
