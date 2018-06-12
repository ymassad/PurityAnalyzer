using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PurityAnalyzer
{
    [CreateMatchMethods(typeof(IdentifierUsage))]
    public static class ExtensionMethods
    {
       

        public static TResult Match<TResult>(this IdentifierUsage instance, Func<IdentifierUsage.ReadFromAndWrittenToCase, TResult> readFromAndWrittenToCaseCase, Func<IdentifierUsage.WrittenToCase, TResult> writtenToCaseCase, Func<IdentifierUsage.ReadFromCase, TResult> readFromCaseCase)
        {
            if (instance is IdentifierUsage.ReadFromAndWrittenToCase readFromAndWrittenToCase)
                return readFromAndWrittenToCaseCase(readFromAndWrittenToCase);
            if (instance is IdentifierUsage.WrittenToCase writtenToCase)
                return writtenToCaseCase(writtenToCase);
            if (instance is IdentifierUsage.ReadFromCase readFromCase)
                return readFromCaseCase(readFromCase);
            throw new Exception("Invalid IdentifierUsage type");
        }

        public static void Match(this IdentifierUsage instance, Action<IdentifierUsage.ReadFromAndWrittenToCase> readFromAndWrittenToCaseCase, Action<IdentifierUsage.WrittenToCase> writtenToCaseCase, Action<IdentifierUsage.ReadFromCase> readFromCaseCase)
        {
            if (instance is IdentifierUsage.ReadFromAndWrittenToCase readFromAndWrittenToCase)
            {
                readFromAndWrittenToCaseCase(readFromAndWrittenToCase);
                return;
            }

            if (instance is IdentifierUsage.WrittenToCase writtenToCase)
            {
                writtenToCaseCase(writtenToCase);
                return;
            }

            if (instance is IdentifierUsage.ReadFromCase readFromCase)
            {
                readFromCaseCase(readFromCase);
                return;
            }

            throw new Exception("Invalid IdentifierUsage type");
        }

        public static bool IsCompiled(this ISymbol symbol) => !symbol.IsInCode();

        public static bool IsInCode(this ISymbol symbol)
        {
            return symbol.Locations.All(x => x.IsInSource);
        }

        public static bool IsStatic(this BaseMethodDeclarationSyntax method)
        {
            return method.Modifiers.Any(SyntaxKind.StaticKeyword);
        }

        public static bool IsStatic(this PropertyDeclarationSyntax prop)
        {
            return prop.Modifiers.Any(SyntaxKind.StaticKeyword);
        }

        public static bool IsStatic(this FieldDeclarationSyntax field)
        {
            return field.Modifiers.Any(SyntaxKind.StaticKeyword);
        }

        public static T ThrowIf<T>(this T value, Func<T, bool> condition, string errorMessage)
        {
            if(condition(value))
                throw new Exception(errorMessage);

            return value;
        }
    }
}