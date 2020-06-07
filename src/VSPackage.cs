using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Runtime.InteropServices;
using System.Threading;

using task = System.Threading.Tasks.Task;

namespace ShowTheShortcut
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration("#110", "#112", Vsix.Version)]
	[ProvideOptionPage(typeof(Options), "Environment\\Keyboard", "Shortcuts", 0, 0, true, ProvidesLocalizedCategoryName = false)]
	[ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
	[Guid(Vsix.Id)]
	public sealed class VSPackage : AsyncPackage
	{
		protected override async task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync();
			await Logger.InitializeAsync(this, Vsix.Name);

			Options options = GetDialogPage(typeof(Options)) as Options;

			await CommandHandler.InitializeAsync(this, options);
		}
	}
}
