using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class NewObjectTests
    {
        [Test]
        public void CreatingAnInstanceOfAClassWithPureConstructorKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    public PureDto(int age) => Age = age;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureInstanceConstructorMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state = 0;

    public PureDto(int age) { state++; Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureInstanceFieldInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    int state = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureInstancePropertyInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    int state {get;} = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureStaticFieldInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureStaticPropertyInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state {get;} = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureStaticConstructorMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state = 0;

    static PureDto()
    {
        state++;
    }

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        /* Note about creating objects with virutal methods:
         * Virtual here means (1) method with virtual keyword (2) method with override keyword (3) method belongs to an interface implementation
         * If a method creates a class that has an impure virtual method, I consider the method to be impure.
         * The reason is that the created object might be passed to a function that takes the abstract type as a parameter and invokes the virtual method.
         * By default, I consider invoking a virtual method to be pure (e.g. invoking an interface method)
         * There might be a better way to do all of this. But for now I am using these rules.
        */

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureNonVirtualMethodKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state = 0;

    public void ImpureMethod() => state++;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureVirtualMethodMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state = 0;

    public virtual int ImpureMethod() => state++;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureOverriddenMethodMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    abstract public int ImpureMethod();
}

public class PureDto : Base
{
    public int Age {get;}

    static int state = 0;

    public override int ImpureMethod() => state++;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureMethodWhichImplementsAnInterfaceMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int ImpureMethod();
}

public class PureDto : IInterface
{
    public int Age {get;}

    static int state = 0;

    public int ImpureMethod() => state++;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureMethodWhichImplementsAnInterfaceExplicitlyMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int ImpureMethod();
}

public class PureDto : IInterface
{
    public int Age {get;}

    static int state = 0;

    int IInterface.ImpureMethod() => state++;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        [Test]
        public void CreatingAnInstanceOfAClassThatHasAPureVirtualMethodKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state = 0;

    public virtual int PureMethod() => 1;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void CreatingAnInstanceOfAClassThatHasAPureOverriddenMethodKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    abstract public int PureMethod();
}

public class PureDto : Base
{
    public int Age {get;}

    static int state = 0;

    public override int PureMethod() => 1;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void CreatingAnInstanceOfAClassThatHasAPureMethodWhichImplementsAnInterfaceKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int PureMethod();
}

public class PureDto : IInterface
{
    public int Age {get;}

    static int state = 0;

    public int PureMethod() => 1;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAPureMethodWhichImplementsAnInterfaceExplicitlyKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int PureMethod();
}

public class PureDto : IInterface
{
    public int Age {get;}

    static int state = 0;

    int IInterface.PureMethod() => 1;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureStaticMethodKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state = 0;

    public static int ImpureMethod() => state++;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

    }
}
