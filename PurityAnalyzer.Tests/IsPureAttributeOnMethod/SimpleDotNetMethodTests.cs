using System;
using System.Collections.Generic;
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
            dignostics.Length.Should().Be(0);
        }

        [TestCaseSource(nameof(GetImpureCases))]
        public void TestImpureInvocation(string invocation)
        {
            string code = $@"
using System;

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


        public static IEnumerable<string> GetPureCases()
        {
            foreach (var c in GetPureCasesForInt32()) yield return c;

            foreach (var c in GetPureCasesForUInt32()) yield return c;

            foreach (var c in GetPureCasesForInt64()) yield return c;

            foreach (var c in GetPureCasesForUInt64()) yield return c;

            yield return "true.ToString()";
            yield return @"Guid.Parse(""41C19760-DF17-4499-A992-F8D8423B2294"")";
            yield return @"Guid.Parse(""41C19760-DF17-4499-A992-F8D8423B2294"").ToString()";
            yield return @"String.Join("","", new []{""1"", ""2""})";
            yield return @"String.Equals(""1"", ""2"")";
            yield return @"""1"".Equals(""2"")";
            yield return @"String.Equals(""1"", ""2"", StringComparison.Ordinal)";
            yield return @"""1"".Equals(""2"", StringComparison.Ordinal)";
            yield return @"String.Equals(""1"", ""2"", StringComparison.OrdinalIgnoreCase)";
            yield return @"""1"".Equals(""2"", StringComparison.OrdinalIgnoreCase)";
            yield return @"var a = ((int?)1).HasValue";
            yield return @"var a = ((int?)1).Value";
            yield return @"var a = string.Empty";






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

            yield return @"var a = !true";
            yield return @"var a = true == true";
            yield return @"var a = true != true";
            yield return @"var a = true.Equals(true)";
            yield return @"var a = true & true";
            yield return @"var a = true ^ true";
            yield return @"var a = true | true";
            yield return @"var a = true && true";
            yield return @"var a = true || true";
            yield return @"var a = true; a &= true";
            yield return @"var a = true; a |= true";
            yield return @"var a = true; a ^= true";


            yield return @"var a = IntPtr.Zero == IntPtr.Zero";
            yield return @"var a = IntPtr.Zero != IntPtr.Zero";
            yield return @"var a = IntPtr.Zero.Equals(IntPtr.Zero)";

            yield return @"var a = UIntPtr.Zero == UIntPtr.Zero";
            yield return @"var a = UIntPtr.Zero != UIntPtr.Zero";
            yield return @"var a = UIntPtr.Zero.Equals(UIntPtr.Zero)";
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

        public static IEnumerable<string> GetImpureCases()
        {
            yield return @"1.ToString()";
        }
    }
}
