using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class InvokingStaticMethodTests
    {
        [Test]
        public void InvokingStaticMethodOnClassWithPureStaticConstructorKeepsMethodPure()
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
        return StaticClass.DoSomething();
    }
}

public static class StaticClass
{
    static StaticClass()
    {

    }

    public static int DoSomething() => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void InvokingStaticMethodOnClassWithPureStaticFieldInitializerKeepsMethodPure()
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
        return StaticClass.DoSomething();
    }
}

public static class StaticClass
{
    public static int field = 0;

    public static int DoSomething() => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void InvokingStaticMethodOnClassWithPureStaticPropertyInitializerKeepsMethodPure()
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
        return StaticClass.DoSomething();
    }
}

public static class StaticClass
{
    public static int Property {get;set;} = 0;

    public static int DoSomething() => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void InvokingStaticMethodOnClassWithImpureStaticConstructorMakesMethodImpure()
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
        return StaticClass.DoSomething();
    }
}

public static class StaticClass
{
    static StaticClass()
    {
        AnotherClass.state++;
    }

    public static int DoSomething() => 1;
}

public static class AnotherClass
{
    public static int state = 0;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void InvokingStaticMethodOnClassWithImpureStaticFieldInitializerMakesMethodImpure()
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
        return StaticClass.DoSomething();
    }
}

public static class StaticClass
{
    public static int field = AnotherClass.state++;

    public static int DoSomething() => 1;
}

public static class AnotherClass
{
    public static int state = 0;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void InvokingStaticMethodOnClassWithImpureStaticPropertyInitializerMakesMethodImpure()
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
        return StaticClass.DoSomething();
    }
}

public static class StaticClass
{
    public static int Property {get;set;} = AnotherClass.state++;

    public static int DoSomething() => 1;
}

public static class AnotherClass
{
    public static int state = 0;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void InvokingStaticMethodOnClassWithImpureInstanceConstructorKeepsMethodPure()
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
        return Class1.DoSomething();
    }
}

public class Class1
{
    public Class1()
    {
        AnotherClass.state++;
    }

    public static int DoSomething() => 1;
}

public static class AnotherClass
{
    public static int state = 0;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void InvokingStaticMethodOnClassWithImpureInstanceFieldInitializerKeepsMethodPure()
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
        return Class1.DoSomething();
    }
}

public class Class1
{
    public int field = AnotherClass.state++;

    public static int DoSomething() => 1;
}

public static class AnotherClass
{
    public static int state = 0;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void InvokingStaticMethodOnClassWithImpureInstancePropertyInitializerKeepsMethodPure()
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
        return Class1.DoSomething();
    }
}

public class Class1
{
    public int Property {get;set;} = AnotherClass.state++;

    public static int DoSomething() => 1;
}

public static class AnotherClass
{
    public static int state = 0;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }
    }
}
