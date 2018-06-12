using System;
using System.Collections.Generic;
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
        [TestCaseSource(nameof(GetCases))]
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

        public static IEnumerable<string> GetCases()
        {
            yield return "1.ToString()";
            yield return "1f.ToString()";
            yield return "1d.ToString()";
            yield return "1m.ToString()";
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
        }
    }
}
