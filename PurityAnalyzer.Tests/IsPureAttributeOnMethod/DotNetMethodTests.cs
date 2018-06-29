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
    public class DotNetMethodTests
    {
        [Test]
        public void TestAccessOuterXmlPropertyOnInputParameterOfTypeXmlNode()
        {
            string code = @"
using System;
using System.Xml;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static void DoSomething(XmlNode node)
    {
        var output = node.OuterXml;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.CreateFromType<XmlDocument>());
            dignostics.Length.Should().Be(0);
        }

    }
}
