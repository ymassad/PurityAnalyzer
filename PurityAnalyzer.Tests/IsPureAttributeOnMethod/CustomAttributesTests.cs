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
    public class CustomAttributesTests
    {
        [Test]
        public void MethodDecoratedWithAttributeWhoseConstructorIsImpureIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Attribute1Attribute : Attribute
{
    static int state;
    public Attribute1Attribute()
    {
        state++;
    }
}

public static class Module1
{
    [IsPure]
    [Attribute1]
    public static int DoSomething()
    {
        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }
    }
}
