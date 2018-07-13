using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod.OverriddenMethods
{
    [TestFixture]
    public class DownCasting_VirtualMethodTests
    {
        [Test]
        public void DownCastingFromObjectToTypeWithVirtualPureMethodMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Base)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void DownCastingFromObjectToTypeWithVirtualImpureMethodKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    static int state = 0;
    public virtual int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Base)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void DownCastingFromObjectToTypeThatOverridesAPureVirtualMethodWithAMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    static int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void DownCastingFromObjectToTypeThatOverridesAPureVirtualMethodWithAMethodThatIsPureMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    public override int Method() => 2;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void DownCastingFromObjectToTypeThatOverridesAnImpureVirtualMethodWithAMethodThatIsPureMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    static int state = 0;
    public virtual int Method() => state++;
}

public class Derived : Base
{
    public override int Method() => 2;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void DownCastingFromObjectToSealedTypeThatOverridesAPureVirtualMethodWithAMethodThatIsPureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public sealed class Derived : Base
{
    public override int Method() => 2;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void DownCastingFromObjectToTypeThatOverridesAPureVirtualMethodWithASealedMethodThatIsPureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    public sealed override int Method() => 2;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void DownCastingFromObjectToTypeThatOverridesAPureVirtualMethodWithASealedMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    static int state = 0;
    public sealed override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void DownCastingFromObjectToTypeThatOverridesAnImpureVirtualMethodWithASealedMethodThatIsPureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    static int state = 0;
    public virtual int Method() => state++;
}

public class Derived : Base
{
    public sealed override int Method() => 2;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }



        [Test]
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithAMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Middle : Base
{
    static int state = 0;
    public override int Method() => state++;
}

public class Derived : Middle
{

}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithAMethodThatIsPureMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Middle : Base
{
    public override int Method() => 2;
}

public class Derived : Middle
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnImpureVirtualMethodWithAMethodThatIsPureMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    static int state = 0;
    public virtual int Method() => state++;
}

public class Middle : Base
{
    public override int Method() => 2;
}

public class Derived : Middle
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithASealedMethodThatIsPureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Middle : Base
{
    public sealed override int Method() => 2;
}

public class Derived : Middle
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithASealedMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Middle : Base
{
    static int state = 0;
    public sealed override int Method() => state++;
}

public class Derived : Middle
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnImpureVirtualMethodWithASealedMethodThatIsPureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    static int state = 0;
    public virtual int Method() => state++;
}

public class Middle : Base
{
    public sealed override int Method() => 2;
}

public class Derived : Middle
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


    }
}
