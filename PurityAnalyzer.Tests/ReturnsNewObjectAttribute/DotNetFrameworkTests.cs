using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using PurityAnalyzer.Tests.CompiledCsharpLib;

namespace PurityAnalyzer.Tests.ReturnsNewObjectAttribute
{
    [TestFixture]
    public class DotNetFrameworkTests
    {
        [Test]
        public void MethodThatCallsACompiledMethodWithTheIsPureAttributeIsPure()
        {
            string code = @"
using System;
using System.Collections.Immutable;

public class ReturnsNewObjectAttribute : Attribute
{
}

public static class Module1
{
    [ReturnsNewObjectAttribute]
    public static ImmutableArray<char> DoSomething(ImmutableArray<char> array)
    {
        return array.SetItem(0, 'b');
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(
                code, Utilities.GetAllReferencesNeededForType(typeof(ImmutableArray<>)));

            dignostics.Length.Should().Be(0);

        }
    }
}
