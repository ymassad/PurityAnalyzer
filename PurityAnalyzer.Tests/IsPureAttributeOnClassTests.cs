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
    public class IsPureAttributeOnClassTests
    {
        [Test]
        public void IsPureOnClassRequiresStaticMethodsToBePure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

[IsPure]
public class Class1
{
    static int c = 1;
    
    public static string DoSomething()
    {
        return c.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void IsPureOnClassRequiresInstanceMethodsToBePure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

[IsPure]
public class Class1
{
    int c = 1;
    
    public string DoSomething()
    {
        return c.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }
    }
}
