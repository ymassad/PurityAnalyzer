using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class CallingPureExceptLocallyMethodsAndPropertiesOnNewDotNetObjectsTests
    {
        [TestCaseSource(nameof(GetCases))]
        public void TestPureInvocation(string invocation)
        {
            string code = $@"
using System;
using System.Text;
using System.Xml;

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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.CreateFromType<XmlDocument>());
            dignostics.Length.Should().Be(0);
        }

        public static IEnumerable<string> GetCases()
        {
            yield return "new StringBuilder().AppendLine()";
            yield return "new XmlDocument().CreateElement(\"ele\").SetAttribute(\"attribute\", \"value\")";
        }
    }
}
