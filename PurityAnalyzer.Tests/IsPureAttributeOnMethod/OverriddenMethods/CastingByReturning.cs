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
    public class CastingByReturning
    {
        [Test]
        public void CastingByReturningANewInstanceOfAClassThatHasAnImpureVirtualOverriddenMethodAsBaseTypeMakesMethodImpure()
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
    public static Base DoSomething()
    {
        var obj = new PureDto(1);

        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingByReturningANewInstanceOfAClassThatHasAnImpureAbstractOverriddenMethodAsBaseTypeMakesMethodImpure()
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
    public static Base DoSomething()
    {
        var obj = new PureDto(1);

        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void CastingByReturningANewInstanceOfAClassThatHasAnImpureMethodWhichImplementsAnInterfaceAsTheInterfaceMakesMethodImpure()
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
    public static IInterface DoSomething()
    {
        var obj = new PureDto(1);

        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingByReturningANewInstanceOfAClassThatHasAnImpureMethodWhichImplementsAnInterfaceExplicitlyAsTheInterfaceMakesMethodImpure()
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
    public static IInterface DoSomething()
    {
        var obj = new PureDto(1);

        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }




        [Test]
        public void CastingByReturningANewInstanceOfAClassThatHasAPureOverriddenVirtualMethodAsBaseTypeKeepsMethodPure()
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
    public static Base DoSomething()
    {
        var obj = new PureDto(1);

        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void CastingByReturningANewInstanceOfAClassThatHasAPureAbstractOverriddenMethodAsBaseTypeKeepsMethodPure()
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
    public static Base DoSomething()
    {
        var obj = new PureDto(1);

        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void CastingByReturningANewInstanceOfAClassThatHasAPureMethodWhichImplementsAnInterfaceAsTheInterfaceKeepsMethodPure()
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
    public static IInterface DoSomething()
    {
        var obj = new PureDto(1);

        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingByReturningANewInstanceOfAClassThatHasAPureMethodWhichImplementsAnInterfaceExplicitlyAsTheInterfaceKeepsMethodPure()
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
    public static IInterface DoSomething()
    {
        var obj = new PureDto(1);

        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        //Because the target method should assume that the virtual method is impure since the base method is also impure
        [Test]
        public void CastingByReturningANewInstanceOfAClassThatHasAnImpureVirtualOverriddenMethodAsBaseTypeWhenBaseTypeMethodItSelfIsImpureKeepsMethodPure()
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

    static int state = 0;

    public override int Method() => state++;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static Base DoSomething()
    {
        var obj = new PureDto(1);

        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingByReturningANewInstanceOfAClassThatInheritsAnImpureMethodFromParentAsParentTypeKeepsMethodPure()
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
    public static Base DoSomething()
    {
        var obj = new PureDto(1);

        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingByReturningANewInstanceOfAClassThatInheritsAnImpureMethodFromParentAsGrandParentTypeWhereTheMethodIsDefinedAsPureMakesMethodImpure()
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
    public static Base0 DoSomething()
    {
        var obj = new PureDto(1);

        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void CastingByReturningANewInstanceOfAClassThatInheritsAnImpureMethodFromParentAsParentTypeKeepsMethodPure_MethodIsExpressionBodied()
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
    public static Base DoSomething() => new PureDto(1);
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingByReturningANewInstanceOfAClassThatInheritsAnImpureMethodFromParentAsGrandParentTypeWhereTheMethodIsDefinedAsPureMakesMethodImpure_MethodIsExpressionBodied()
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
    public static Base0 DoSomething() => new PureDto(1);
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }
}
