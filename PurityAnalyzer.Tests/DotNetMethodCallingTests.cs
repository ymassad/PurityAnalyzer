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
    public class DotNetMethodCallingTests
    {
        [Test]
        public void CallingEnumerableLinqMethodsKeepsMethodPure()
        {
            string code = @"
using System;
using System.Linq;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(int[] data)
    {
        return
            data
                .Where(x => x > 0)
                .Select(x => x + 1)
                .SelectMany(x => new []{x, x * 2})
                .GroupBy(x => x > 2)
                .Select(x => x.Key.ToString())
                .First();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }
    }
}
