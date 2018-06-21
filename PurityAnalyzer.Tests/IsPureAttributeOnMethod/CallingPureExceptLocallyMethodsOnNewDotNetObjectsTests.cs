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
    public class CallingPureExceptLocallyMethodsOnNewDotNetObjectsTests
    {
        [TestCaseSource(nameof(GetCases))]
        public void TestPureInvocation(string invocation)
        {
            string code = $@"
using System;
using System.Text;
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
            yield return "new System.Text.StringBuilder().AppendLine()";
        }
    }
}
