using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class ArrayTests
    {
        //Although arrays are mutable, I assume reading from an array passed as a parameter to be a pure operation
        //It is better to pass ImmutableArray though
        [Test]
        public void MethodThatReadsArrayElementFromInputIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(string[] input)
    {
        return input[0];
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatSetsArrayElementFromInputIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(string[] input)
    {
        input[0] = """";
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        //It is impure because arrays are mutable
        [Test]
        public void MethodThatGetsElementInArrayDefinedAsAStaticFieldIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    static readonly int[] arr = {1,2,3};

    [IsPure]
    public static string DoSomething()
    {
        var v = arr[1];
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void MethodThatSetsElementInArrayDefinedAsAStaticFieldIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    static readonly int[] arr = {1,2,3};

    [IsPure]
    public static string DoSomething()
    {
        arr[1] = 2;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }


}
