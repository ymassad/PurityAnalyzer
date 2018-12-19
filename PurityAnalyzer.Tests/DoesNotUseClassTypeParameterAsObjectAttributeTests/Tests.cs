using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.DoesNotUseClassTypeParameterAttributeTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void GenericMethodThatDoesNothing_DoesNotUseClassTypeParameterTAsObject()
        {
            string code = @"
using System;

public class DoesNotUseClassTypeParameterAsObjectAttribute : Attribute
{
    public string parameterName;

    public DoesNotUseClassTypeParameterAsObjectAttribute(string parameterName) => this.parameterName = parameterName;
}

public class Class1<T>
{
    [DoesNotUseClassTypeParameterAsObjectAttribute(nameof(T))]
    public void DoSomething(T input)
    {
        
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void GenericMethodThatCallsToStringOnT_UsesClassTypeParameterTAsObject()
        {
            string code = @"
using System;

public class DoesNotUseClassTypeParameterAsObjectAttribute : Attribute
{
    public string parameterName;

    public DoesNotUseClassTypeParameterAsObjectAttribute(string parameterName) => this.parameterName = parameterName;
}

public class Class1<T>
{
    [DoesNotUseClassTypeParameterAsObjectAttribute(nameof(T))]
    public void DoSomething(T input)
    {
        var result = input.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void GenericMethodThatCallsToStringOnMethodTypeParameterT_DoesNotUseClassTypeParameterTAsObject()
        {
            string code = @"
using System;

public class DoesNotUseClassTypeParameterAsObjectAttribute : Attribute
{
    public string parameterName;

    public DoesNotUseClassTypeParameterAsObjectAttribute(string parameterName) => this.parameterName = parameterName;
}

public class Class1<T>
{
    [DoesNotUseClassTypeParameterAsObjectAttribute(nameof(T))]
    public void DoSomething<T>(T input)
    {
        var result = input.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

    }
}

