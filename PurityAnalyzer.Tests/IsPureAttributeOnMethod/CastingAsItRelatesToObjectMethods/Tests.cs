using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod.CastingAsItRelatesToObjectMethods
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void TestCastingFromClassWithImpureToStringMethodToObject()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public override string ToString() => Console.ReadLine();
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        object obj = new Class1();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(1);
        }

        [Test]
        public void TestCastingFromClassWithImpureToStringMethodToAnInterface()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
}

public class Class1 : IInterface
{
    public override string ToString() => Console.ReadLine();
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        IInterface obj = new Class1();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(1);
        }

        [Test]
        public void TestCastingFromClassWithImpureToStringMethodToBaseClassWithPureToString()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public override string ToString() => string.Empty;
}

public class Class1 : Base
{
    public override string ToString() => Console.ReadLine();
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        Base obj = new Class1();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(1);
        }

        [Test]
        public void TestCastingFromClassWithImpureToStringMethodToBaseClassThatDoesNotOverrideToString()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{

}

public class Class1 : Base
{
    public override string ToString() => Console.ReadLine();
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        Base obj = new Class1();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(1);
        }
    }
}
