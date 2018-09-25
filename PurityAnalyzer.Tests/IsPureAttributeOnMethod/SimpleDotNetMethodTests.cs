using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class SimpleDotNetMethodTests
    {
        [TestCaseSource(nameof(GetPureCases))]
        public void TestPureInvocation(string invocation)
        {
            string code = $@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class IsPureAttribute : Attribute
{{
}}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        {invocation};
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetAllReferencesNeededForType(typeof(ImmutableArray<>)));
            dignostics.Length.Should().Be(0);
        }

        [TestCaseSource(nameof(GetImpureCases))]
        public void TestImpureInvocation(string invocation)
        {
            string code = $@"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{{
}}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        {invocation};
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [TestCaseSource(nameof(GetPureExceptReadLocallyCases))]
        public void TestPureExceptReadLocallyInvocation((string objectType, string constructionArguments, string invocation) caseInfo)
        {
            string code = $@"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{{
}}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var obj = new {caseInfo.objectType}{caseInfo.constructionArguments};
        
        {caseInfo.invocation};
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

            string code2 = $@"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{{
}}

public static class Module1
{{
    [IsPure]
    public static void DoSomething({caseInfo.objectType} obj)
    {{
        {caseInfo.invocation};
    }}
}}";

            var dignostics2 = Utilities.RunPurityAnalyzer(code2);
            dignostics2.Length.Should().Be(0);

            string code3 = $@"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{{
}}

public static class Module1
{{
    private static readonly {caseInfo.objectType} obj;

    [IsPure]
    public static void DoSomething()
    {{
        {caseInfo.invocation};
    }}
}}";

            var dignostics3 = Utilities.RunPurityAnalyzer(code3);
            dignostics3.Length.Should().BePositive();
        }

        [TestCaseSource(nameof(GetPureExceptLocallyCases))]
        public void TestPureExceptLocallyInvocation((string objectType, string constructionArguments, string invocation) caseInfo)
        {
            string code = $@"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{{
}}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var obj = new {caseInfo.objectType}{caseInfo.constructionArguments};
        
        {caseInfo.invocation};
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

            string code2 = $@"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{{
}}

public static class Module1
{{
    [IsPure]
    public static void DoSomething({caseInfo.objectType} obj)
    {{
        {caseInfo.invocation};
    }}
}}";

            var dignostics2 = Utilities.RunPurityAnalyzer(code2);
            dignostics2.Length.Should().BePositive();

            string code3 = $@"
using System;
using System.Collections;
using System.Collections.Generic;

public class IsPureAttribute : Attribute
{{
}}

public static class Module1
{{
    private static readonly {caseInfo.objectType} obj;

    [IsPure]
    public static void DoSomething()
    {{
        {caseInfo.invocation};
    }}
}}";

            var dignostics3 = Utilities.RunPurityAnalyzer(code3);
            dignostics3.Length.Should().BePositive();
        }

        public static IEnumerable<(string objectType, string constructionArguments, string invocation)>
            GetPureExceptLocallyCases()
        {
            yield return ("List<char>", "()", "obj.Capacity = 5");
            yield return ("List<char>", "()", "obj[0] = 'a'");
            yield return ("List<char>", "()", "obj.Add('a')");
            yield return ("List<char>", "()", "obj.AddRange(new []{'a'})");
            yield return ("List<char>", "()", "obj.Clear()");
            yield return ("List<char>", "()", "obj.Insert(0, 'a')");
            yield return ("List<char>", "()", "obj.InsertRange(0, new []{'a'})");
            yield return ("List<char>", "()", "obj.RemoveAll(x => x == 'a')");
            yield return ("List<char>", "()", "obj.RemoveAt(0)");
            yield return ("List<char>", "()", "obj.RemoveRange(0, 2)");
            yield return ("List<char>", "()", "obj.Reverse()");
            yield return ("List<char>", "()", "obj.Reverse(0, 2)");
            yield return ("List<char>", "()", "obj.Sort((x,y) => 0)");
            yield return ("List<char>", "()", "obj.TrimExcess()");

            yield return ("System.Text.StringBuilder", "()", "obj.Append(string.Empty)");
            yield return ("System.Text.StringBuilder", "()", @"obj.Append("""")");
        }

        public static IEnumerable<(string objectType, string constructionArguments, string invocation)>
            GetPureExceptReadLocallyCases()
        {
            yield return ("List<char>", "()", "var a = obj.Capacity");
            yield return ("List<char>", "()", "var a = obj.Count");
            yield return ("List<char>", "()", "var a = obj[0]");
            yield return ("List<char>", "()", "var a = obj.ConvertAll(x => (int)x)");
            yield return ("List<char>", "()", "var a = obj.Exists(x => x == 'a')");
            yield return ("List<char>", "()", "var a = obj.Find(x => x == 'a')");
            yield return ("List<char>", "()", "var a = obj.FindAll(x => x == 'a')");
            yield return ("List<char>", "()", "var a = obj.FindIndex(x => x == 'a')");
            yield return ("List<char>", "()", "var a = obj.FindIndex(0, x => x == 'a')");
            yield return ("List<char>", "()", "var a = obj.FindIndex(0, 2, x => x == 'a')");
            yield return ("List<char>", "()", "var a = obj.FindLast(x => x == 'a')");
            yield return ("List<char>", "()", "var a = obj.FindLastIndex(x => x == 'a')");
            yield return ("List<char>", "()", "var a = obj.FindLastIndex(0, x => x == 'a')");
            yield return ("List<char>", "()", "var a = obj.FindLastIndex(0, 2, x => x == 'a')");
            yield return ("List<char>", "()", "var a = obj.GetEnumerator()");
            yield return ("List<char>", "()", "var a = (IEnumerable<char>) obj");
            yield return ("List<char>", "()", "var a = (IEnumerable) obj");
            yield return ("List<char>", "()", "var a = obj.GetRange(0, 2)");
            yield return ("List<char>", "()", "var a = obj.ToArray()");
            yield return ("List<char>", "()", "var a = obj.TrueForAll(x => x == 'a')");


            yield return ("char[]", "{'a'}", "var a = (IEnumerable<char>) obj");
            yield return ("char[]", "{'a'}", "var a = (IEnumerable) obj");


            yield return ("System.Text.StringBuilder", "()", "var a = obj.ToString()");

        }

        public static IEnumerable<string> GetPureCases()
        {
            foreach (var c in GetPureCasesForInt32()) yield return c;

            foreach (var c in GetPureCasesForUInt32()) yield return c;

            foreach (var c in GetPureCasesForInt64()) yield return c;

            foreach (var c in GetPureCasesForUInt64()) yield return c;

            foreach (var c in GetPureCasesForInt16()) yield return c;

            foreach (var c in GetPureCasesForUInt16()) yield return c;

            foreach (var c in GetPureCasesForBoolean()) yield return c;

            foreach (var c in GetPureCasesForByte()) yield return c;

            foreach (var c in GetPureCasesForSByte()) yield return c;

            foreach (var c in GetPureCasesForIntPtr()) yield return c;

            foreach (var c in GetPureCasesForUIntPtr()) yield return c;

            foreach (var c in GetPureCasesForChar()) yield return c;

            foreach (var c in GetPureCasesForString()) yield return c;

            foreach (var c in GetPureCasesForSingle()) yield return c;

            foreach (var c in GetPureCasesForDouble()) yield return c;

            foreach (var c in GetPureCasesForGuid()) yield return c;

            foreach (var c in GetPureCasesForList()) yield return c;

            foreach (var c in GetPureCasesForDateTime()) yield return c;

            foreach (var c in GetPureCasesForImmutableArray()) yield return c;

            yield return @"var a = ((int?)1).HasValue";
            yield return @"var a = ((int?)1).Value";

            yield return @"var a = StringComparison.Ordinal == StringComparison.Ordinal";
            yield return @"var a = StringComparison.Ordinal != StringComparison.Ordinal";

        }

        public static IEnumerable<string> GetPureCasesForInt32()
        {
            yield return @"var a = 1 + 1";
            yield return @"var a = 1 - 1";
            yield return @"var a = 1 * 1";
            yield return @"var a = 1 / 1";
            yield return @"var a = 1 % 1";
            yield return @"var a = 1 > 1";
            yield return @"var a = 1 < 1";
            yield return @"var a = 1 >= 1";
            yield return @"var a = 1 <= 1";
            yield return @"var a = 1 == 1";
            yield return @"var a = 1 != 1";
            yield return @"var a = 1.Equals(1)";
            yield return @"var a = 1.Equals(new object())";
            yield return @"var b = 1; var a = -b";
            yield return @"var b = 1; var a = +b";
            yield return @"var b = 1; var a = --b";
            yield return @"var b = 1; var a = ++b";
            yield return @"var b = 1; var a = b++";
            yield return @"var b = 1; var a = b--";
            yield return @"var b = 1; var a = ~b";
            yield return @"var a = 1 >> 1";
            yield return @"var a = 1 << 1";
            yield return @"var a = 1 & 1";
            yield return @"var a = 1 ^ 1";
            yield return @"var a = 1 | 1";
            yield return @"var a = 1; a += 1";
            yield return @"var a = 1; a *= 1";
            yield return @"var a = 1; a /= 1";
            yield return @"var a = 1; a %= 1";
            yield return @"var a = 1; a -= 1";
            yield return @"var a = 1; a &= 1";
            yield return @"var a = 1; a |= 1";
            yield return @"var a = 1; a <<= 1";
            yield return @"var a = 1; a >>= 1";
            yield return @"var a = 1; a ^= 1";

            yield return @"var a = 1.CompareTo(1)";
            yield return @"var a = 1.CompareTo(new object())";
            yield return @"var a = 1.GetHashCode()";
            yield return @"var a = 1.GetTypeCode()";
        }

        public static IEnumerable<string> GetPureCasesForUInt32()
        {
            yield return @"var a = 1u + 1u";
            yield return @"var a = 1u - 1u";
            yield return @"var a = 1u * 1u";
            yield return @"var a = 1u / 1u";
            yield return @"var a = 1u % 1u";
            yield return @"var a = 1u > 1u";
            yield return @"var a = 1u < 1u";
            yield return @"var a = 1u >= 1u";
            yield return @"var a = 1u <= 1u";
            yield return @"var a = 1u == 1u";
            yield return @"var a = 1u != 1u";
            yield return @"var a = 1u.Equals(1u)";
            yield return @"var a = 1u.Equals(new object())";
            yield return @"var b = 1u; var a = +b";
            yield return @"var b = 1u; var a = --b";
            yield return @"var b = 1u; var a = ++b";
            yield return @"var b = 1u; var a = b++";
            yield return @"var b = 1u; var a = b--";
            yield return @"var b = 1u; var a = ~b";
            yield return @"var a = 1u >> 1";
            yield return @"var a = 1u << 1";
            yield return @"var a = 1u & 1u";
            yield return @"var a = 1u ^ 1u";
            yield return @"var a = 1u | 1u";
            yield return @"var a = 1u; a += 1u";
            yield return @"var a = 1u; a *= 1u";
            yield return @"var a = 1u; a /= 1u";
            yield return @"var a = 1u; a %= 1u";
            yield return @"var a = 1u; a -= 1u";
            yield return @"var a = 1u; a &= 1u";
            yield return @"var a = 1u; a |= 1u";
            yield return @"var a = 1u; a <<= 1";
            yield return @"var a = 1u; a >>= 1";
            yield return @"var a = 1u; a ^= 1u";

            yield return @"var a = 1u.CompareTo(1u)";
            yield return @"var a = 1u.CompareTo(new object())";
            yield return @"var a = 1u.GetHashCode()";
            yield return @"var a = 1u.GetTypeCode()";
        }

        public static IEnumerable<string> GetPureCasesForInt64()
        {
            yield return @"var a = 1L + 1L";
            yield return @"var a = 1L - 1L";
            yield return @"var a = 1L * 1L";
            yield return @"var a = 1L / 1L";
            yield return @"var a = 1L % 1L";
            yield return @"var a = 1L > 1L";
            yield return @"var a = 1L < 1L";
            yield return @"var a = 1L >= 1L";
            yield return @"var a = 1L <= 1L";
            yield return @"var b = 1L; var a = -b";
            yield return @"var b = 1L; var a = +b";
            yield return @"var b = 1L; var a = --b";
            yield return @"var b = 1L; var a = ++b";
            yield return @"var b = 1L; var a = b++";
            yield return @"var b = 1L; var a = b--";
            yield return @"var b = 1L; var a = ~b";
            yield return @"var a = 1L == 1L";
            yield return @"var a = 1L != 1L";
            yield return @"var a = 1L.Equals(1L)";
            yield return @"var a = 1L.Equals(new object())";
            yield return @"var a = 1L >> 1";
            yield return @"var a = 1L << 1";
            yield return @"var a = 1L & 1L";
            yield return @"var a = 1L ^ 1L";
            yield return @"var a = 1L | 1L";
            yield return @"var a = 1L; a += 1L";
            yield return @"var a = 1L; a *= 1L";
            yield return @"var a = 1L; a /= 1L";
            yield return @"var a = 1L; a %= 1L";
            yield return @"var a = 1L; a -= 1L";
            yield return @"var a = 1L; a &= 1L";
            yield return @"var a = 1L; a |= 1L";
            yield return @"var a = 1L; a <<= 1";
            yield return @"var a = 1L; a >>= 1";
            yield return @"var a = 1L; a ^= 1L";


            yield return @"var a = 1L.CompareTo(1L)";
            yield return @"var a = 1L.CompareTo(new object())";
            yield return @"var a = 1L.GetHashCode()";
            yield return @"var a = 1L.GetTypeCode()";
        }

        public static IEnumerable<string> GetPureCasesForUInt64()
        {
            yield return @"var a = 1ul + 1ul";
            yield return @"var a = 1ul - 1ul";
            yield return @"var a = 1ul * 1ul";
            yield return @"var a = 1ul / 1ul";
            yield return @"var a = 1ul % 1ul";
            yield return @"var a = 1ul > 1ul";
            yield return @"var a = 1ul < 1ul";
            yield return @"var a = 1ul >= 1ul";
            yield return @"var a = 1ul <= 1ul";
            yield return @"var a = 1ul == 1ul";
            yield return @"var a = 1ul != 1ul";
            yield return @"var a = 1ul.Equals(1ul)";
            yield return @"var a = 1ul.Equals(new object())";
            yield return @"var b = 1ul; var a = +b";
            yield return @"var b = 1ul; var a = --b";
            yield return @"var b = 1ul; var a = ++b";
            yield return @"var b = 1ul; var a = b++";
            yield return @"var b = 1ul; var a = b--";
            yield return @"var b = 1ul; var a = ~b";
            yield return @"var a = 1ul >> 1";
            yield return @"var a = 1ul << 1";
            yield return @"var a = 1ul & 1ul";
            yield return @"var a = 1ul ^ 1ul";
            yield return @"var a = 1ul | 1ul";
            yield return @"var a = 1ul; a += 1ul";
            yield return @"var a = 1ul; a *= 1ul";
            yield return @"var a = 1ul; a /= 1ul";
            yield return @"var a = 1ul; a %= 1ul";
            yield return @"var a = 1ul; a -= 1ul";
            yield return @"var a = 1ul; a &= 1ul";
            yield return @"var a = 1ul; a |= 1ul";
            yield return @"var a = 1ul; a <<= 1";
            yield return @"var a = 1ul; a >>= 1";
            yield return @"var a = 1ul; a ^= 1ul";

            yield return @"var a = 1ul.CompareTo(1ul)";
            yield return @"var a = 1ul.CompareTo(new object())";
            yield return @"var a = 1ul.GetHashCode()";
            yield return @"var a = 1ul.GetTypeCode()";
        }

        public static IEnumerable<string> GetPureCasesForInt16()
        {
            yield return @"short s = 1; var a = s + s";
            yield return @"short s = 1; var a = s - s";
            yield return @"short s = 1; var a = s * s";
            yield return @"short s = 1; var a = s / s";
            yield return @"short s = 1; var a = s % s";
            yield return @"short s = 1; var a = s > s";
            yield return @"short s = 1; var a = s < s";
            yield return @"short s = 1; var a = s >= s";
            yield return @"short s = 1; var a = s <= s";
            yield return @"short s = 1; var a = s == s";
            yield return @"short s = 1; var a = s != s";
            yield return @"short s = 1; var a = s.Equals(s)";
            yield return @"short s = 1; var a = s.Equals(new object())";
            yield return @"short s = 1; var a = -s";
            yield return @"short s = 1; var a = +s";
            yield return @"short s = 1; var a = --s";
            yield return @"short s = 1; var a = ++s";
            yield return @"short s = 1; var a = s++";
            yield return @"short s = 1; var a = s--";
            yield return @"short s = 1; var a = ~s";
            yield return @"short s = 1; var a = s >> 1";
            yield return @"short s = 1; var a = s << 1";
            yield return @"short s = 1; var a = s & s";
            yield return @"short s = 1; var a = s ^ s";
            yield return @"short s = 1; var a = s | s";
            yield return @"short s = 1; var a = s; a += s";
            yield return @"short s = 1; var a = s; a *= s";
            yield return @"short s = 1; var a = s; a /= s";
            yield return @"short s = 1; var a = s; a %= s";
            yield return @"short s = 1; var a = s; a -= s";
            yield return @"short s = 1; var a = s; a &= s";
            yield return @"short s = 1; var a = s; a |= s";
            yield return @"short a = 1; a <<= 1";
            yield return @"short a = 1; a >>= 1";
            yield return @"short s = 1; var a = s; a ^= s";

            yield return @"short s = 1; var a = s.CompareTo(s)";
            yield return @"short s = 1; var a = s.CompareTo(new object())";
            yield return @"short s = 1; var a = s.GetHashCode()";
            yield return @"short s = 1; var a = s.GetTypeCode()";
        }

        public static IEnumerable<string> GetPureCasesForUInt16()
        {
            yield return @"ushort s = 1; var a = s + s";
            yield return @"ushort s = 1; var a = s - s";
            yield return @"ushort s = 1; var a = s * s";
            yield return @"ushort s = 1; var a = s / s";
            yield return @"ushort s = 1; var a = s % s";
            yield return @"ushort s = 1; var a = s > s";
            yield return @"ushort s = 1; var a = s < s";
            yield return @"ushort s = 1; var a = s >= s";
            yield return @"ushort s = 1; var a = s <= s";
            yield return @"ushort s = 1; var a = s == s";
            yield return @"ushort s = 1; var a = s != s";
            yield return @"ushort s = 1; var a = s.Equals(s)";
            yield return @"ushort s = 1; var a = s.Equals(new object())";
            yield return @"ushort s = 1; var a = -s";
            yield return @"ushort s = 1; var a = +s";
            yield return @"ushort s = 1; var a = --s";
            yield return @"ushort s = 1; var a = ++s";
            yield return @"ushort s = 1; var a = s++";
            yield return @"ushort s = 1; var a = s--";
            yield return @"ushort s = 1; var a = ~s";
            yield return @"ushort s = 1; var a = s >> 1";
            yield return @"ushort s = 1; var a = s << 1";
            yield return @"ushort s = 1; var a = s & s";
            yield return @"ushort s = 1; var a = s ^ s";
            yield return @"ushort s = 1; var a = s | s";
            yield return @"ushort s = 1; var a = s; a += s";
            yield return @"ushort s = 1; var a = s; a *= s";
            yield return @"ushort s = 1; var a = s; a /= s";
            yield return @"ushort s = 1; var a = s; a %= s";
            yield return @"ushort s = 1; var a = s; a -= s";
            yield return @"ushort s = 1; var a = s; a &= s";
            yield return @"ushort s = 1; var a = s; a |= s";
            yield return @"ushort a = 1; a <<= 1";
            yield return @"ushort a = 1; a >>= 1";
            yield return @"ushort s = 1; var a = s; a ^= s";

            yield return @"ushort s = 1; var a = s.CompareTo(s)";
            yield return @"ushort s = 1; var a = s.CompareTo(new object())";
            yield return @"ushort s = 1; var a = s.GetHashCode()";
            yield return @"ushort s = 1; var a = s.GetTypeCode()";
        }

        public static IEnumerable<string> GetPureCasesForBoolean()
        {
            yield return @"var a = !true";
            yield return @"var a = true == true";
            yield return @"var a = true != true";
            yield return @"var a = true & true";
            yield return @"var a = true ^ true";
            yield return @"var a = true | true";
            yield return @"var a = true && true";
            yield return @"var a = true || true";
            yield return @"var a = true; a &= true";
            yield return @"var a = true; a |= true";
            yield return @"var a = true; a ^= true";

            yield return @"var a = true.Equals(true)";
            yield return @"var a = true.Equals(new object())";
            yield return "true.ToString()";
            yield return "true.ToString(null)";
            yield return @"var a = true.CompareTo(true)";
            yield return @"var a = true.CompareTo(new object())";
            yield return @"var a = true.GetHashCode()";
            yield return @"var a = true.GetTypeCode()";

            yield return @"var a = bool.Parse(""True"")";
            yield return @"var a = bool.TryParse(""True"", out var result)";

        }

        public static IEnumerable<string> GetPureCasesForByte()
        {
            yield return @"byte s = 1; var a = s + s";
            yield return @"byte s = 1; var a = s - s";
            yield return @"byte s = 1; var a = s * s";
            yield return @"byte s = 1; var a = s / s";
            yield return @"byte s = 1; var a = s % s";
            yield return @"byte s = 1; var a = s > s";
            yield return @"byte s = 1; var a = s < s";
            yield return @"byte s = 1; var a = s >= s";
            yield return @"byte s = 1; var a = s <= s";
            yield return @"byte s = 1; var a = s == s";
            yield return @"byte s = 1; var a = s != s";
            yield return @"byte s = 1; var a = s.Equals(s)";
            yield return @"byte s = 1; var a = s.Equals(new object())";
            yield return @"byte s = 1; var a = -s";
            yield return @"byte s = 1; var a = +s";
            yield return @"byte s = 1; var a = --s";
            yield return @"byte s = 1; var a = ++s";
            yield return @"byte s = 1; var a = s++";
            yield return @"byte s = 1; var a = s--";
            yield return @"byte s = 1; var a = ~s";
            yield return @"byte s = 1; var a = s >> 1";
            yield return @"byte s = 1; var a = s << 1";
            yield return @"byte s = 1; var a = s & s";
            yield return @"byte s = 1; var a = s ^ s";
            yield return @"byte s = 1; var a = s | s";
            yield return @"byte s = 1; var a = s; a += s";
            yield return @"byte s = 1; var a = s; a *= s";
            yield return @"byte s = 1; var a = s; a /= s";
            yield return @"byte s = 1; var a = s; a %= s";
            yield return @"byte s = 1; var a = s; a -= s";
            yield return @"byte s = 1; var a = s; a &= s";
            yield return @"byte s = 1; var a = s; a |= s";
            yield return @"byte a = 1; a <<= 1";
            yield return @"byte a = 1; a >>= 1";
            yield return @"byte s = 1; var a = s; a ^= s";

            yield return @"byte s = 1; var a = s.CompareTo(s)";
            yield return @"byte s = 1; var a = s.CompareTo(new object())";
            yield return @"byte s = 1; var a = s.GetHashCode()";
            yield return @"byte s = 1; var a = s.GetTypeCode()";
        }

        public static IEnumerable<string> GetPureCasesForSByte()
        {
            yield return @"sbyte s = 1; var a = s + s";
            yield return @"sbyte s = 1; var a = s - s";
            yield return @"sbyte s = 1; var a = s * s";
            yield return @"sbyte s = 1; var a = s / s";
            yield return @"sbyte s = 1; var a = s % s";
            yield return @"sbyte s = 1; var a = s > s";
            yield return @"sbyte s = 1; var a = s < s";
            yield return @"sbyte s = 1; var a = s >= s";
            yield return @"sbyte s = 1; var a = s <= s";
            yield return @"sbyte s = 1; var a = s == s";
            yield return @"sbyte s = 1; var a = s != s";
            yield return @"sbyte s = 1; var a = s.Equals(s)";
            yield return @"sbyte s = 1; var a = s.Equals(new object())";
            yield return @"sbyte s = 1; var a = -s";
            yield return @"sbyte s = 1; var a = +s";
            yield return @"sbyte s = 1; var a = --s";
            yield return @"sbyte s = 1; var a = ++s";
            yield return @"sbyte s = 1; var a = s++";
            yield return @"sbyte s = 1; var a = s--";
            yield return @"sbyte s = 1; var a = ~s";
            yield return @"sbyte s = 1; var a = s >> 1";
            yield return @"sbyte s = 1; var a = s << 1";
            yield return @"sbyte s = 1; var a = s & s";
            yield return @"sbyte s = 1; var a = s ^ s";
            yield return @"sbyte s = 1; var a = s | s";
            yield return @"sbyte s = 1; var a = s; a += s";
            yield return @"sbyte s = 1; var a = s; a *= s";
            yield return @"sbyte s = 1; var a = s; a /= s";
            yield return @"sbyte s = 1; var a = s; a %= s";
            yield return @"sbyte s = 1; var a = s; a -= s";
            yield return @"sbyte s = 1; var a = s; a &= s";
            yield return @"sbyte s = 1; var a = s; a |= s";
            yield return @"sbyte a = 1; a <<= 1";
            yield return @"sbyte a = 1; a >>= 1";
            yield return @"sbyte s = 1; var a = s; a ^= s";

            yield return @"sbyte s = 1; var a = s.CompareTo(s)";
            yield return @"sbyte s = 1; var a = s.CompareTo(new object())";
            yield return @"sbyte s = 1; var a = s.GetHashCode()";
            yield return @"sbyte s = 1; var a = s.GetTypeCode()";
        }

        public static IEnumerable<string> GetPureCasesForIntPtr()
        {
            yield return @"var a = IntPtr.Zero == IntPtr.Zero";
            yield return @"var a = IntPtr.Zero != IntPtr.Zero";
            yield return @"var a = IntPtr.Zero.Equals(IntPtr.Zero)";
            yield return @"var a = IntPtr.Zero.Equals(new object())";
            yield return @"var a = new IntPtr(1)";
            yield return @"var a = new IntPtr(1L)";
            yield return @"var a = IntPtr.Zero.GetHashCode()";
            yield return @"var a = IntPtr.Zero.ToInt32()";
            yield return @"var a = IntPtr.Zero.ToInt64()";
            yield return @"var a = IntPtr.Zero.ToString()"; //This is InvariantCulture
            yield return @"var a = IntPtr.Zero.ToString("""")"; //This is InvariantCulture
            yield return @"var a = (IntPtr)1";
            yield return @"var a = (IntPtr)1L";
            yield return @"var a = (int)IntPtr.Zero";
            yield return @"var a = (long)IntPtr.Zero";
            yield return @"var a = IntPtr.Add(IntPtr.Zero, 1)";
            yield return @"var a = IntPtr.Zero + 1";
            yield return @"var a = IntPtr.Subtract(IntPtr.Zero, 1)";
            yield return @"var a = IntPtr.Zero - 1";
        }

        public static IEnumerable<string> GetPureCasesForUIntPtr()
        {
            yield return @"var a = UIntPtr.Zero == UIntPtr.Zero";
            yield return @"var a = UIntPtr.Zero != UIntPtr.Zero";
            yield return @"var a = UIntPtr.Zero.Equals(UIntPtr.Zero)";
            yield return @"var a = UIntPtr.Zero.Equals(new object())";
            yield return @"var a = new UIntPtr(1u)";
            yield return @"var a = new UIntPtr(1UL)";
            yield return @"var a = UIntPtr.Zero.GetHashCode()";
            yield return @"var a = UIntPtr.Zero.ToUInt32()";
            yield return @"var a = UIntPtr.Zero.ToUInt64()";
            yield return @"var a = UIntPtr.Zero.ToString()"; //This is InvariantCulture
            yield return @"var a = (UIntPtr)1u";
            yield return @"var a = (UIntPtr)1UL";
            yield return @"var a = (uint)UIntPtr.Zero";
            yield return @"var a = (ulong)UIntPtr.Zero";
            yield return @"var a = UIntPtr.Add(UIntPtr.Zero, 1)";
            yield return @"var a = UIntPtr.Zero + 1";
            yield return @"var a = UIntPtr.Subtract(UIntPtr.Zero, 1)";
            yield return @"var a = UIntPtr.Zero - 1";
        }

        public static IEnumerable<string> GetPureCasesForChar()
        {
            yield return @"var a = 'a' + 'a'";
            yield return @"var a = 'a' - 'a'";
            yield return @"var a = 'a' * 'a'";
            yield return @"var a = 'a' / 'a'";
            yield return @"var a = 'a' % 'a'";
            yield return @"var a = 'a' > 'a'";
            yield return @"var a = 'a' < 'a'";
            yield return @"var a = 'a' >= 'a'";
            yield return @"var a = 'a' <= 'a'";
            yield return @"var b = 'a'; var a = -b";
            yield return @"var b = 'a'; var a = +b";
            yield return @"var b = 'a'; var a = --b";
            yield return @"var b = 'a'; var a = ++b";
            yield return @"var b = 'a'; var a = b++";
            yield return @"var b = 'a'; var a = b--";
            yield return @"var b = 'a'; var a = ~b";
            yield return @"var a = 'a' == 'a'";
            yield return @"var a = 'a' != 'a'";
            yield return @"var a = 'a'.Equals('a')";
            yield return @"var a = 'a'.Equals(new object())";
            yield return @"var a = 'a' >> 1";
            yield return @"var a = 'a' << 1";
            yield return @"var a = 'a' & 'a'";
            yield return @"var a = 'a' ^ 'a'";
            yield return @"var a = 'a' | 'a'";
            yield return @"var a = 'a'; a += 'a'";
            yield return @"var a = 'a'; a *= 'a'";
            yield return @"var a = 'a'; a /= 'a'";
            yield return @"var a = 'a'; a %= 'a'";
            yield return @"var a = 'a'; a -= 'a'";
            yield return @"var a = 'a'; a &= 'a'";
            yield return @"var a = 'a'; a |= 'a'";
            yield return @"var a = 'a'; a <<= 1";
            yield return @"var a = 'a'; a >>= 1";
            yield return @"var a = 'a'; a ^= 'a'";
            yield return @"var a = 'a'.GetHashCode()";
            yield return @"var a = 'a'.CompareTo('a')";
            yield return @"var a = 'a'.CompareTo(new object())";
            yield return @"var a = 'a'.ToString()";
            yield return @"var a = 'a'.ToString(null)";
            yield return @"var a = char.ToString('a')";
            yield return @"var a = char.Parse(""a"")";
            yield return @"var a = char.TryParse(""a"", out var result)";
            yield return @"var a = 'a'.GetTypeCode()";
            yield return @"var a = char.IsSurrogate('a')";
            yield return @"var a = char.IsSurrogate(""a"", 0)";
            yield return @"var a = char.IsHighSurrogate('a')";
            yield return @"var a = char.IsHighSurrogate(""a"", 0)";
            yield return @"var a = char.IsLowSurrogate('a')";
            yield return @"var a = char.IsLowSurrogate(""a"", 0)";
            yield return @"var a = char.IsSurrogatePair(""a"", 0)";
            yield return @"var a = char.IsSurrogatePair('a', 'a')";
            yield return @"var a = char.ConvertFromUtf32(1)";
            yield return @"var a = char.ConvertToUtf32('a', 'a')";
        }

        public static IEnumerable<string> GetPureCasesForString()
        {
            yield return @"String.Equals(""1"", ""2"")";
            yield return @"""1"".Equals(""2"")";
            yield return @"""1"".Equals(new object())";

            yield return @"var a = string.Empty";
            yield return @"var a = string.Empty + string.Empty";
            yield return @"var a = string.Empty == string.Empty";
            yield return @"var a = string.Empty != string.Empty";
            yield return @"var a = string.Empty; a += string.Empty";
            yield return @"String.Join("","", new []{""1"", ""2""})";
            yield return @"var a = string.Join(string.Empty, string.Empty, string.Empty)";
            yield return @"var a = string.Join(string.Empty, new object(), new object())";
            yield return @"var a = string.Join<char>(string.Empty, new List<char> {'a','b','c'})";
            yield return @"var a = string.Join(string.Empty, new List<string> {string.Empty, string.Empty})";
            yield return @"var a = string.Join(string.Empty, new [] {string.Empty, string.Empty}, 0 ,2)";
            yield return @"var a = ""1""[0]";
            yield return @"var a = string.Empty.ToCharArray()";
            yield return @"var a = ""1"".ToCharArray(0,1)";
            yield return @"var a = string.IsNullOrEmpty(string.Empty)";
            yield return @"var a = string.IsNullOrWhiteSpace(string.Empty)";
            yield return @"var a = string.Empty.Length";
            yield return @"var a = ""1"".Substring(0)";
            yield return @"var a = ""1"".Substring(0,1)";
            yield return @"var a = string.CompareOrdinal(""1"", ""1"")";
            yield return @"var a = string.CompareOrdinal(""1"", 0, ""1"", 0, 1)";
            yield return @"var a = string.Empty.Contains(string.Empty)";
            yield return @"var a = ""1"".IndexOf('1')";
            yield return @"var a = ""1"".IndexOf('1', 0)";
            yield return @"var a = ""1"".IndexOf('1', 0, 1)";
            yield return @"var a = ""1"".IndexOfAny(new []{'1'})";
            yield return @"var a = ""1"".IndexOfAny(new []{'1'}, 0)";
            yield return @"var a = ""1"".IndexOfAny(new []{'1'}, 0, 1)";
            yield return @"var a = ""1"".LastIndexOf('1')";
            yield return @"var a = ""1"".LastIndexOf('1', 0)";
            yield return @"var a = ""1"".LastIndexOf('1', 0, 1)";
            yield return @"var a = ""1"".LastIndexOfAny(new []{'1'})";
            yield return @"var a = ""1"".LastIndexOfAny(new []{'1'}, 0)";
            yield return @"var a = ""1"".LastIndexOfAny(new []{'1'}, 0, 1)";
            yield return @"var a = ""1"".PadLeft(2)";
            yield return @"var a = ""1"".PadLeft(2, 'a')";
            yield return @"var a = ""1"".PadRight(2)";
            yield return @"var a = ""1"".PadRight(2, 'a')";
            yield return @"var a = ""a"".ToUpperInvariant()";
            yield return @"var a = ""A"".ToLowerInvariant()";
            yield return @"var a = ""A"".ToString()";
            yield return @"var a = ""A"".ToString(null)";
            yield return @"var a = ""A"".Clone()";
            yield return @"var a = ""A"".Replace('a', 'b')";
            yield return @"var a = ""A"".Replace(""a"", ""b"")";
            yield return @"var a = ""A"".Remove(0, 1)";
            yield return @"var a = ""A"".Remove(0)";
            yield return @"var a = string.Copy(""A"")";
            yield return @"var a = string.Concat(new object())";
            yield return @"var a = string.Concat(new object(), new object())";
            yield return @"var a = string.Concat(new object(), new object(), new object())";
            yield return @"var a = string.Concat(new object []{new object(), new object(), new object()})";
            yield return @"var a = string.Concat<char>(new List<char> {'a','b','c'})";
            yield return @"var a = string.Concat(new List<string> {""a"",""b"",""c""})";
            yield return @"var a = string.Concat(string.Empty, string.Empty)";
            yield return @"var a = string.Concat(string.Empty, string.Empty, string.Empty)";
            yield return @"var a = string.Concat(string.Empty, string.Empty, string.Empty, string.Empty)";
            yield return @"var a = string.Concat(new string[]{string.Empty, string.Empty})";
            yield return @"var a = ""1"".GetTypeCode()";
            yield return @"var a = (IEnumerable<char>) ""1""";
            yield return @"var a = ""1"".GetEnumerator()";
        }

        public static IEnumerable<string> GetPureCasesForSingle()
        {
            yield return @"var a = 1f + 1f";
            yield return @"var a = 1f - 1f";
            yield return @"var a = 1f * 1f";
            yield return @"var a = 1f / 1f";
            yield return @"var a = 1f % 1f";
            yield return @"var a = 1f > 1f";
            yield return @"var a = 1f < 1f";
            yield return @"var a = 1f >= 1f";
            yield return @"var a = 1f <= 1f";
            yield return @"var b = 1f; var a = -b";
            yield return @"var b = 1f; var a = +b";
            yield return @"var b = 1f; var a = --b";
            yield return @"var b = 1f; var a = ++b";
            yield return @"var b = 1f; var a = b++";
            yield return @"var b = 1f; var a = b--";

            yield return @"var a = 1f == 1f";
            yield return @"var a = 1f != 1f";
            yield return @"var a = 1f.Equals(1f)";
            yield return @"var a = 1f.Equals(new object())";


            yield return @"var a = 1f; a += 1f";
            yield return @"var a = 1f; a *= 1f";
            yield return @"var a = 1f; a /= 1f";
            yield return @"var a = 1f; a %= 1f";
            yield return @"var a = 1f; a -= 1f";

            yield return @"var a = 1f.CompareTo(1f)";
            yield return @"var a = 1f.CompareTo(new object())";
            yield return @"var a = 1f.GetHashCode()";
            yield return @"var a = 1f.GetTypeCode()";

            yield return @"var a = Single.IsInfinity(1f)";
            yield return @"var a = Single.IsPositiveInfinity(1f)";
            yield return @"var a = Single.IsNegativeInfinity(1f)";
            yield return @"var a = Single.IsNaN(1f)";


        }

        public static IEnumerable<string> GetPureCasesForDouble()
        {
            yield return @"var a = 1d + 1d";
            yield return @"var a = 1d - 1d";
            yield return @"var a = 1d * 1d";
            yield return @"var a = 1d / 1d";
            yield return @"var a = 1d % 1d";
            yield return @"var a = 1d > 1d";
            yield return @"var a = 1d < 1d";
            yield return @"var a = 1d >= 1d";
            yield return @"var a = 1d <= 1d";
            yield return @"var b = 1d; var a = -b";
            yield return @"var b = 1d; var a = +b";
            yield return @"var b = 1d; var a = --b";
            yield return @"var b = 1d; var a = ++b";
            yield return @"var b = 1d; var a = b++";
            yield return @"var b = 1d; var a = b--";

            yield return @"var a = 1d == 1d";
            yield return @"var a = 1d != 1d";
            yield return @"var a = 1d.Equals(1d)";
            yield return @"var a = 1d.Equals(new object())";


            yield return @"var a = 1d; a += 1d";
            yield return @"var a = 1d; a *= 1d";
            yield return @"var a = 1d; a /= 1d";
            yield return @"var a = 1d; a %= 1d";
            yield return @"var a = 1d; a -= 1d";

            yield return @"var a = 1d.CompareTo(1d)";
            yield return @"var a = 1d.CompareTo(new object())";
            yield return @"var a = 1d.GetHashCode()";
            yield return @"var a = 1d.GetTypeCode()";

            yield return @"var a = Double.IsInfinity(1d)";
            yield return @"var a = Double.IsPositiveInfinity(1d)";
            yield return @"var a = Double.IsNegativeInfinity(1d)";
            yield return @"var a = Double.IsNaN(1d)";
        }

        public static IEnumerable<string> GetPureCasesForGuid()
        {
            yield return @"var guid = new Guid(new byte[16])";
            yield return @"var guid = new Guid(1u,1,1,1,1,1,1,1,1,1,1)";
            yield return @"var guid = new Guid(1, 1, 1, new byte[8])";
            yield return @"var guid = new Guid(1,1,1,1,1,1,1,1,1,1,1)";
            yield return @"var guid = new Guid(""41C19760-DF17-4499-A992-F8D8423B2294"")";
            yield return @"var guid = Guid.Parse(""41C19760-DF17-4499-A992-F8D8423B2294"")";
            yield return @"var a = Guid.Empty.ToString()";
            yield return @"var a = Guid.TryParse(""41C19760-DF17-4499-A992-F8D8423B2294"", out var guid)";
            yield return @"var guid = Guid.ParseExact(""41C19760-DF17-4499-A992-F8D8423B2294"", ""d"")";
            yield return @"var a = Guid.TryParseExact(""41C19760-DF17-4499-A992-F8D8423B2294"", ""d"", out var guid)";
            yield return @"var a = Guid.Empty.ToByteArray()";
            yield return @"var a = Guid.Empty.GetHashCode()";
            yield return @"var a = Guid.Empty.Equals(Guid.Empty)";
            yield return @"var a = Guid.Empty.Equals((object)Guid.Empty)";
            yield return @"var a = Guid.Empty.CompareTo(Guid.Empty)";
            yield return @"var a = Guid.Empty.CompareTo((object)Guid.Empty)";
            yield return @"var a = Guid.Empty == Guid.Empty";
            yield return @"var a = Guid.Empty != Guid.Empty";
            yield return @"var a = Guid.Empty.ToString(""d"", (IFormatProvider)null)";
            yield return @"var a = Guid.Empty.ToString(""d"")";
        }

        public static IEnumerable<string> GetPureCasesForList()
        {
            yield return @"var a = new List<char>()";
            yield return @"var a = new List<char>(1)";
            yield return @"var a = new List<char>((IEnumerable<char>)new []{'a'})";
        }


        public static IEnumerable<string> GetPureCasesForDateTime()
        {
            yield return @"var a = new DateTime(2018,1,1)";
            yield return @"var a = new DateTime(2018,1,1,1,1,1)";
            yield return @"var a = new DateTime(2018,1,1,1,1,1).Year";
            yield return @"var a = new DateTime(2018,1,1,1,1,1).Month";
            yield return @"var a = new DateTime(2018,1,1,1,1,1).Day";
            yield return @"var a = new DateTime(2018,1,1,1,1,1).Hour";
            yield return @"var a = new DateTime(2018,1,1,1,1,1).Minute";
            yield return @"var a = new DateTime(2018,1,1,1,1,1).Second";
            yield return @"var a = new DateTime(2018,1,1,1,1,1).Millisecond";
        }

        public static IEnumerable<string> GetPureCasesForImmutableArray()
        {
            yield return @"var a = System.Collections.Immutable.ImmutableArray<char>.Empty";
            yield return @"var a = System.Collections.Immutable.ImmutableArray<char>.Empty.Add('c')";
            yield return @"var a = System.Collections.Immutable.ImmutableArray<char>.Empty.AddRange(new []{'c', 'd'})";
            yield return @"var a = System.Collections.Immutable.ImmutableArray<char>.Empty.Add('c').Select(x => x.ToString())";
            yield return @"var a = System.Collections.Immutable.ImmutableArray<char>.Empty.Add('c').Where(x => x == 'c')";

            //TODO: continue working on ImmutableArray
        }


        public static IEnumerable<string> GetImpureCases()
        {
            yield return @"1.ToString()";
            yield return @"String.Equals(""1"", ""2"", StringComparison.CurrentCulture)";
            yield return @"""1"".Equals(""2"", StringComparison.CurrentCulture)";
            yield return @"String.Equals(""1"", ""2"", StringComparison.CurrentCultureIgnoreCase)";
            yield return @"""1"".Equals(""2"", StringComparison.CurrentCultureIgnoreCase)";


            yield return @"var a = ""1"".IndexOf(""1"")";
            yield return @"var a = ""1"".IndexOf(""1"", 0)";
            yield return @"var a = ""1"".IndexOf(""1"", 0, 1)";

            yield return @"var a = string.Join<int>(string.Empty, new List<int> {1,2,3})";
            yield return @"var a = string.Concat<int>(new List<int> {1,2,3})";

            yield return @"var guid = Guid.NewGuid()";

            yield return "new System.Text.StringBuilder().AppendLine()"; //Environment.NewLine is not pure!?
        }
    }
}
