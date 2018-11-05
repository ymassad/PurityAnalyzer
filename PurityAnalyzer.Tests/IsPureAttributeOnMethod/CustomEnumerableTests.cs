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
    public class CustomEnumerableTests
    {
        [Test]
        public void MethodThatEnumeratesACustomEnumerableAndAllComponentsArePureIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass
{
    public EnumeratorClass GetEnumerator() => new EnumeratorClass();
}

public class EnumeratorClass
{
    public char Current => '1';

    public bool MoveNext() => true;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatEnumeratesACustomEnumerableAndTheGetEnumeratorMethodIsImpureIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass
{
    static int state = 0;
    public EnumeratorClass GetEnumerator()
    {   
        state++;
        return new EnumeratorClass();
    }
}

public class EnumeratorClass
{
    public char Current => '1';

    public bool MoveNext() => true;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatEnumeratesACustomEnumerableAndTheCurrentPropertyIsImpureIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass
{
    public EnumeratorClass GetEnumerator()
    {   
        return new EnumeratorClass();
    }
}

public class EnumeratorClass
{
    static int state = 0;

    public char Current
    {
        get
        {
            state++;

            return '1';
        }
    }

    public bool MoveNext() => true;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatEnumeratesACustomEnumerableAndTheMoveNextMethodIsImpureIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass
{
    public EnumeratorClass GetEnumerator()
    {   
        return new EnumeratorClass();
    }
}

public class EnumeratorClass
{
    static int state = 0;

    public char Current => '1';

    public bool MoveNext() => (state++) == 1;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatEnumeratesACustomEnumerableAndEnumeratorImplmenetsIDisposableAndAllComponentsArePureIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass
{
    public EnumeratorClass GetEnumerator() => new EnumeratorClass();
}

public class EnumeratorClass : IDisposable
{
    public char Current => '1';

    public bool MoveNext() => true;

    public void Dispose(){}
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatEnumeratesACustomEnumerableAndEnumeratorImplmenetsIDisposableAndTheDisposeMethodIsImpureIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass
{
    public EnumeratorClass GetEnumerator() => new EnumeratorClass();
}

public class EnumeratorClass : IDisposable
{
    public char Current => '1';

    public bool MoveNext() => true;

    static int state = 0;

    public void Dispose()
    {
        state++;
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatEnumeratesACustomEnumerableAndEnumeratorImplmenetsIDisposableExplicitlyAndTheDisposeMethodIsImpureIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass
{
    public EnumeratorClass GetEnumerator() => new EnumeratorClass();
}

public class EnumeratorClass : IDisposable
{
    public char Current => '1';

    public bool MoveNext() => true;

    static int state = 0;

    void IDisposable.Dispose()
    {
        state++;
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsIEnumerableAndAllComponentsArePureIsPure()
        {
            string code = @"
using System;
using System.Collections;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass : IEnumerable
{
    public IEnumerator GetEnumerator() => new EnumeratorClass();
}

public class EnumeratorClass : IEnumerator
{
    public object Current => '1';

    public bool MoveNext() => true;

    public void Reset()
    {
        
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsIEnumerableAndTheGetEnumeratorMethodIsImpureIsImpure()
        {
            string code = @"
using System;
using System.Collections;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass : IEnumerable
{
    static int state = 0;
    public IEnumerator GetEnumerator()
    {   
        state++;
        return new EnumeratorClass();
    }
}

public class EnumeratorClass : IEnumerator
{
    public object Current => '1';

    public bool MoveNext() => true;

    public void Reset()
    {
        
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsIEnumerableAndTheCurrentPropertyIsImpureIsImpure()
        {
            string code = @"
using System;
using System.Collections;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass : IEnumerable
{
    public IEnumerator GetEnumerator()
    {   
        return new EnumeratorClass();
    }
}

public class EnumeratorClass : IEnumerator
{
    static int state = 0;

    public object Current
    {
        get
        {
            state++;

            return '1';
        }
    }

    public bool MoveNext() => true;

    public void Reset()
    {
        
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsIEnumerableAndTheMoveNextMethodIsImpureIsImpure()
        {
            string code = @"
using System;
using System.Collections;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass : IEnumerable
{
    public IEnumerator GetEnumerator()
    {   
        return new EnumeratorClass();
    }
}

public class EnumeratorClass : IEnumerator
{
    static int state = 0;

    public object Current => '1';

    public bool MoveNext() => (state++) == 1;

    public void Reset()
    {
        
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsIEnumerableAndIDisposableAndAllComponentsArePureIsPure()
        {
            string code = @"
using System;
using System.Collections;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass : IEnumerable
{
    public IEnumerator GetEnumerator() => new EnumeratorClass();
}

public class EnumeratorClass : IEnumerator, IDisposable
{
    public object Current => '1';

    public bool MoveNext() => true;

    public void Dispose(){}

    public void Reset()
    {
        
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsIEnumerableAndIDisposableAndTheDisposeMethodIsImpureIsImpure()
        {
            string code = @"
using System;
using System.Collections;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass : IEnumerable
{
    public IEnumerator GetEnumerator() => new EnumeratorClass();
}

public class EnumeratorClass : IEnumerator, IDisposable
{
    public object Current => '1';

    public bool MoveNext() => true;

    static int state = 0;

    public void Dispose()
    {
        state++;
    }

    public void Reset()
    {
        
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsIEnumerableAndIDisposableExplicitlyAndTheDisposeMethodIsImpureIsImpure()
        {
            string code = @"
using System;
using System.Collections;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass : IEnumerable
{
    public IEnumerator GetEnumerator() => new EnumeratorClass();
}

public class EnumeratorClass : IEnumerator, IDisposable
{
    public object Current => '1';

    public bool MoveNext() => true;

    static int state = 0;

    void IDisposable.Dispose()
    {
        state++;
    }

    public void Reset()
    {
        
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsGenericIEnumerableAndAllComponentsArePureIsPure()
        {
            string code = @"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{
}



public class EnumerableClass : IEnumerable<char>
{
    public IEnumerator<char> GetEnumerator()
    {
        return new EnumeratorClass();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class EnumeratorClass : IEnumerator<char>
{
    public char Current => '1';

    public bool MoveNext() => true;

    public void Reset()
    {
        
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsGenericIEnumerableAndTheGetEnumeratorMethodIsImpureIsImpure()
        {
            string code = @"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{
}


public class EnumerableClass : IEnumerable<char>
{
    static int state = 0;

    public IEnumerator<char> GetEnumerator()
    {
        state++;
        return new EnumeratorClass();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class EnumeratorClass : IEnumerator<char>
{
    public char Current => '1';

    public bool MoveNext() => true;

    public void Reset()
    {
        
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsGenericIEnumerableAndTheCurrentPropertyIsImpureIsImpure()
        {
            string code = @"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass : IEnumerable<char>
{
    public IEnumerator<char> GetEnumerator()
    {
        return new EnumeratorClass();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class EnumeratorClass : IEnumerator<char>
{
    static int state = 0;

    public char Current
    {
        get
        {
            state++;

            return '1';
        }
    }

    public bool MoveNext() => true;

    public void Reset()
    {
        
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsGenericIEnumerableAndTheMoveNextMethodIsImpureIsImpure()
        {
            string code = @"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass : IEnumerable<char>
{
    public IEnumerator<char> GetEnumerator()
    {
        return new EnumeratorClass();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class EnumeratorClass : IEnumerator<char>
{
    static int state = 0;

    public char Current => '1';

    public bool MoveNext() => (state++) == 1;

    public void Reset()
    {
        
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatEnumeratesAnEnumerableClassThatImplementsGenericIEnumerableAndIDisposableAndTheDisposeMethodIsImpureIsImpure()
        {
            string code = @"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{
}

public class EnumerableClass : IEnumerable<char>
{
    public IEnumerator<char> GetEnumerator()
    {
        return new EnumeratorClass();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class EnumeratorClass : IEnumerator<char>
{
    static int state = 0;

    public char Current => '1';

    public bool MoveNext() => true;

    public void Reset()
    {
        
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        state++;
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        foreach (var item in new EnumerableClass())
        {
        }
        
        return 0;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }
}
