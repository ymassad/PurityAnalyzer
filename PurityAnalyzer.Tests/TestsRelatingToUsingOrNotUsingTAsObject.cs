using System.Linq;
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

        [TestCase("bool value = array.All(x => true);")]
        [TestCase("bool value = array.Any();")]
        [TestCase("bool value = array.Any(x => true);")]
        [TestCase("var value = array.SelectMany(x => new []{1,2,3});")]
        [TestCase("var value = array.SelectMany(x => new []{1,2,3}, (x,y) => 2);")]
        [TestCase("var value = array.Where(x => true);")]
        [TestCase("var value = array.Aggregate((x,y) => x);")]
        [TestCase("var value = array.Aggregate(1, (x,y) => x);")]
        [TestCase("var value = array.Aggregate(1, (x,y) => x, x => 2);")]
        [TestCase("var value = array.ElementAt(0);")]
        [TestCase("var value = array.ElementAtOrDefault(0);")]
        [TestCase("var value = array.First();")]
        [TestCase("var value = array.First(x => true);")]
        [TestCase("var value = array.FirstOrDefault();")]
        [TestCase("var value = array.FirstOrDefault(x => true);")]
        [TestCase("var value = array.Last();")]
        [TestCase("var value = array.Last(x => true);")]
        [TestCase("var value = array.LastOrDefault();")]
        [TestCase("var value = array.LastOrDefault(x => true);")]
        [TestCase("var value = array.Single();")]
        [TestCase("var value = array.Single(x => true);")]
        [TestCase("var value = array.SingleOrDefault();")]
        [TestCase("var value = array.SingleOrDefault(x => true);")]
        [TestCase("var value = array.ToArray();")]
        public void
            CallOnImmutableArrayOfClassThatHasImpureToStringEqualsAndGetHashCode_LeavesMethodPure(string code1)
        {
            string code = $@"
using System;
using System.Collections.Immutable;
using System.Linq;

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

        {code1}
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzerWithImmutableCollections(code);
            dignostics.Length.Should().Be(0);
        }

    }
}