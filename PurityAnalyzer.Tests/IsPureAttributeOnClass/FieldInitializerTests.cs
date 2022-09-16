using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnClass
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

            Utilities.AssertPure(code);

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

            Utilities.AssertImpure(code);

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

            Utilities.AssertPure(code);

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

            Utilities.AssertImpure(code);

        }
    }
}
