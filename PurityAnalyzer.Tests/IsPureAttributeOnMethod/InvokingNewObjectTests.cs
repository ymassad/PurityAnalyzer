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
    public class InvokingNewObjectTests
    {
        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public void Increment() => a++;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.Increment();

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        return instance.a;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.a = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.a++;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedObjectDirectlyIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public void Increment() => a++;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        new Class1().Increment();

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedObjectDirectlyIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        return new Class1().a;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedObjectDirectlyIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        new Class1().a = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedObjectDirectlyIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        new Class1().a++;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedObjectViaAnotherMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public void Increment() => a++;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        Create().Increment();

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        return Create().a;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        Create().a = 2;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        Create().a++;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public void Increment() => a++;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var class1 = Create();
        class1.Increment();

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var class1 = Create();
        return class1.a;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var class1 = Create();
        class1.a = 2;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var class1 = Create();
        class1.a++;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public int Prop => a++;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        return instance.Prop;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetOnNewlyCreatedObjectDirectlyIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public int Prop => a++;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        return new Class1().Prop;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetOnNewlyCreatedObjectViaAnotherMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public int Prop => a++;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        return Create().Prop;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public int Prop => a++;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var class1 = Create();
        return class1.Prop;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertySetOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public int Prop { set => a = value; }
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.Prop = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertySetOnNewlyCreatedObjectDirectlyIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public int Prop { set => a = value; }
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        new Class1().Prop = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertySetOnNewlyCreatedObjectViaAnotherMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public int Prop { set => a = value; }
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        Create().Prop = 2;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertySetOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public int Prop { set => a = value; }
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var class1 = Create();
        class1.Prop = 2;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertySetOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableTwiceIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public int Prop { set => a = value; }
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var class1 = Create();

        var instance = class1;
        
        instance.Prop = 2;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAnImpurePropertySetOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableTwiceIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public int Prop
    {
        set 
        {
            a = value;
            state++;
        }
    }

    static int state = 0;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var class1 = Create();

        var instance = class1;
        
        instance.Prop = 2;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnObjectAccessedViaReadWriteFieldOnNewlyCreatedObjectDirectlyIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public class Class0
{
    public Class1 class1 = new Class1();
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        return new Class0().class1.a;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnObjectAccessedViaReadWriteFieldOnNewlyCreatedObjectDirectlyIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public class Class0
{
    public Class1 class1 = new Class1();
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        new Class0().class1.a = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnAnObjectObtainedByAccesingAReadWriteFieldOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public void Increment() => a++;
}

public class Class0
{
    public Class1 class1 = new Class1();
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        new Class0().class1.Increment();

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnObjectAccessedViaAutoReadonlyPropertyOnNewlyCreatedObjectDirectlyIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public class Class0
{
    public Class1 class1 {get;} = new Class1();
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        return new Class0().class1.a;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnObjectAccessedViaReadonlyPropertyThatReturnsStaticObjectOnNewlyCreatedObjectDirectlyIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public class Class0
{
    public Class1 class1 => c1;

    static Class1 c1 = new Class1();
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        return new Class0().class1.a;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReadsAnElementOnMutableArrayObtainedFromFieldOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int[] arr = {1,2,3};
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        return instance.arr[0];
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAnElementOnMutableArrayObtainedFromFieldOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int[] arr = {1,2,3};
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.arr[0] = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAnElementOnMutableArrayObtainedFromReadOnlyAutoPropertyOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int[] arr {get;} = {1,2,3};
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        return instance.arr[0];
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAnElementOnMutableArrayObtainedFromReadOnlyAutoPropertyOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int[] arr {get;} = {1,2,3};
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.arr[0] = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAnElementOnMutableArrayObtainedFromReadWriteAutoPropertyOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int[] arr {get; set;} = {1,2,3};
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        return instance.arr[0];
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAnElementOnMutableArrayObtainedFromReadWriteAutoPropertyOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int[] arr {get; set;} = {1,2,3};
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.arr[0] = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAnElementOnMutableArrayObtainedFromReadWriteWithBackingFieldPropertyOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    private int[] _arr = {1,2,3};
    
    public int[] arr
    {
        get => _arr;
        set => _arr = value;
    }
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        return instance.arr[0];
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAnElementOnMutableArrayObtainedFromReadWriteWithBackingFieldPropertyOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    private int[] _arr = {1,2,3};
    
    public int[] arr
    {
        get => _arr;
        set => _arr = value;
    }
}


public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.arr[0] = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAnElementOnMutableArrayObtainedFromReadWriteWithBackingFieldPropertyOnNewlyCreatedObjectAndPropertyGetMutatesStaticFieldIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    private int[] _arr = {1,2,3};
    
    static int state = 0;

    public int[] arr
    {
        get 
        {
            state++;
            return _arr;
        }
        set => _arr = value;
    }
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        return instance.arr[0];
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatWritesAnElementOnMutableArrayObtainedFromReadWriteWithBackingFieldPropertyOnNewlyCreatedObjectAndPropertySetMutatesStaticFieldIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    private int[] _arr = {1,2,3};
    
    static int state = 0;

    public int[] arr
    {
        get => _arr;
        set
        {
            state++;
            _arr = value;
        }
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.arr[0] = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedObjectViaAnotherMethodWhichIsDefinedInAnotherFileIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public void Increment() => a++;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        Module2.Create().Increment();

        return 1;
    }
}";

            var codeInAnotherFile = @"
public static class Module2
{
    public static Class1 Create() => new Class1();
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code, codeInAnotherFile);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedObjectInDirectlyViaTwoMethodsWhichAreDefinedInAnotherFileIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public void Increment() => a++;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        Module2.Create().Increment();

        return 1;
    }
}";

            var codeInAnotherFile = @"
public static class Module2
{
    public static Class1 Create() => Create2();

    private static Class1 Create2() => new Class1();
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code, codeInAnotherFile);
            dignostics.Length.Should().Be(0);
        }

    }
}
