using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class HigherOrderFunctionsTests
    {
        [Test]
        public void TakeingAFunctionAsAParameterAndCallingItKeepsTheMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Func<int,string> function)
    {
        return function(1);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TakeingAFunctionAsAParameterAndCallingItViaInvokeKeepsTheMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Func<int,string> function)
    {
        return function.Invoke(1);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CallingPureHigherOrderFunctionWithAPureFunctionKeepsMethodPure()
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
        return HigherOrderFunction(x => x);
    }

    [IsPure]
    public static int HigherOrderFunction(Func<int,int> function)
    {
        return function.Invoke(1);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CallingPureHigherOrderFunctionWithAnImpureFunctionMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{

    static int state = 0;

    [IsPure]
    public static string DoSomething()
    {
        return HigherOrderFunction(x => state.ToString());
    }

    [IsPure]
    public static string HigherOrderFunction(Func<int,string> function)
    {
        return function.Invoke(1);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }
}
