using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests
{
    [TestFixture]
    public class TestsRelatingToUsingOrNotUsingTAsObject
    {
        [Test]
        public void CallingImmutableArrayCreateOnClassThatHasImpureToStringEqualsAndGetHashCode_LeavesMethodPure()
        {
            string code = @"
using System;
using System.Collections.Immutable;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int state;

    public override string ToString()
    {
        state++;
        return """";
    }

    public override bool Equals(object obj)
    {
        state++;
        return false;
    }

    public override int GetHashCode()
    {
        state++;
        return 0;
    }
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        var array = ImmutableArray.Create(new Class1());
    }
}";

            var dignostics = Utilities.RunPurityAnalyzerWithImmutableCollections(code);
            dignostics.Length.Should().Be(0);

        }
    }
}