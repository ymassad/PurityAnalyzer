using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class ExpressionBodiesMethodsTests
    {
        [Test]
        public void MethodWithImpureExpressionBodyIsConsideredImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    static int c = 1;
    [IsPure]
    public static int DoSomething() => c++;

}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void MethodWithPureExpressionBodyIsConsideredPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething() => 1;

}";

            Utilities.AssertPure(code);

        }
    }
}
