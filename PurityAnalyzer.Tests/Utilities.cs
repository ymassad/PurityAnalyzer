using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using PurityAnalyzer.Tests.CompiledCsharpLib;

namespace PurityAnalyzer.Tests
{
    public static class Utilities
    {
        public static string NormalizeCode(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var newRoot = syntaxTree.GetRoot().NormalizeWhitespace();

            return newRoot.ToString();
        }

        public static string MergeParts(params string[] parts)
        {
            return String.Join(Environment.NewLine, parts);
        }

        public static string InNamespace(string content, string @namespace)
        {
            return $@"namespace {@namespace}
{{
{content}
}}";
        }


        public static PortableExecutableReference GetTestsCompiledCsharpLibProjectReference()
        {
            return MetadataReference.CreateFromFile(typeof(ImmutableDto1).Assembly.Location);
        }

        public static PortableExecutableReference CreateFromType<T>()
        {
            return MetadataReference.CreateFromFile(typeof(T).Assembly.Location);
        }

        public static Diagnostic[] RunPurityAnalyzer(string content, params MetadataReference[] additionalReferences)
        {
            return RunPurityAnalyzer(content, Maybe.NoValue, additionalReferences);
        }

        public static Diagnostic[] RunPurityAnalyzer(string content, Maybe<string> secondFileContent, params MetadataReference[] additionalReferences)
        {
            var workspace = new AdhocWorkspace();

            var solution = workspace.CurrentSolution;

            var projectId = ProjectId.CreateNewId();

            solution = AddNewProjectToWorkspace(solution, "NewProject", projectId, additionalReferences);

            var documentId = DocumentId.CreateNewId(projectId);

            solution = AddNewSourceFile(solution, content, "NewFile.cs", documentId);

            if (secondFileContent.HasValue)
            {
                var secondDocumentId = DocumentId.CreateNewId(projectId);

                solution = AddNewSourceFile(solution, secondFileContent.GetValue(), "NewFile2.cs", secondDocumentId);
            }

            var result = solution.GetProject(projectId).GetCompilationAsync().Result
                .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new PurityAnalyzerAnalyzer()));

            var results = result.GetAllDiagnosticsAsync().Result;

            var compilationErrors = results.Where(x => !IsFromPurityAnalyzer(x))
                .Where(x => x.Severity == DiagnosticSeverity.Error).ToList();

            if (compilationErrors.Any())
            {
                throw new Exception("Error in compilation" + Environment.NewLine + string.Join(Environment.NewLine, compilationErrors.Select(x => x.GetMessage())));
            }

            var ad0001Results = results.Where(x => x.Descriptor.Id == "AD0001").ToArray();

            if (ad0001Results.Any())
            {
                throw new Exception(ad0001Results.First().GetMessage());
            }

            bool IsFromPurityAnalyzer(Diagnostic x)
            {
                return x.Descriptor.Id == PurityAnalyzerAnalyzer.PurityDiagnosticId || x.Descriptor.Id == PurityAnalyzerAnalyzer.ReturnsNewObjectDiagnosticId;
            }

            var diagnostics = results.Where(IsFromPurityAnalyzer).ToArray();

            if (diagnostics.Any())
            {
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine(diag);
                }
            }

            return diagnostics;
        }

        private static Solution AddNewSourceFile(
            Solution solution,
            string fileContent,
            string fileName,
            DocumentId documentId)
        {
            return solution.AddDocument(documentId, fileName, SourceText.From(fileContent));
        }

        private static Solution AddNewProjectToWorkspace(
            Solution solution, string projName, ProjectId projectId, params MetadataReference[] additionalReferences)
        {
            MetadataReference csharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
            MetadataReference codeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

            MetadataReference corlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            MetadataReference systemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);

            return
                solution.AddProject(
                    ProjectInfo.Create(
                            projectId,
                            VersionStamp.Create(),
                            projName,
                            projName,
                            LanguageNames.CSharp,
                            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                        .WithMetadataReferences(new[]
                        {
                            corlibReference,
                            systemCoreReference,
                            csharpSymbolsReference,
                            codeAnalysisReference
                        }.Concat(additionalReferences).ToArray()));
        }

    }
}
