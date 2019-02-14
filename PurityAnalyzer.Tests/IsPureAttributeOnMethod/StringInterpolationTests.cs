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
    public class StringInterpolationTests
    {
        [Test]
        public void MethodThatUsesStringInterpolationWithoutExpressionsIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        string str = $""hello"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfTypeStringIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        string exp = ""value"";
        string str = $""hello{exp}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfTypeIntIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        int exp = 1;
        string str = $""hello{exp}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomSealedTypeThatHasAPureToStringMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public sealed class Class1
{
    public override string ToString() => string.Empty;
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        string str = $""hello{new Class1()}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomSealedTypeThatHasAnImpureToStringMethodIsImpure()
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
    public static void DoSomething()
    {
        string str = $""hello{new Class1()}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomSealedTypeThatHasAPureToStringMethodAndAPureIFormattableStringMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public sealed class Class1 : IFormattable
{
    public override string ToString() => string.Empty;

    public string ToString(string format, IFormatProvider formatProvider) => string.Empty;
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        string str = $""hello{new Class1()}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomSealedTypeThatHasAnImpureToStringMethodButAPureIFormattableStringMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public sealed class Class1 : IFormattable
{
    public static int state = 0;

    public override string ToString()
    {
        state++;
        return string.Empty;
    }

    public string ToString(string format, IFormatProvider formatProvider) => string.Empty;
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        string str = $""hello{new Class1()}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }



        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomSealedTypeThatHasAPureToStringMethodButAnImpureIFormattableStringMethodIsImpure()
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
    public static void DoSomething()
    {
        string str = $""hello{new Class1()}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomNonSealedTypeThatHasAPureIFormattableStringMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public class Class1 : IFormattable
{
    public string ToString(string format, IFormatProvider formatProvider) => string.Empty;
}

public static class Module1
{
    [IsPure]
    public static void DoSomething(Class1 param)
    {
        string str = $""hello{param}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomNonSealedTypeThatHasAPureVirtualIFormattableStringMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public class Class1 : IFormattable
{
    public virtual string ToString(string format, IFormatProvider formatProvider) => string.Empty;
}

public static class Module1
{
    [IsPure]
    public static void DoSomething(Class1 param)
    {
        string str = $""hello{param}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomNonSealedTypeThatHasAPureAbstractIFormattableStringMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public abstract class Class1 : IFormattable
{
    public abstract string ToString(string format, IFormatProvider formatProvider);
}

public static class Module1
{
    [IsPure]
    public static void DoSomething(Class1 param)
    {
        string str = $""hello{param}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }



        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomNonSealedTypeThatHasAnImpureIFormattableStringMethodIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public class Class1 : IFormattable
{
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
    public static void DoSomething()
    {
        string str = $""hello{new Class1()}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        //the formatting checks at runtime whether the type implements IFormattable.
        //If we cannot know for sure that the object type is the same as the variable/parameter type,
        //we cannot know if the real type implements IFormattable
        //and thus whether the IFormattable.ToString method is impure. We assume impurity
        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomNonSealedTypeThatHasNoIFormattableStringMethodIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public class Class1
{
    public override string ToString() => string.Empty;
}

public static class Module1
{
    [IsPure]
    public static void DoSomething(Class1 param)
    {
        string str = $""hello{param}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomStructTypeThatHasNoIFormattableStringMethodButHasAPureToStringMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public struct Struct1
{
    public override string ToString() => string.Empty;
}

public static class Module1
{
    [IsPure]
    public static void DoSomething(Struct1 param)
    {
        string str = $""hello{param}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomStructTypeThatHasNoIFormattableStringMethodButHasAnImpureToStringMethodIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public struct Struct1
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
    public static void DoSomething(Struct1 param)
    {
        string str = $""hello{param}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomStructTypeThatHasAPureIFormattableStringMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public struct Struct1 : IFormattable
{
    public string ToString(string format, IFormatProvider formatProvider) => string.Empty;
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        string str = $""hello{new Struct1()}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesStringInterpolationWithAnExpressionOfACustomStructTypeThatHasAnImpureIFormattableStringMethodIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}
public struct Struct1 : IFormattable
{
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
    public static void DoSomething()
    {
        string str = $""hello{new Struct1()}"";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }
}
