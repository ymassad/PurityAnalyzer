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
    public class FormattableStringInvariantTests
    {
        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAParameterOfTypeFormattableStringIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(FormattableString param)
    {
        return FormattableString.Invariant(param);
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAConstantInterpolatedStringIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        return FormattableString.Invariant($""constant"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodViaUsingStaticFormattableStringPassingAConstantInterpolatedStringIsPure()
        {
            string code = @"
using System;
using static System.FormattableString;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        return Invariant($""constant"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasAStringExpressionIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        string str = ""hello"";

        return FormattableString.Invariant($""{str}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasAnInt32ExpressionIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        int i = 1;

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void MethodThatCallsTheInvariantMethodPassingStringInterpolationWithAnExpressionOfACustomSealedTypeThatHasAnImpureToStringMethodIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public sealed class Class1
{
    public static int state = 0;

    public override string ToString()
    {
        state++;
        return string.Empty;
    }
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        return FormattableString.Invariant($""hello{new Class1()}"");
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingStringInterpolationWithAnExpressionOfACustomSealedTypeThatHasAPureToStringMethodButAnImpureIFormattableStringMethodIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public sealed class Class1 : IFormattable
{
    public override string ToString() => string.Empty;

    public static int state = 0;

    public string ToString(string format, IFormatProvider formatProvider)
    {
        state++;
        return string.Empty;
    }
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        return FormattableString.Invariant($""hello{new Class1()}"");
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }
}
