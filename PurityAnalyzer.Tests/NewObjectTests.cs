using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests
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
        public void CreatingAnInstanceOfAClassThatHasAnImpureConstructorMakesMethodImpure()
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
        public void CreatingAnInstanceOfAClassThatHasAnImpureFieldInitializerMakesMethodImpure()
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
        public void CreatingAnInstanceOfAClassThatHasAnImpurePropertyInitializerMakesMethodImpure()
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
    }
}
