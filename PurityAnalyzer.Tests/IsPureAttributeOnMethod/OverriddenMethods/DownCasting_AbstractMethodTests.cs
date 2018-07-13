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
    public class DownCasting_AbstractMethodTests
    {
        [Test]
        public void DownCastingFromObjectToTypeWithAbstractMethodMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    public abstract int Method();
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
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodWithAMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    public abstract int Method();
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
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodWithAMethodThatIsPureMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    public abstract int Method();
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
        public void DownCastingFromObjectToSealedTypeThatOverridesAnAbstractMethodWithAMethodThatIsPureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    public abstract int Method();
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
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodWithASealedMethodThatIsPureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    public abstract int Method();
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
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodWithASealedMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    public abstract int Method();
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithAMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    public abstract int Method();
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithAMethodThatIsPureMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    public abstract int Method();
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithASealedMethodThatIsPureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    public abstract int Method();
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithASealedMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public abstract class Base
{
    public abstract int Method();
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

    }
}
