using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class MethodCallingTests
    {
        [Test]
        public void MethodThatCallsAPureMethodIsPure()
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
        return DoSomethingElsePure();
    }

    private static string DoSomethingElsePure()
    {   
        return """";
    }

}";

            Utilities.AssertPure(code);

        }

        [Test]
        public void MethodThatCallsAnImpureMethodIsImpure()
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
        return DoSomethingElseImpure();
    }

    private static int state;

    private static string DoSomethingElseImpure()
    {   
        return state.ToString();
    }

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatCallsAPureLocalFunctionIsPure()
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
        string DoSomethingElsePure()
        {   
            return """";
        }

        return DoSomethingElsePure();
    }
}";

            Utilities.AssertPure(code);

        }

        [Test]
        public void MethodThatCallsAnImpureLocalFunctionIsImpure()
        {
            string code = @"
using System;

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
            return state.ToString();
        }

        return DoSomethingElseImpure();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatCallsAnLocalFunctionThatUpdatesLocalStateIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        int localstate = 0;

        int DoSomethingElseImpure()
        {   
            localstate++;
            return localstate;
        }

        return DoSomethingElseImpure();
    }
}";

            Utilities.AssertPure(code);

        }

    }
}
