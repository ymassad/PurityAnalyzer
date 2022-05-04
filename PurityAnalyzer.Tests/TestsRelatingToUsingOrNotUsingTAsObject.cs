using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests
{
    [TestFixture]
    public class TestsRelatingToUsingOrNotUsingTAsObject
    {
        private static readonly string ClassWithImpureToStringEqualsAndGetHashCode = $@"
public class ClassWithImpureToStringEqualsAndGetHashCode
{{
    public int state;

    public override string ToString()
    {{
        state++;
        return """";
    }}

    public override bool Equals(object obj)
    {{
        state++;
        return false;
    }}

    public override int GetHashCode()
    {{
        state++;
        return 0;
    }}
}}";

        [Test]
        public void CallingImmutableArrayCreateWhereTIsClassThatHasImpureToStringEqualsAndGetHashCode_LeavesMethodPure()
        {
            string code = $@"
using System;
using System.Collections.Immutable;

public class IsPureAttribute : Attribute
{{
}}

{ClassWithImpureToStringEqualsAndGetHashCode}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var array = ImmutableArray.Create(new ClassWithImpureToStringEqualsAndGetHashCode());
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzerWithImmutableCollections(code);
            dignostics.Length.Should().Be(0);

        }
    }
}