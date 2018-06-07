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
    public class FieldInitializerTests
    {
        [Test]
        public void PureFieldInitializerKeepsClassPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

[IsPure]
public class Class1
{
    readonly int field = AnotherClass.PureMethod();
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
        public void ImpureFieldInitializerMakesClassImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

[IsPure]
public class Class1
{
    readonly int field = AnotherClass.ImpureMethod();
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
        public void PureStaticFieldInitializerKeepsClassPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

[IsPure]
public class Class1
{
    readonly static int field = AnotherClass.PureMethod();
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
        public void ImpureStaticFieldInitializerMakesClassImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

[IsPure]
public class Class1
{
    static readonly int field = AnotherClass.ImpureMethod();
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
