using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Basic.Reference.Assemblies;
using FluentAssertions;
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

        public static MetadataReference GetSystemXmlReference()
        {
            return Net472.References.SystemXml;
        }

        public static MetadataReference[] GetImmutableCollectionsReferences()
        {
            var immutableCollectionsPath = Path.Combine(
                AppContext.BaseDirectory, "TestReferences", "System.Collections.Immutable.dll");

            return new MetadataReference[]
            {
                MetadataReference.CreateFromFile(immutableCollectionsPath),
                Net472.References.SystemRuntime
            };
        }

        public static Diagnostic[] RunPurityAnalyzer(string content, params MetadataReference[] additionalReferences)
        {
            return RunPurityAnalyzer(content, Maybe.NoValue, additionalReferences);
        }

        public static Diagnostic[] RunPurityAnalyzerWithImmutableCollections(
            string content, params MetadataReference[] additionalReferences)
        {
            return RunPurityAnalyzer(
                content,
                Maybe.NoValue,
                additionalReferences.Concat(GetImmutableCollectionsReferences()).ToArray());
        }


        public static Diagnostic[] RunPurityAnalyzer(string content, Maybe<string> secondFileContent,
            params MetadataReference[] additionalReferences)
        {
            return RunPurityAnalyzer(content, secondFileContent, false, additionalReferences);
        }

        public static Diagnostic[] RunPurityAnalyzer(string content, Maybe<string> secondFileContent, bool secondFileIsInDifferentProject, params MetadataReference[] additionalReferences)
        {
            var workspace = new AdhocWorkspace();

            var solution = workspace.CurrentSolution;

            var projectId = ProjectId.CreateNewId();

            solution = AddNewProjectToWorkspace(solution, "NewProject", projectId, additionalReferences);

            var documentId = DocumentId.CreateNewId(projectId);

            solution = AddNewSourceFile(solution, content, "NewFile.cs", documentId);

            if (secondFileContent.HasValue)
            {
                DocumentId secondDocumentId;

                if (secondFileIsInDifferentProject)
                {
                    var secondProjectId = ProjectId.CreateNewId();

                    solution = AddNewProjectToWorkspace(solution, "NewProject2", secondProjectId, additionalReferences);

                    secondDocumentId = DocumentId.CreateNewId(secondProjectId);

                    solution = solution.AddProjectReference(projectId, new ProjectReference(secondProjectId));

                }
                else
                {
                    secondDocumentId = DocumentId.CreateNewId(projectId);
                }
                

                solution = AddNewSourceFile(solution, secondFileContent.GetValue(), "NewFile2.cs", secondDocumentId);
            }

            PurityAnalyzerAnalyzer.GetSemanticModelForSyntaxTreeAsync = async tree =>
            {
                var document = solution.GetDocument(tree);

                return await document.GetSemanticModelAsync();
            };

            var result = solution.GetProject(projectId).GetCompilationAsync().Result
                .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new PurityAnalyzerAnalyzer()));

            var results = result.GetAllDiagnosticsAsync().Result;

            var compilationErrors = results.Where(x => !IsFromPurityAnalyzer(x))
                .Where(x => x.Severity == DiagnosticSeverity.Error).ToList();

            if (compilationErrors.Any())
            {
                throw new Exception("Error in compilation" + Environment.NewLine + String.Join(Environment.NewLine, compilationErrors.Select(x => x.GetMessage())));
            }

            var ad0001Results = results.Where(x => x.Descriptor.Id == "AD0001").ToArray();

            if (ad0001Results.Any())
            {
                throw new Exception(ad0001Results.First().GetMessage());
            }

            bool IsFromPurityAnalyzer(Diagnostic x)
            {
                return x.Descriptor.Id == PurityAnalyzerAnalyzer.PurityDiagnosticId || x.Descriptor.Id == PurityAnalyzerAnalyzer.ReturnsNewObjectDiagnosticId || x.Descriptor.Id == PurityAnalyzerAnalyzer.GenericParameterNotUsedAsObjectDiagnosticId;
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
            // Code under test is compiled against .NET Framework 4.7.2 reference assemblies, not against the
            // runtime that hosts the tests. netstandard.dll is needed for the netstandard2.0 CompiledCsharpLib.
            MetadataReference corlibReference = Net472.References.mscorlib;
            MetadataReference systemCoreReference = Net472.References.SystemCore;
            MetadataReference netstandardReference = Net472.References.netstandard;

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
                            netstandardReference
                        }.Concat(additionalReferences).ToArray()));
        }

        public static void AssertPure(string code)
        {
            var diagnostics = Utilities.RunPurityAnalyzer(code);
            diagnostics.Length.Should().Be(0);
        }

        public static void AssertPure(string code, params MetadataReference[] additionalReferences)
        {
            var diagnostics = Utilities.RunPurityAnalyzer(code, additionalReferences);
            diagnostics.Length.Should().Be(0);
        }

        public static void AssertImpure(string code)
        {
            var diagnostics = Utilities.RunPurityAnalyzer(code);
            diagnostics.Length.Should().BePositive();
        }

        public static void AssertImpure(string code, params MetadataReference[] additionalReferences)
        {
            var diagnostics = Utilities.RunPurityAnalyzer(code, additionalReferences);
            diagnostics.Length.Should().BePositive();
        }
    }
}
