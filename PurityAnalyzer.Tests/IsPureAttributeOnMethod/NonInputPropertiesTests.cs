using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class NonInputPropertiesTests
    {
        [Test]
        public void MethodThatReadsAnInCodeReadWriteStaticPropertyIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static string Prop1 {get;set;}

    [IsPure]
    public static string DoSomething()
    {
        return Prop1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatReadsAnInCodeReadOnlyStaticPropertyIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static string Prop1 {get;} = ""str"";

    [IsPure]
    public static string DoSomething()
    {
        return Prop1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }
    }
}
