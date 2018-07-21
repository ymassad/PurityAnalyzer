using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class SimpleDotNetMethodTests
    {
        [TestCaseSource(nameof(GetPureCases))]
        public void TestPureInvocation(string invocation)
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute
{{
}}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        {invocation};
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [TestCaseSource(nameof(GetImpureCases))]
        public void TestImpureInvocation(string invocation)
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute
{{
}}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        {invocation};
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        public static IEnumerable<string> GetPureCases()
        {
            yield return "true.ToString()";
            yield return @"Guid.Parse(""41C19760-DF17-4499-A992-F8D8423B2294"")";
            yield return @"Guid.Parse(""41C19760-DF17-4499-A992-F8D8423B2294"").ToString()";
            yield return @"String.Join("","", new []{""1"", ""2""})";
            yield return @"String.Equals(""1"", ""2"")";
            yield return @"""1"".Equals(""2"")";
            yield return @"String.Equals(""1"", ""2"", StringComparison.Ordinal)";
            yield return @"""1"".Equals(""2"", StringComparison.Ordinal)";
            yield return @"String.Equals(""1"", ""2"", StringComparison.OrdinalIgnoreCase)";
            yield return @"""1"".Equals(""2"", StringComparison.OrdinalIgnoreCase)";
            yield return @"var a = ((int?)1).HasValue";
            yield return @"var a = ((int?)1).Value";
            yield return @"var a = string.Empty";
        }

        public static IEnumerable<string> GetImpureCases()
        {
            yield return @"1.ToString()";
        }
    }
}
