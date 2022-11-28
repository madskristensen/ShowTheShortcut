global using System;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;

using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.Threading;

namespace ShowTheShortcut
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration("#110", "#112", Vsix.Version)]
	[ProvideOptionPage(typeof(Options), "Environment\\Keyboard", "Shortcuts", 0, 0, true, ProvidesLocalizedCategoryName = false)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
	[Guid(Vsix.Id)]
	public sealed class VSPackage : AsyncPackage
	{
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync();
			await Logger.InitializeAsync(this, Vsix.Name);

			Options options = GetDialogPage(typeof(Options)) as Options;

			CommandHandler.InitializeAsync(this, options).FileAndForget(Vsix.Name);
		}
	}
}
