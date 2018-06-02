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
    public class MethodCallingTests
    {
        [Test]
        public void MethodThatCallsAPreMethodIsPure()
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

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

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

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

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
    public static string DoSomething()
    {
        int localstate;

        string DoSomethingElseImpure()
        {   
            localstate++;
            return localstate.ToString();
        }

        return DoSomethingElseImpure();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

    }
}
