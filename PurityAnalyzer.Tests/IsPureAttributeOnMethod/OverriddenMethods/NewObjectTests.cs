using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod.OverriddenMethods
{
    [TestFixture]
    public class NewObjectTests
    {

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureNonOverriddenMethodKeepsMethodPure()
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

    public void Method() => state++;

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
        public void CreatingAnInstanceOfAClassThatHasAnImpureNonOverriddenVirtualMethodKeepsMethodPure()
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

    public virtual void Method() => state++;

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
        public void CreatingAnInstanceOfAClassThatHasAnImpureVirtualOverriddenMethodKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class PureDto : Base
{
    public int Age {get;}

    static int state = 0;

    public override int Method() => state++;

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
        public void CreatingAnInstanceOfAClassThatHasAnImpureAbstractOverriddenMethodKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    abstract public int Method();
}

public class PureDto : Base
{
    public int Age {get;}

    static int state = 0;

    public override int Method() => state++;

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
        public void CreatingAnInstanceOfAClassThatHasAnImpureMethodWhichImplementsAnInterfaceKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public class PureDto : IInterface
{
    public int Age {get;}

    static int state = 0;

    public int Method() => state++;

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
        public void CreatingAnInstanceOfAClassThatHasAnImpureMethodWhichImplementsAnInterfaceExplicitlyKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public class PureDto : IInterface
{
    public int Age {get;}

    static int state = 0;

    int IInterface.Method() => state++;

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
        public void CreatingAnInstanceOfAClassThatHasAPureNonOverriddenVirtualMethodKeepsMethodPure()
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
        public void CreatingAnInstanceOfAClassThatHasAPureOverriddenVirtualMethodKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int PureMethod() => 1;
}

public class PureDto : Base
{
    public int Age {get;}

    static int state = 0;

    public override int PureMethod() => 2;

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
        public void CreatingAnInstanceOfAClassThatHasAPureAbstractOverriddenMethodKeepsMethodPure()
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
        public void CreatingAnInstanceOfAClassThatHasAnVirtualOverriddenMethodWhichHasPureBodyButInvokesImpureBaseKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    static int state = 0;

    public virtual int Method() => state++;
}

public class PureDto : Base
{
    public int Age {get;}

    public override int Method() => base.Method() + 1;

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
        public void CreatingAnInstanceOfAClassThatHasAnImpureMethodDefinedInGrandParentClassAndOverriddenInParentClassKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base0
{
    public abstract int Method();
}

public class Base : Base0
{
    static int state = 0;

    public override int Method() => state++;
}

public class PureDto : Base
{
    public int Age {get;}

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
