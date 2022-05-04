using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class SystemObjectTests
    {
        [Test]
        public void AccessingObjectGetHashCodeOnParameterOfTypeObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(object o)
    {
        return o.GetHashCode();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void AccessingObjectGetHashCodeOnNewObjectOfTypeObjectIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        var o = new object();

        return o.GetHashCode();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void AccessingGetHashCodeOnParameterOfTypeThatInheritsFromObjectAndDoesNotOverrideMethodIsPure()
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
    public static int DoSomething(Class1 o)
    {
        return o.GetHashCode();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void AccessingGetHashCodeOnNewClassThatInheritsFromObjectAndDoesNotOverrideGetHashCodeIsImpure()
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
    public static int DoSomething()
    {
        var o = new Class1();

        return o.GetHashCode();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        //TODO: what happens if we cast a method into a delegate and pass it to a function when the method returns new object? Or when it is pure except locally, etc.

        [Test]
        public void AccessingObjectGetHashCodeOnNewObjectOfTypeObjectIsImpure_ObjectCreatedInAnotherMethod()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        var o = Create();

        return o.GetHashCode();
    }
    
    public static object Create() => new object();

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void AccessingObjectToStringOnNewObjectOfTypeObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var o = new object();

        return o.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void PassingNewObjectToMethodAccessingObjectGetHashCodeOnParameterOfTypeObjectIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething2()
    {
        return DoSomething(new Object());
    }

    public static int DoSomething(object o)
    {
        return o.GetHashCode();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        [Test]
        public void PassingNewObjectToMethodAccessingObjectGetHashCodeOnParameterOfTypeObjectIsImpure_ObjectCreatedUsingAnotherMethod()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int DoSomething2()
    {
        return DoSomething(Create());
    }

    public static int DoSomething(object o)
    {
        return o.GetHashCode();
    }
    
    public static object Create() => new object();

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CallingDistinctAndCountOnArrayOfNewObjectsDefiningPureGetHashCodeAndEquals_MethodIsPure()
        {
            string code = @"
using System;
using System.Linq;
using System.Collections.Immutable;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int Age {get;}
    public int Size {get;}

    public Class1(int age, int size)
    {
        this.Age = age;
        this.Size = size;
    }

    public override int GetHashCode()
    {
        return Age.GetHashCode() * Size.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is Class1 c && Age == c.Age && Size == c.Size;
    }

}

public static class Module1
{
    [IsPure]
    public static int DoSomething2()
    {
        var o1 = new Class1(15, 20);
        var o2 = new Class1(16, 30);
        
        return ImmutableArray.Create(o1, o2).Distinct().Count();
    }

}";

            var dignostics = Utilities.RunPurityAnalyzerWithImmutableCollections(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CallingDistinctAndCountOnArrayOfNewObjectsNotDefiningGetHashCodeAndEquals_MethodIsPure()
        {
            string code = @"
using System;
using System.Linq;
using System.Collections.Immutable;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int Age {get;}
    public int Size {get;}

    public Class1(int age, int size)
    {
        this.Age = age;
        this.Size = size;
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething2()
    {
        var o1 = new Class1(15, 20);
        var o2 = new Class1(16, 30);
        
        return ImmutableArray.Create(o1, o2).Distinct().Count();
    }

}";

            var dignostics = Utilities.RunPurityAnalyzerWithImmutableCollections(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CallingDistinctAndCountOnArrayOfNewObjectsDefiningGetHashCodeAndEquals_GetHashCodeIsImpure_MethodIsImpure()
        {
            string code = @"
using System;
using System.Linq;
using System.Collections.Immutable;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int Age {get;}
    public int Size {get;}

    public Class1(int age, int size)
    {
        this.Age = age;
        this.Size = size;
    }

    public static int state = 0;

    public override int GetHashCode()
    {
        state++;
        return Age.GetHashCode() * Size.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is Class1 c && Age == c.Age && Size == c.Size;
    }

}

public static class Module1
{
    [IsPure]
    public static int DoSomething2()
    {
        var o1 = new Class1(15, 20);
        var o2 = new Class1(16, 30);
        
        return ImmutableArray.Create(o1, o2).Distinct().Count();
    }

}";

            var dignostics = Utilities.RunPurityAnalyzerWithImmutableCollections(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CallingDistinctAndCountOnArrayOfNewObjectsDefiningGetHashCodeAndEquals_GetHashCodeIsImpureBecauseItCallsGetHashCodeOnNewObject_MethodIsImpure()
        {
            string code = @"
using System;
using System.Linq;
using System.Collections.Immutable;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int Age {get;}
    public int Size {get;}

    public Class1(int age, int size)
    {
        this.Age = age;
        this.Size = size;
    }

    public static int state = 0;

    public override int GetHashCode()
    {
        return new object().GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is Class1 c && Age == c.Age && Size == c.Size;
    }

}

public static class Module1
{
    [IsPure]
    public static int DoSomething2()
    {
        var o1 = new Class1(15, 20);
        var o2 = new Class1(16, 30);
        
        return ImmutableArray.Create(o1, o2).Distinct().Count();
    }

}";

            var dignostics = Utilities.RunPurityAnalyzerWithImmutableCollections(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CallingDistinctAndCountOnArrayOfNewObjectsDefiningGetHashCodeAndEquals_GetHashCodeIsImpureBecauseItCallsGetHashCodeOnNewObjectCreatedInAnotherMethod_MethodIsImpure()
        {
            string code = @"
using System;
using System.Linq;
using System.Collections.Immutable;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int Age {get;}
    public int Size {get;}

    public Class1(int age, int size)
    {
        this.Age = age;
        this.Size = size;
    }

    public static int state = 0;

    public override int GetHashCode()
    {
        return CreateObject().GetHashCode();
    }

    public static object CreateObject() => new object();

    public override bool Equals(object obj)
    {
        return obj is Class1 c && Age == c.Age && Size == c.Size;
    }

}

public static class Module1
{
    [IsPure]
    public static int DoSomething2()
    {
        var o1 = new Class1(15, 20);
        var o2 = new Class1(16, 30);
        
        return ImmutableArray.Create(o1, o2).Distinct().Count();
    }

}";

            var dignostics = Utilities.RunPurityAnalyzerWithImmutableCollections(code);
            dignostics.Length.Should().BePositive();

        }

        //TODO: add more tests related to ObjectGetHashCode purity

        [Test]
        public void CallingDistinctAndCountOnArrayOfNewObjectsDefiningGetHashCodeAndEquals_EqualsIsImpure_MethodIsImpure()
        {
            string code = @"
using System;
using System.Linq;
using System.Collections.Immutable;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int Age {get;}
    public int Size {get;}

    public Class1(int age, int size)
    {
        this.Age = age;
        this.Size = size;
    }

    public static int state = 0;

    public override int GetHashCode()
    {        
        return Age.GetHashCode() * Size.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        state++;
        return obj is Class1 c && Age == c.Age && Size == c.Size;
    }

}

public static class Module1
{
    [IsPure]
    public static int DoSomething2()
    {
        var o1 = new Class1(15, 20);
        var o2 = new Class1(16, 30);
        
        return ImmutableArray.Create(o1, o2).Distinct().Count();
    }

}";

            var dignostics = Utilities.RunPurityAnalyzerWithImmutableCollections(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CallingDistinctAndCountOnArrayOfNewObjectsDefiningGetHashCodeAndEqualsBasedOnObjectTypedMember_MethodIsPure()
        {
            string code = @"
using System;
using System.Linq;
using System.Collections.Immutable;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public object Member {get;}

    public Class1(object member)
    {
        this.Member = member;
    }

    public override int GetHashCode()
    {
        return Member.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is Class1 c && Member == c.Member;
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething2()
    {
        var o1 = new Class1(new object());
        var o2 = new Class1(new object());
        
        return ImmutableArray.Create(o1, o2).Distinct().Count();
    }

}";

            var dignostics = Utilities.RunPurityAnalyzerWithImmutableCollections(code);
            dignostics.Length.Should().Be(0);

        }

    }
}
