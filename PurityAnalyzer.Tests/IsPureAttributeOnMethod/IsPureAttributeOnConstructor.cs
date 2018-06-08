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

    }
}
