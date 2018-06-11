using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class RecursiveMethodsTests
    {
        [Test]
        public void TestSimplePureRecursiveMethod()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(int param)
    {
        if(param == 0)
            return 1;

        return DoSomething(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TestSimpleImpureRecursiveMethod()
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
    public static int DoSomething(int param)
    {
        state++;

        if(param == 0)
            return 1;

        return DoSomething(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void TestPureTwoHopRecursiveMethod()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(int param)
    {
        if(param == 0)
            return 1;

        return DoSomething2(param - 1) + 2;
    }

    public static int DoSomething2(int param)
    {
        if(param == 0)
            return 1;

        return DoSomething(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TestImpureTwoHopRecursiveMethod()
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
    public static int DoSomething(int param)
    {
        state++;

        if(param == 0)
            return 1;

        return DoSomething2(param - 1) + 2;
    }

    public static int DoSomething2(int param)
    {
        if(param == 0)
            return 1;

        return DoSomething(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void TestImpureTwoHopRecursiveMethodWhereTheMethodBodyIsPureButTheOtherMethodIsImpure()
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
    public static int DoSomething(int param)
    {
        if(param == 0)
            return 1;

        return DoSomething2(param - 1) + 2;
    }

    public static int DoSomething2(int param)
    {
        state++;

        if(param == 0)
            return 1;

        return DoSomething(param - 1) + 2;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }
    }
}
