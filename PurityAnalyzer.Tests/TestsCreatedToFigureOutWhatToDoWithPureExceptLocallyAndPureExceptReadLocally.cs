using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests
{
    [TestFixture]
    public class TestsCreatedToFigureOutWhatToDoWithPureExceptLocallyAndPureExceptReadLocally
    {
        [Test]
        public void TryingToBreakPureExceptLocally_Test1()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    private int state;

    public void Increment() => state++;
    public int Read() => state;
}}

public static class B
{{
    public static readonly A A = new A();
}}

public class C
{{
    private readonly A a;

    public C()
    {{
        a = B.A;
    }}

    public void Increment() => a.Increment();
    public int Read() => a.Read();
}}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var c2 = new C();

        c2.Increment();
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive("DoSomething will cause global state to be mutated");
        }

        [Test]
        public void TryingToBreakPureExceptReadLocally_Test1()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    private int state;

    public void Increment() => state++;
    public int Read() => state;
}}

public static class B
{{
    public static readonly A A = new A();
}}

public class C
{{
    private readonly A a;

    public C()
    {{
        a = B.A;
    }}

    public void Increment() => a.Increment();
    public int Read() => a.Read();
}}

public static class Module1
{{
    [IsPure]
    public static int DoSomething(C c2)
    {{
        return c2.Read();
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive("DoSomething will read global state");
        }


        [Test]
        public void PureExceptReadLocally_Test1()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    private int state;

    public void Increment() => state++;
    public int Read() => state;
}}

public class C
{{
    private readonly A a;

    public C()
    {{
        a = new A();
    }}

    public void Increment() => a.Increment();
    public int Read() => a.Read();
}}

public static class Module1
{{
    [IsPure]
    public static int DoSomething(C c2)
    {{
        return c2.Read();
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0, "C.a is readonly and is initialized as a new object");
        }

        [Test]
        public void PureExceptReadLocally_Test2()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    private int state;

    public void Increment() => state++;
    public int Read() => state;
}}

public class C
{{
    private readonly A a;

    public C(A a2)
    {{
        a = a2;
    }}

    public void Increment() => a.Increment();
    public int Read() => a.Read();
}}

public static class Module1
{{
    [IsPure]
    public static int DoSomething(C c2)
    {{
        return c2.Read();
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive("C.a is initialized from a constructor parameter and so it could reference global state");
        }

        [Test]
        public void PureExceptReadLocally_Test3()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    private readonly int state = 1;

    public int Read() => state;
}}

public class C
{{
    private readonly A a;

    public C(A a2)
    {{
        a = a2;
    }}

    public int Read() => a.Read();
}}

public static class Module1
{{
    [IsPure]
    public static int DoSomething(C c2)
    {{
        return c2.Read();
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0, "A.state is readonly");
        }


        [Test]
        public void PureExceptLocally_Test1()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    public int state;
}}

public class C
{{
    public A Field = new A();
}}

public static class Module1
{{
    [IsPure]
    public static int DoSomething()
    {{
        var c = new C();
        
        c.Field = new A();

        return 0;
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void PureExceptLocally_Test2()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    public int state;
}}

public class C
{{
    public A Field = new A();
}}

public static class Module1
{{
    [IsPure]
    public static int DoSomething(A a)
    {{
        var c = new C();
        
        c.Field = a;

        c.Field.state = 1;

        return 0;
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void PureExceptLocally_Test3()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    public int state;
}}

public class C
{{
    public readonly A Field = new A();
}}

public static class Module1
{{
    [IsPure]
    public static int DoSomething()
    {{
        var c = new C();
        
        c.Field.state = 1;

        return 0;
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void PureExceptLocally_Test4()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    public int state;
}}

public class C
{{
    public A Field = new A();
}}

public static class Module1
{{
    [IsPure]
    public static int DoSomething()
    {{
        var c = new C();
        
        c.Field.state = 1;

        return 0;
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive("C.Field is public non-readonly");
        }

        [Test]
        public void PureExceptLocally_Test5()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class C
{{
    public int state;
}}

public static class Module1
{{
    [IsPure]
    public static int DoSomething()
    {{
        var c = new C();
        
        c.state = 1;

        return 0;
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void PureExceptLocally_Test6()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    public int state;
}}

public class C
{{
    public A Field = new A();
}}

public static class Module1
{{
    [IsPure]
    public static int DoSomething(A a)
    {{
        var c = new C();
        
        c.Field = a;

        return 0;
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void GlobalState_Test1()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    private int state;

    public void Increment() => state++;
    public int Read() => state;
}}

public static class B
{{
    public static readonly A A = new A();
}}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var a = B.A;

        a.Increment();
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive("DoSomething will cause global state to be mutated");
        }

        [Test]
        public void GlobalState_Test2()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    private int state;

    public void Increment() => state++;
    public int Read() => state;
}}

public static class B
{{
    public static readonly A A = new A();
}}

public static class Module1
{{
    [IsPure]
    public static int DoSomething()
    {{
        var a = B.A;

        return a.Read();
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive("DoSomething will read global state");
        }

    }
}