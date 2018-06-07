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
    public class IsPureAttributeOnPropertyTests
    {
        [Test]
        public void TestIsPureAttributeOnStaticAutomaticReadonlyProperty()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static Class1()
    {
        Prop1 = 1;
    }
    
    [IsPure]
    public static int Prop1 {get;}
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TestIsPureAttributeOnStaticPropertyThatIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int a;

    [IsPure]
    public static int Prop1
    {
        get
        {
            return a++;
        }
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void TestIsPureAttributeOnStaticPropertyThatIsImpureAndThatIsExpressionBodied()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int a;

    [IsPure]
    public static int Prop1 => a++;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void TestIsPureAttributeOnStaticPropertyThatIsImpureAndThatIsExpressionBodiedGet()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int a;

    [IsPure]
    public static int Prop1
    {
        get => a++;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }
}
