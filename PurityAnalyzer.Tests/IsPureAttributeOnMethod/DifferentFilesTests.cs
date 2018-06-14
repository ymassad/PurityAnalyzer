using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class DifferentFilesTests
    {
        [Test]
        public void TestCallingPureMethodInAnotherFile()
        {
            string file1Code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    
    [IsPure]
    public static string DoSomething()
    {
        return Module2.PureMethod();
    }
}";

            string file2Code = @"
using System;

public static class Module2
{
    public static string PureMethod()
    {
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(file1Code, file2Code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TestCallingImpureMethodInAnotherFile()
        {
            string file1Code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    
    [IsPure]
    public static string DoSomething()
    {
        return Module2.ImpureMethod();
    }
}";

            string file2Code = @"
using System;

public static class Module2
{
    static int state = 0;

    public static string ImpureMethod()
    {
        state++;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(file1Code, file2Code);
            dignostics.Length.Should().BePositive();

        }
    }
}
