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
                code,
                GetAllReferencesNeededForType(typeof(ImmutableArray<>)));

            dignostics.Length.Should().Be(0);

        }

        private static MetadataReference[] GetAllReferencesNeededForType(Type type)
        {
            var immutableCollectionsAssembly = type.Assembly;

            var files =
                immutableCollectionsAssembly.GetReferencedAssemblies()
                    .Select(x => Assembly.Load(x.FullName))
                    .Select(x => x.Location)
                    .ToList();

            files.Add(immutableCollectionsAssembly.Location);

            return files.Select(x => MetadataReference.CreateFromFile(x)).Cast<MetadataReference>().ToArray();
        }
    }
}
