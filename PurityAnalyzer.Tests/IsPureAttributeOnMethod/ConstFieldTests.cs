using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class ConstFieldTests
    {
        [Test]
        public void MethodThatReadsAConstValueIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    const int c = 1;
    [IsPure]
    public static int DoSomething()
    {
        return c;
    }
}";

            Utilities.AssertPure(code);

        }
    }
}
