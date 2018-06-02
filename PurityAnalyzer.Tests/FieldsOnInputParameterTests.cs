using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests
{
    [TestFixture]
    public class FieldsOnInputParameterTests
    {
        [Test]
        public void MethodThatReadsAReadOnlyFieldOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public readonly int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        
        return input.Field1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        
        return input.Field1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Field1 = 6;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatIncrementsAReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Field1 = input.Field1 + 1;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Field1++;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatIncrementsViaPlusEqualsAReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Field1 += 1;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }
    }
}
