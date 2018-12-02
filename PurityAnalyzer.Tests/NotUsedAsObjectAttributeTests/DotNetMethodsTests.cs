using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace PurityAnalyzer.Tests.NotUsedAsObjectAttributeTests
{
    [TestFixture]
    public class DotNetMethodsTests
    {
        private string class1Code = @"
public class Class1
{
    static int state = 0;
    
    public override string ToString()
    {
        state++;
        return string.Empty;
    }
}";

        private string isPureAttributeCode = @"
public class IsPureAttribute : Attribute
{
}";

        [Test]
        public void CallingEnumerableWhereAndUsingTypeArgumentThatHasImpureToStringMethod_KeepsMethodPure()
        {
            string code = $@"
using System;
using System.Linq;

{class1Code}
{isPureAttributeCode}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var result = new []{{new Class1()}}.Where(x => true);
    }}
}}";
            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void CallingEnumerableSelectAndUsingTSourceAndTResultTypeArgumentsThatHaveImpureToStringMethod_KeepsMethodPure()
        {
            string code = $@"
using System;
using System.Linq;

{class1Code}
{isPureAttributeCode}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var result = new []{{new Class1()}}.Select(x => new Class1());
    }}
}}";
            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void CallingEnumerableSelectManyAndUsingTSourceAndTResultTypeArgumentsThatHaveImpureToStringMethod_KeepsMethodPure()
        {
            string code = $@"
using System;
using System.Linq;

{class1Code}
{isPureAttributeCode}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var result = new []{{new Class1()}}.SelectMany(x => new[]{{new Class1()}});
    }}
}}";
            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void CallingEnumerableFirstAndUsingTypeArgumentThatHasImpureToStringMethod_KeepsMethodPure()
        {
            string code = $@"
using System;
using System.Linq;

{class1Code}
{isPureAttributeCode}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var result = new []{{new Class1()}}.First();
    }}
}}";
            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void CallingEnumerableGroupByAndUsingTSourceTypeArgumentThatHasImpureToStringMethod_KeepsMethodPure()
        {
            string code = $@"
using System;
using System.Linq;

{class1Code}
{isPureAttributeCode}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var result = new []{{new Class1()}}.GroupBy(x => 'a');
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void CallingEnumerableGroupByAndUsingTKeyTypeArgumentThatHasImpureToStringMethod_MakesMethodImpure()
        {
            string code = $@"
using System;
using System.Linq;

{class1Code}
{isPureAttributeCode}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var result = new []{{'a'}}.GroupBy(x => new Class1());
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void CallingListConvertAllAndUsingTypeArgumentForTThatHasImpureToStringMethod_KeepsMethodPure()
        {
            string code = $@"
using System;
using System.Linq;
using System.Collections.Generic;

{class1Code}
{isPureAttributeCode}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var result = new List<Class1>{{new Class1()}}.ConvertAll(x => 'a');
    }}
}}";
            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void CallingListConvertAllAndUsingTypeArgumentForTResultThatHasImpureToStringMethod_KeepsMethodPure()
        {
            string code = $@"
using System;
using System.Linq;
using System.Collections.Generic;

{class1Code}
{isPureAttributeCode}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var result = new List<char> {{'c'}}.ConvertAll(x => new Class1());
    }}
}}";
            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }
    }
}
