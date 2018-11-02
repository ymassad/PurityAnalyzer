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
    public class CallingGenericMethods
    {
        [Test]
        public void CallingSimpleGenericMethod()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1 
{
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        GenericMethod<Class1>(new Class1());
    }

    public static void GenericMethod<T>(T param)
    {

    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }
    }
}
