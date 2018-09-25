using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace PurityAnalyzer.Vsix
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [ProvideOptionPage(typeof(OptionPageGrid),
        "Purity Analyzer", "Settings Page", 0, 0, true)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(VSPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string)]
    public sealed class VSPackage : AsyncPackage
    {
        /// <summary>
        /// VSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "890b2098-b623-48f6-a8d8-953d270968c3";

        /// <summary>
        /// Initializes a new instance of the <see cref="VSPackage"/> class.
        /// </summary>
        public VSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            PurityAnalyzerAnalyzer.CustomPureMethodsFilename = CustomPureMethodsFilename.ToMaybe().If(x => x != "");
            PurityAnalyzerAnalyzer.CustomPureExceptLocallyMethodsFilename = CustomPureExceptLocallyMethodsFilename.ToMaybe().If(x => x != "");
            PurityAnalyzerAnalyzer.CustomPureExceptReadLocallyMethodsFilename = CustomPureExceptReadLocallyMethodsFilename.ToMaybe().If(x => x != "");
            PurityAnalyzerAnalyzer.CustomPureTypesFilename = CustomPureTypesFilename.ToMaybe().If(x => x != "");
            PurityAnalyzerAnalyzer.CustomReturnsNewObjectMethodsFilename = CustomReturnsNewObjectMethodsFilename.ToMaybe().If(x => x != "");


            var componentModel = (IComponentModel) await this.GetServiceAsync(typeof(SComponentModel));
            var workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

            PurityAnalyzerAnalyzer.GetSemanticModelForSyntaxTreeAsync = async tree =>
            {
                var document  = workspace.CurrentSolution.GetDocument(tree);

                return await document.GetSemanticModelAsync();
            };
        }

        #endregion

        public string CustomPureTypesFilename
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.CustomPureTypesFilename;
            }
        }

        public string CustomPureMethodsFilename
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.CustomPureMethodsFilename;
            }
        }

        public string CustomPureExceptLocallyMethodsFilename
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.CustomPureExceptLocallyMethodsFilename;
            }
        }

        public string CustomPureExceptReadLocallyMethodsFilename
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.CustomPureExceptReadLocallyMethodsFilename;
            }
        }

        public string CustomReturnsNewObjectMethodsFilename
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.CustomReturnsNewObjectMethodsFilename;
            }
        }
    }

    public class OptionPageGrid : DialogPage
    {
        [Category("Purity Analyzer")]
        [DisplayName("Custom Pure Types Filename")]
        [Description("Full filename that contains custom types to consider pure")]
        public string CustomPureTypesFilename { get; set; } = "";

        [Category("Purity Analyzer")]
        [DisplayName("Custom Pure Methods Filename")]
        [Description("Full filename that contains custom methods to consider pure")]
        public string CustomPureMethodsFilename { get; set; } = "";

        [Category("Purity Analyzer")]
        [DisplayName("Custom Pure Except Locally Methods Filename")]
        [Description("Full filename that contains custom methods to consider pure-except-locally")]
        public string CustomPureExceptLocallyMethodsFilename { get; set; } = "";

        [Category("Purity Analyzer")]
        [DisplayName("Custom Pure Except Read Locally Methods Filename")]
        [Description("Full filename that contains custom methods to consider pure-except-read-locally")]
        public string CustomPureExceptReadLocallyMethodsFilename { get; set; } = "";


        [Category("Purity Analyzer")]
        [DisplayName("Custom Returns New Object Methods Filename")]
        [Description("Full filename that contains custom methods that return new objects")]
        public string CustomReturnsNewObjectMethodsFilename { get; set; } = "";
    }

}
