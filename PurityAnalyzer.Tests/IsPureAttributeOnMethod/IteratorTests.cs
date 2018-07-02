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
    public class IteratorTests
    {
        [Test]
        public void IteratorMethodThatReadsAConstantFieldIsPure()
        {
            string code = @"
using System;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    const int c = 1;
    [IsPure]
    public static IEnumerable<int> DoSomething()
    {
        yield return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void IteratorMethodThatWritesStaticFieldIsImpure()
        {
            string code = @"
using System;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    static int state = 0;

    [IsPure]
    public static IEnumerable<int> DoSomething()
    {
        yield return 1;

        state++;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatCallsAnIteratorMethodThatReadsAConstantFieldIsPure()
        {
            string code = @"
using System;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    const int c = 1;
    
    [IsPure]
    public static object DoSomethingElse() => DoSomething();

    public static IEnumerable<int> DoSomething()
    {
        yield return c;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatCallsAnIteratorMethodThatWritesStaticFieldIsImpure()
        {
            string code = @"
using System;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    static int state = 0;

    [IsPure]
    public static object DoSomethingElse() => DoSomething();

    public static IEnumerable<int> DoSomething()
    {
        yield return 1;

        state++;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }
    }
}
