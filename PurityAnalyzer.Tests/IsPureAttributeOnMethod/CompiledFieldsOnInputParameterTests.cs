using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class CompiledFieldsOnInputParameterTests
    {
        [Test]
        public void MethodThatReadsAReadOnlyFieldOnParameterWhoseTypeIsCompiledIsPure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(ImmutableDto1 input)
    {
        return input.Field1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }


        //Reading from mutable input is considered pure
        [Test]
        public void MethodThatReadsAReadWriteFieldOnParameterWhoseTypeIsCompiledIsPure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(MutableDto1 input)
    {
        return input.Field1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnParameterWhoseTypeIsCompiledIsImpure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(MutableDto1 input)
    {
        input.Field1 = 1;
        return """";
    }
}";

            Utilities.AssertImpure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatReadsAndWritesAReadWriteFieldOnParameterWhoseTypeIsCompiledIsImpure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(MutableDto1 input)
    {
        input.Field1 = input.Field1 + 1;
        return """";
    }
}";

            Utilities.AssertImpure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }
    }
}
