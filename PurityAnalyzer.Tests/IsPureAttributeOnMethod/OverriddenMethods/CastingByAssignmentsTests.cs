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
    public class CastingByAssignmentsTests
    {
        [Test]
        public void CastingByAssignmentANewInstanceOfAClassThatHasAnImpureVirtualOverriddenMethodAsBaseTypeMakesMethodImpure()
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

        Base input = obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingByAssignmentANewInstanceOfAClassThatHasAnImpureAbstractOverriddenMethodAsBaseTypeMakesMethodImpure()
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

        Base input = obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void CastingByAssignmentANewInstanceOfAClassThatHasAnImpureMethodWhichImplementsAnInterfaceAsTheInterfaceMakesMethodImpure()
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

        IInterface input = obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingByAssignmentANewInstanceOfAClassThatHasAnImpureMethodWhichImplementsAnInterfaceExplicitlyAsTheInterfaceMakesMethodImpure()
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

        IInterface input = obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }




        [Test]
        public void CastingByAssignmentANewInstanceOfAClassThatHasAPureOverriddenVirtualMethodAsBaseTypeKeepsMethodPure()
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

        Base input = obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void CastingByAssignmentANewInstanceOfAClassThatHasAPureAbstractOverriddenMethodAsBaseTypeKeepsMethodPure()
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

        Base input = obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void CastingByAssignmentANewInstanceOfAClassThatHasAPureMethodWhichImplementsAnInterfaceAsTheInterfaceKeepsMethodPure()
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

        IInterface input = obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingByAssignmentANewInstanceOfAClassThatHasAPureMethodWhichImplementsAnInterfaceExplicitlyAsTheInterfaceKeepsMethodPure()
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

        IInterface input = obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        //Because the target method should assume that the virtual method is impure since the base method is also impure
        [Test]
        public void CastingByAssignmentANewInstanceOfAClassThatHasAnImpureVirtualOverriddenMethodAsBaseTypeWhenBaseTypeMethodItSelfIsImpureKeepsMethodPure()
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
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        Base input = obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingByAssignmentANewInstanceOfAClassThatInheritsAnImpureMethodFromParentAsParentTypeKeepsMethodPure()
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

        Base input = obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingByAssignmentANewInstanceOfAClassThatInheritsAnImpureMethodFromParentAsGrandParentTypeWhereTheMethodIsDefinedAsPureMakesMethodImpure()
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

        Base0 input = obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }
}
