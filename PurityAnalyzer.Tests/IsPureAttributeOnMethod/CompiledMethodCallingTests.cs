using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class CompiledMethodCallingTests
    {
        [Test]
        public void MethodThatCallsACompiledMethodWithTheIsPureAttributeIsPure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;


public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        return StaticClass.PureMethod();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatCallsAnCompiledMethodThatDoesNotHaveTheIsPureAttributeIsImpure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        return StaticClass.ImpureMethod();
    }

}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatCallsALocalFunctionThatCallsACompiledMethodWithTheIsPureAttributeIsPure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        string DoSomethingElsePure()
        {   
            return StaticClass.PureMethod();
        }

        return DoSomethingElsePure();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatCallsALocalFunctionThatClassACompiledMethodThatDoesNotHaveTheIsPureAttributeIsImpure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    private static int state;

    [IsPure]
    public static string DoSomething()
    {
        string DoSomethingElseImpure()
        {   
            return StaticClass.ImpureMethod();
        }

        return DoSomethingElseImpure();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().BePositive();

        }


    }
}
