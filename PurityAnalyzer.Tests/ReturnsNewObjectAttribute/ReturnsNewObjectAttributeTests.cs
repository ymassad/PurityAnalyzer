using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.ReturnsNewObjectAttribute
{
    [TestFixture]
    public class ReturnsNewObjectAttributeTests
    {
        [Test]
        public void MethodThatReturnsInt_ReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static int DoSomething()
    {
        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsParameterDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething(Class1 class1)
    {
        return class1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsFieldDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    static Class1 class1;

    [ReturnsNewObject]
    public static Class1 DoSomething()
    {
        return class1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewObjectDirectlyReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething()
    {
        return new Class1();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsNewObjectStoredInVariableReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething()
    {
        var class1 = new Class1();
        return class1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsTheResultOfCallingAMethodThatDoesNotReturnANewObjectDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething()
    {
        return DoSomething2();
    }

    static Class1 class1;

    public static Class1 DoSomething2() => class1;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void ExpressionBodiedMethodThatReturnsTheResultOfCallingAMethodThatDoesNotReturnANewObjectDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething() => DoSomething2();

    static Class1 class1;

    public static Class1 DoSomething2() => class1;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatReturnsTheResultOfCallingAMethodThatReturnsANewObjectReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething()
    {
        return DoSomething2();
    }

    public static Class1 DoSomething2() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void ExpressionBodiedMethodThatReturnsTheResultOfCallingAMethodThatReturnsANewObjectReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething() => DoSomething2();

    public static Class1 DoSomething2() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsTheResultOfCallingACompiledMethodThatReturnsANewObjectReturnsNewObject()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class ReturnsNewObjectAttribute : Attribute
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static MutableDto1 DoSomething()
    {
        return StaticClass.CreateNew();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsTheResultOfCallingACompiledMethodThatDoesNotReturnANewObjectDoesNotReturnNewObject()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class ReturnsNewObjectAttribute : Attribute
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static MutableDto1 DoSomething()
    {
        return StaticClass.ReturnExisting();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void ReturnNewObjectAttributeCanBeAppliedOnInterfaceMethods()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public interface IInterface
{
    [ReturnsNewObject]
    Class1 DoSomething(Class1 class1);
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void ReturnNewObjectAttributeCanBeAppliedOnAbstractMethods()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public abstract class AbstractClass
{
    [ReturnsNewObject]
    public abstract Class1 DoSomething(Class1 class1);
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void AMethodThatReturnsAValueTypeNotByReference_ReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    public int Value;
}


public static class MyClass
{
    [ReturnsNewObjectAttribute]
    public static Struct1 DoSomething()
    {
        return new Struct1();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void AMethodThatReturnsAValueTypeFromInputByReference_DoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    public int Value;
}


public static class MyClass
{
    [ReturnsNewObjectAttribute]
    public static ref Struct1 DoSomething(ref Struct1 input)
    {
        return ref input;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewObjectConstructedUsingMethodParameterObjectDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
    Class2 class2;
    
    public Class1(Class2 class2) => this.class2 = class2;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething(Class2 param)
    {
        return new Class1(param);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewObjectConstructedUsingMethodParameterObjectIndirectlyByStoringTheObjectInAVariableDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
    Class2 class2;
    
    public Class1(Class2 class2) => this.class2 = class2;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething(Class2 param)
    {
        var v = new Class1(param);
        return v;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewObjectConstructedUsingMethodParameterObjectIndirectlyByStoringTheObjectInAVariableAndByStoringTheParameterInAVariableDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
    Class2 class2;
    
    public Class1(Class2 class2) => this.class2 = class2;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething(Class2 param)
    {
        var p = param;
        var v = new Class1(p);
        return v;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewObjectConstructedUsingMethodParameterObjectViaObjectInitializationSyntaxDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
    public Class2 class2;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething(Class2 param)
    {
        return new Class1() { class2 = param};
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewObjectConstructedUsingMethodParameterObjectViaFieldSetDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
    public Class2 class2;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething(Class2 param)
    {
        var obj = new Class1();
        obj.class2 = param;
        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewObjectConstructedUsingMethodParameterObjectViaPropertySetDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
    public Class2 class2 {get;set;}
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething(Class2 param)
    {
        var obj = new Class1();
        obj.class2 = param;
        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewObjectConstructedUsingMethodParameterObjectViaMethodDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
    public Class2 class2 {get;set;}

    public void Set(Class2 c) => class2 = c;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething(Class2 param)
    {
        var obj = new Class1();
        obj.Set(param);
        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterObjectDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    Class2 class2;
    
    public Struct1(Class2 class2) => this.class2 = class2;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Class2 param)
    {
        return new Struct1(param);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterObjectIndirectlyByStoringTheObjectInAVariableDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    Class2 class2;
    
    public Struct1(Class2 class2) => this.class2 = class2;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Class2 param)
    {
        var v = new Struct1(param);
        return v;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterObjectIndirectlyByStoringTheObjectInAVariableAndByStoringTheParameterInAVariableDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    Class2 class2;
    
    public Struct1(Class2 class2) => this.class2 = class2;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Class2 param)
    {
        var p = param;
        var v = new Struct1(p);
        return v;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterObjectViaObjectInitializationSyntaxDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    public Class2 class2;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Class2 param)
    {
        return new Struct1() { class2 = param};
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterObjectViaFieldSetDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    public Class2 class2;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Class2 param)
    {
        var obj = new Struct1();
        obj.class2 = param;
        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterObjectViaPropertySetDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    public Class2 class2 {get;set;}
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Class2 param)
    {
        var obj = new Struct1();
        obj.class2 = param;
        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterObjectViaMethodDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    public Class2 class2 {get;set;}

    public void Set(Class2 c) => class2 = c;
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Class2 param)
    {
        var obj = new Struct1();
        obj.Set(param);
        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterValueTypeReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    Struct2 struct2;
    
    public Struct1(Struct2 struct2) => this.struct2 = struct2;
}

public class Struct2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Struct2 param)
    {
        return new Struct1(param);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterValueTypeIndirectlyByStoringTheObjectInAVariableReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    Struct2 struct2;
    
    public Struct1(Struct2 struct2) => this.struct2 = struct2;
}

public class Struct2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Struct2 param)
    {
        var v = new Struct1(param);
        return v;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterValueTypeIndirectlyByStoringTheObjectInAVariableAndByStoringTheParameterInAVariableReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    Struct2 struct2;
    
    public Struct1(Struct2 struct2) => this.struct2 = struct2;
}

public class Struct2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Struct2 param)
    {
        var p = param;
        var v = new Struct1(p);
        return v;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterValueTypeViaObjectInitializationSyntaxReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    public Struct2 struct2;
}

public class Struct2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Struct2 param)
    {
        return new Struct1() { struct2 = param};
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterValueTypeViaFieldSetReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    public Struct2 struct2;
}

public class Struct2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Struct2 param)
    {
        var obj = new Struct1();
        obj.struct2 = param;
        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterValueTypetViaPropertySetReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    public Struct2 struct2 {get;set;}
}

public class Struct2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Struct2 param)
    {
        var obj = new Struct1();
        obj.struct2 = param;
        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsNewValueTypeConstructedUsingMethodParameterValueTypeViaMethodReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct1
{
    public Struct2 struct2 {get;set;}

    public void Set(Struct2 c) => struct2 = c;
}

public class Struct2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct1 DoSomething(Struct2 param)
    {
        var obj = new Struct1();
        obj.Set(param);
        return obj;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }



        [Test]
        public void MethodThatReturnsTupleConstructedUsingMethodParameterObjectDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}


public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static (Class2, Class2) DoSomething(Class2 param)
    {
        return (param, param);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsTupleConstructedUsingMethodParameterValueTypeReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}


public struct Struct2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static (Struct2, Struct2) DoSomething(Struct2 param)
    {
        return (param, param);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsTupleConstructedUsingMethodParameterObjectWrappedInsideStructDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}


public class Class2
{

}

public struct Struct1
{
    public Class2 class2;
}


public static class Module1
{
    [ReturnsNewObject]
    public static (Struct1, Struct1) DoSomething(Class2 param)
    {
        return (new Struct1 { class2 = param}, new Struct1 { class2 = param});
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsgMethodParameterObjectAsArray_Syntax1_DoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Class2[] DoSomething(Class2 param)
    {
        return new Class2[]{param};
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsgMethodParameterObjectAsArray_Syntax2_DoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Class2[] DoSomething(Class2 param)
    {
        return new []{param};
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsgMethodParameterObjectAsArray_Syntax3_DoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Class2[] DoSomething(Class2 param)
    {
        Class2[] result = {param, param};
        return result;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsArrayContaingNewObjectReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Class2[] DoSomething(Class2 param)
    {
        Class2[] result = {new Class2(), new Class2()};
        return result;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsgMethodParameterValueTypeAsArray_Syntax3_ReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public struct Struct2
{

}

public static class Module1
{
    [ReturnsNewObject]
    public static Struct2[] DoSomething(Struct2 param)
    {
        Struct2[] result = {param, param};
        return result;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }


    }
}
