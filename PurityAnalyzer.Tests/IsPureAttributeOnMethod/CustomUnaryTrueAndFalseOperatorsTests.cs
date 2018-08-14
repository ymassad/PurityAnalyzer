using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    //Note: a && b evaluates to T.false(a) ? a : T.&(a,b)
    //Note: a || b evaluates to T.true(a) ? a : T.|(a,b)

    [TestFixture]
    public class CustomUnaryTrueAndFalseOperatorsTests
    {
        [Test]
        public void PureCustomTrueOperatorMethodIsConsideredPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class CustomType
{
    [IsPure]
    public static bool operator true(CustomType c1)
    {
        return true;
    }

    public static bool operator false(CustomType c1)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void PureCustomFalseOperatorMethodIsConsideredPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class CustomType
{

    public static bool operator true(CustomType c1)
    {
        return true;
    }

    [IsPure]
    public static bool operator false(CustomType c1)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void ImpureCustomTrueOperatorMethodIsConsideredImpure()
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
    public static bool operator true(CustomType c1)
    {
        state++;
        return true;
    }

    public static bool operator false(CustomType c1)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void ImpureCustomFalseOperatorMethodIsConsideredImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class CustomType
{
    static int state = 0;

    public static bool operator true(CustomType c1)
    {
        return true;
    }

    [IsPure]
    public static bool operator false(CustomType c1)
    {
        state++;
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void MethodThatUsesPureCustomTrueOperatorIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static void DoSomething()
    {
        var a = new CustomType();

        if(a)
        {
        }
    }
}

public class CustomType
{    
    public static bool operator true(CustomType c1)
    {
        return true;
    }

    public static bool operator false(CustomType c1)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesImpureCustomTrueOperatorViaIfIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static void DoSomething()
    {
        var a = new CustomType();

        if(a)
        {
        }
    }
}

public class CustomType
{    
    static int state = 0;
    public static bool operator true(CustomType c1)
    {
        state++;
        return true;
    }

    public static bool operator false(CustomType c1)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void MethodThatUsesImpureCustomTrueOperatorViaTernaryOperatorIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static void DoSomething()
    {
        var a = new CustomType();

        var b = a ? 1 : 2;
    }
}

public class CustomType
{    
    static int state = 0;
    public static bool operator true(CustomType c1)
    {
        state++;
        return true;
    }

    public static bool operator false(CustomType c1)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatUsesPureCustomFalseOperatorViaDoubleAnd_AndSingleAndOperatorIsPure_IsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static void DoSomething()
    {
        var a = new CustomType();

        var b = new CustomType();

        var c = a && b;
    }
}

public class CustomType
{    
    public static bool operator true(CustomType c1)
    {
        return true;
    }

    public static bool operator false(CustomType c1)
    {
        return true;
    }

    public static CustomType operator &(CustomType x, CustomType y)
    {
        return x;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesPureCustomFalseOperatorViaDoubleAnd_AndSingleAndOperatorIsImpure_IsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static void DoSomething()
    {
        var a = new CustomType();

        var b = new CustomType();

        var c = a && b;
    }
}

public class CustomType
{    
    public static bool operator true(CustomType c1)
    {
        return true;
    }

    public static bool operator false(CustomType c1)
    {
        return true;
    }

    static int state = 0;

    public static CustomType operator &(CustomType x, CustomType y)
    {
        state++;
        return x;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatUsesImpureCustomFalseOperatorViaDoubleAndIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static void DoSomething()
    {
        var a = new CustomType();

        var b = new CustomType();

        var c = a && b;
    }
}

public class CustomType
{    
    public static bool operator true(CustomType c1)
    {
        return true;
    }

    static int state = 0;

    public static bool operator false(CustomType c1)
    {
        state++;
        return true;
    }


    public static CustomType operator &(CustomType x, CustomType y)
    {
        return x;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void MethodThatUsesPureCustomTrueOperatorViaDoubleOr_AndSingleOrOperatorIsPure_IsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static void DoSomething()
    {
        var a = new CustomType();

        var b = new CustomType();

        var c = a || b;
    }
}

public class CustomType
{    
    public static bool operator true(CustomType c1)
    {
        return true;
    }

    public static bool operator false(CustomType c1)
    {
        return true;
    }

    public static CustomType operator |(CustomType x, CustomType y)
    {
        return x;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesPureCustomTrueOperatorViaDoubleOr_AndSingleOrOperatorIsImpure_IsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static void DoSomething()
    {
        var a = new CustomType();

        var b = new CustomType();

        var c = a || b;
    }
}

public class CustomType
{    
    public static bool operator true(CustomType c1)
    {
        return true;
    }

    public static bool operator false(CustomType c1)
    {
        return true;
    }

    static int state = 0;

    public static CustomType operator |(CustomType x, CustomType y)
    {
        state++;
        return x;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatUsesImpureCustomTrueOperatorViaDoubleOrIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static void DoSomething()
    {
        var a = new CustomType();

        var b = new CustomType();

        var c = a || b;
    }
}

public class CustomType
{   
    static int state = 0;
    public static bool operator true(CustomType c1)
    {
        state++;
        return true;
    }

    public static bool operator false(CustomType c1)
    {   
        return true;
    }

    public static CustomType operator |(CustomType x, CustomType y)
    {
        return x;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }
    }
}
