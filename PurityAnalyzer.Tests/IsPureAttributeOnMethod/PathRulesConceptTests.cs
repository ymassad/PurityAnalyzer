using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    /// <summary>
    /// This test class is the result of exploring the concept of "paths" in purity analysis.
    /// </summary>
    [TestFixture]
    public class PathRulesConceptTests
    {
        [Test]
        public void MethodThatReadsGlobalMutableStateIndirectlyViaParameterPathIsImpure()
        {
            // This targets: "how do we know we are not accessing state inside some object
            // that was constructed with a copy of mutable global state?"
            //
            // Here the parameter owns an object, but that object contains a reference to static mutable state.
            // So the path effectively reads global state.
            string code = @"
using System;
using System.Collections.Generic;

public class IsPureAttribute : Attribute { }

public static class Globals
{
    public static List<int> GlobalList = new List<int> { 1, 2, 3 };
}

public class Dto
{
    public readonly List<int> Captured;

    public Dto()
    {
        // Captures global mutable state into instance state.
        Captured = Globals.GlobalList;
    }
}

public static class Module1
{
    [IsPure]
    public static int F(Dto input)
    {
        // Path: input.Captured.Count
        // This is really reading a global list through the parameter.
        return input.Captured.Count;
    }
}";
            Utilities.AssertImpure(code);
        }
    }
}