using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ShowTheShortcut
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideOptionPage(typeof(Options), "Environment\\Keyboard", "Shortcuts", 0, 0, true, ProvidesLocalizedCategoryName = false)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(Vsix.Id)]
    public sealed class VSPackage : Package
    {
        protected override void Initialize()
        {
            ThreadHelper.Generic.BeginInvoke(DispatcherPriority.ApplicationIdle, () =>
            {
                var options = GetDialogPage(typeof(Options)) as Options;

                Logger.Initialize(this, Vsix.Name);
                CommandHandler.Initialize(this, options);
            });
        }
    }
}
