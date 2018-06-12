using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class CustomPlusBinaryOperatorTests
    {
        [Test]
        public void PureCustomPlusBinaryOperatorMethodIsConsideredPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class CustomType
{
    [IsPure]
    public static CustomType operator +(CustomType c1, CustomType c2)
    {
        return new CustomType();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void ImpureCustomPlusBinaryOperatorMethodIsConsideredImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class CustomType
{
    static int state = 0;

    [IsPure]
    public static CustomType operator +(CustomType c1, CustomType c2)
    {
        state++;
        return new CustomType();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatUsesPureCustomPlusBinaryOperatorIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static CustomType DoSomething()
    {
        return new CustomType() + new CustomType();
    }
}

public class CustomType
{    
    public static CustomType operator +(CustomType c1, CustomType c2)
    {
        return new CustomType();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesImpureCustomPlusBinaryOperatorIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static CustomType DoSomething()
    {
        return new CustomType() + new CustomType();
    }
}

public class CustomType
{
    static int state = 0;

    public static CustomType operator +(CustomType c1, CustomType c2)
    {
        state++;
        return new CustomType();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatUsesPureCustomPlusBinaryOperatorViaPlusEqualsIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static CustomType DoSomething()
    {
        var a = new CustomType();

        a += new CustomType();

        return a;
    }
}

public class CustomType
{    
    public static CustomType operator +(CustomType c1, CustomType c2)
    {
        return new CustomType();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesImpureCustomPlusBinaryOperatorViaPlusEqualsIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static CustomType DoSomething()
    {
        var a = new CustomType();

        a += new CustomType();

        return a;
    }
}

public class CustomType
{
    static int state = 0;

    public static CustomType operator +(CustomType c1, CustomType c2)
    {
        state++;
        return new CustomType();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }
    }
}
