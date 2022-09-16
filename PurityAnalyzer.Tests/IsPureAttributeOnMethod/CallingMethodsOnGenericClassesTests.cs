using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using PurityAnalyzer.Tests.IsPureAttributeOnMethod.OverriddenMethods;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class CallingMethodsOnGenericClassesTests
    {
        [Test]
        public void CallingSimpleMethod_MethodIsPure()
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
        GenericClass<Class1>.Method(new Class1());
    }
}

public static class GenericClass<T>
{
    public static void Method(T param)
    {

    }
}

";

            Utilities.AssertPure(code);
        }

        [Test]
        public void CallingMethodWhereTHasNoConstraintsAndTypeArgumentToStringMethodIsImpure_AndToStringMethodOfTIsUsed_MethodIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1 
{
    public static int state = 0;

    public override string ToString()
    {
        state++;
        return """";
    }
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        GenericClass<Class1>.Method(new Class1());
    }
}

public static class GenericClass<T>
{
    public static void Method(T param)
    {
        var str = param.ToString();
    }
}

";

            Utilities.AssertImpure(code);
        }


        [Test]
        public void CallingMethodWhereTHasNoConstraintsAndTypeArgumentToStringMethodIsImpure_AndNoObjectMethodOfTIsUsed_MethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1 
{
    public static int state = 0;

    public override string ToString()
    {
        state++;
        return """";
    }
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        GenericClass<Class1>.Method(new Class1());
    }
}

public static class GenericClass<T>
{
    public static void Method(T param)
    {

    }
}";

            Utilities.AssertPure(code);
        }


        [Test]
        public void CallingCompiledMethodOnGenericClassWhereTHasNoConstraintsAndTypeArgumentToStringMethodIsImpure_AndTIsNotAnnotatedWithAttributes_MethodIsImpure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class IsPureAttribute : Attribute
{
}

public class Class1 
{
    public static int state = 0;

    public override string ToString()
    {
        state++;
        return """";
    }
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        GenericClassAndTIsUsedAsObject<Class1>.MethodThatUsesTAsObject(new Class1());
    }
}
";

            Utilities.AssertImpure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }


        [Test]
        public void CallingCompiledMethodOnGenericClassWhereTHasNoConstraintsAndTypeArgumentToStringMethodIsImpure_TIsAnnotatedWithNotUsedAsObjectAttribute_MethodIsPure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;


public class IsPureAttribute : Attribute
{
}

public class Class1 
{
    public static int state = 0;

    public override string ToString()
    {
        state++;
        return """";
    }
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        GenericClassAndTIsNotUsedAsObject<Class1>.MethodThatDoesNotUseTAsObject(new Class1());
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }


        [Test]
        public void CallingCompiledMethodOnGenericClass_MethodHasAttributeToIndicateItDoesNotUseTAsObject_PassingClassWithImpureToString_KeepsMethodPure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;


public class IsPureAttribute : Attribute
{
}

public class Class1 
{
    public static int state = 0;

    public override string ToString()
    {
        state++;
        return """";
    }
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        GenericClassWithSomeMethodsThatUseTAsObjectAndSomeDoNot<Class1>.MethodThatDoesNotUseTAsObject(new Class1());
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void CallingCompiledMethodOnGenericClass_MethodDoesNotHaveAttributeToIndicateItDoesNotUseTAsObject_PassingClassWithImpureToString_MakesMethodImpure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;


public class IsPureAttribute : Attribute
{
}

public class Class1 
{
    public static int state = 0;

    public override string ToString()
    {
        state++;
        return """";
    }
}

public static class Module1
{
    [IsPure]
    public static void DoSomething()
    {
        GenericClassWithSomeMethodsThatUseTAsObjectAndSomeDoNot<Class1>.MethodThatUsesTAsObject(new Class1());
    }
}";

            Utilities.AssertImpure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

    }
}
