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
    public static string DoSomething(ImmutableDto1 input)
    {
        return input.Field1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());

            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void MethodThatReadsAReadWriteFieldOnParameterWhoseTypeIsCompiledIsImpure()
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
        return input.Field1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());

            dignostics.Length.Should().BePositive();
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());

            dignostics.Length.Should().BePositive();
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());

            dignostics.Length.Should().BePositive();
        }
    }
}
