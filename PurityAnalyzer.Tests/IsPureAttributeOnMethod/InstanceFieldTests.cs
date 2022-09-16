using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class InstanceFieldTests
    {
        [Test]
        public void MethodThatReadsAReadonlyInstanceFieldIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    readonly int c = 1;
    [IsPure]
    public int DoSomething()
    {
        return c;
    }
}";

            Utilities.AssertPure(code);

        }

        [Test]
        public void MethodThatReadsAReadWriteInstanceFieldIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    int c = 1;
    [IsPure]
    public int DoSomething()
    {
        return c;
    }
}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void MethodThatWritesAnInstanceFieldIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    int c = 1;
    [IsPure]
    public int DoSomething()
    {
        c = 2;
        return 1;
    }
}";

            Utilities.AssertImpure(code);

        }
    }
}
