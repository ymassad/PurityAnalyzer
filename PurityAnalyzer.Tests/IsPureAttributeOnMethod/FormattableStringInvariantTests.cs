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
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasAnInt16ExpressionIsPure()
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
        Int16 i = 1;

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasAnInt64ExpressionIsPure()
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
        Int64 i = 1;

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasADateTimeExpressionIsPure()
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
        var i = new DateTime(2019,1,1);

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasAFloatExpressionIsPure()
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
        float i = 1;

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasADoubleExpressionIsPure()
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
        double i = 1;

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasAByteExpressionIsPure()
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
        byte i = 1;

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasADecimalExpressionIsPure()
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
        decimal i = 1;

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasASByteExpressionIsPure()
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
        sbyte i = 1;

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasAUInt32ExpressionIsPure()
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
        uint i = 1;

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasAUInt16ExpressionIsPure()
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
        UInt16 i = 1;

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasAUInt64ExpressionIsPure()
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
        UInt64 i = 1;

        return FormattableString.Invariant($""{i}"");
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsTheInvariantMethodPassingAInterpolatedStringThatHasATimeSpanExpressionIsPure()
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
        TimeSpan i = new TimeSpan(20, 0, 0);

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
