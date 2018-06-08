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
    public class PropertyInitializerTests
    {
        [Test]
        public void PurePropertyInitializerKeepsClassPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

[IsPure]
public class Class1
{
    int property {get;} = AnotherClass.PureMethod();
}

public static class AnotherClass
{
    public static int PureMethod() => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void ImpurePropertyInitializerMakesClassImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

[IsPure]
public class Class1
{
    int property {get;} = AnotherClass.ImpureMethod();
}

public static class AnotherClass
{
    static int state;

    public static int ImpureMethod() => state++;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void PureStaticPropertyInitializerKeepsClassPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

[IsPure]
public class Class1
{
    static int property {get;} = AnotherClass.PureMethod();
}

public static class AnotherClass
{
    public static int PureMethod() => 1;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void ImpureStaticPropertyInitializerMakesClassImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

[IsPure]
public class Class1
{
    static int property {get;} = AnotherClass.ImpureMethod();
}

public static class AnotherClass
{
    static int state;

    public static int ImpureMethod() => state++;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }
    }
}
