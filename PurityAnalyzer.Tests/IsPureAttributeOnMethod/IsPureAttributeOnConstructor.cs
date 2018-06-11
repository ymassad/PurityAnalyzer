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
    public class IsPureAttributeOnConstructor
    {
        [Test]
        public void EmptyInstanceConstructorIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    [IsPure]
    public Class1() {}
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void InstanceConstructorIsImpureIfThereIsAnImpureInstanceFieldInitializer()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    int field = Utils.ImpureMethod();
    
    [IsPure]
    public Class1() {}
}

public static class Utils
{
    static int state;
    public static int ImpureMethod() => state++;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void InstanceConstructorIsImpureIfThereIsAnImpureInstancePropertyInitializer()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    int prop {get;} = Utils.ImpureMethod();
    
    [IsPure]
    public Class1() {}
}

public static class Utils
{
    static int state;
    public static int ImpureMethod() => state++;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void InstanceConstructorIsImpureIfThereIsAnImpureStaticFieldInitializer()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int field = Utils.ImpureMethod();
    
    [IsPure]
    public Class1() {}
}

public static class Utils
{
    static int state;
    public static int ImpureMethod() => state++;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void InstanceConstructorIsImpureIfThereIsAnImpureStaticPropertyInitializer()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int prop {get;} = Utils.ImpureMethod();
    
    [IsPure]
    public Class1() {}
}

public static class Utils
{
    static int state;
    public static int ImpureMethod() => state++;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }




        [Test]
        public void StaticConstructorIsPureEvenIfThereIsAnImpureInstanceFieldInitializer()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    int field = Utils.ImpureMethod();
    
    [IsPure]
    static Class1() {}
}

public static class Utils
{
    static int state;
    public static int ImpureMethod() => state++;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void StaticConstructorIsPureEvenIfThereIsAnImpureInstancePropertyInitializer()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    int prop {get;} = Utils.ImpureMethod();
    
    [IsPure]
    static Class1() {}
}

public static class Utils
{
    static int state;
    public static int ImpureMethod() => state++;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void StaticConstructorIsImpureIfThereIsAnImpureStaticFieldInitializer()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int field = Utils.ImpureMethod();
    
    [IsPure]
    static Class1() {}
}

public static class Utils
{
    static int state;
    public static int ImpureMethod() => state++;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void StaticConstructorIsImpureIfThereIsAnImpureStaticPropertyInitializer()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int prop {get;} = Utils.ImpureMethod();
    
    [IsPure]
    static Class1() {}
}

public static class Utils
{
    static int state;
    public static int ImpureMethod() => state++;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        //reading and writing fields in constructors
        [Test]
        public void InstanceConstructorIsPureEvenIfItReadsAndWritesAnInstanceReadWriteField()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    int field;
    
    [IsPure]
    public Class1()
    {
        field = 1;
        field = field + 1;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void InstanceConstructorIsPureEvenIfItReadsAndWritesAnInstanceReadOnlyField()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    readonly int field;
    
    [IsPure]
    public Class1()
    {
        field = 1;
        field = field + 1;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void StaticConstructorIsPureEvenIfItReadsAndWritesAStaticReadWriteField()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int field;
    
    [IsPure]
    static Class1()
    {
        field = 1;
        field = field + 1;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void StaticConstructorIsPureEvenIfItReadsAndWritesAStaticReadOnlyField()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static readonly int field;
    
    [IsPure]
    static Class1()
    {
        field = 1;
        field = field + 1;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void StaticConstructorIsImpureIfItReadsAStaticReadWriteFieldInAnotherClass()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    [IsPure]
    static Class1()
    {
        var a = AnotherClass.field;
    }
}

public static class AnotherClass
{
    public static int field;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void StaticConstructorIsImpureIfItWritesAStaticReadWriteFieldInAnotherClass()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    [IsPure]
    static Class1()
    {
        AnotherClass.field = 1;
    }
}

public static class AnotherClass
{
    public static int field;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void InstanceConstructorIsImpureIfItWritesAStaticReadWriteFieldInAnotherClass()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    [IsPure]
    public Class1()
    {
        AnotherClass.field = 1;
    }
}

public static class AnotherClass
{
    public static int field;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void InstanceConstructorIsImpureIfItReadsAStaticReadWriteFieldInAnotherClass()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    [IsPure]
    public Class1()
    {
        var a = AnotherClass.field;
    }
}

public static class AnotherClass
{
    public static int field;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void StaticConstructorIsPureEvenIfItReadsAStaticReadOnlyFieldInAnotherClass()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    [IsPure]
    static Class1()
    {
        var a = AnotherClass.field;
    }
}

public static class AnotherClass
{
    public readonly static int field = 1;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void InstanceConstructorIsPureEvenIfItReadsAStaticReadOnlyFieldInAnotherClass()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    [IsPure]
    public Class1()
    {
        var a = AnotherClass.field;
    }
}

public static class AnotherClass
{
    public readonly static int field = 1;
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void InstanceConstructorIsPureIfItReadsAStaticReadOnlyField()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static readonly int field;
    
    [IsPure]
    public Class1()
    {
        var a = field;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void InstanceConstructorIsImpureIfItReadsAStaticReadWriteField()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int field;
    
    [IsPure]
    public Class1()
    {
        var a = field;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void InstanceConstructorIsImpureIfItWritesAStaticReadWriteField()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int field;
    
    [IsPure]
    public Class1()
    {
        field = 1;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }

}
