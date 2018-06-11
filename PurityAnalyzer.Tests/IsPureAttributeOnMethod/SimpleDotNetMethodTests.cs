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
        }
    }
}
