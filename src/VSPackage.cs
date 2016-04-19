using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ShowTheShortcut
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid(Vsix.Id)]
    [ProvideOptionPage(typeof(Options), "Environment\\Keyboard", "Shortcuts", 0, 0, true, ProvidesLocalizedCategoryName = false)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    public sealed class VSPackage : Package
    {
        protected override void Initialize()
        {
            base.Initialize();
            var options = (Options)GetDialogPage(typeof(Options));

            Logger.Initialize(this, Vsix.Name);

            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    CommandHandler.Initialize(this, options);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

            }), DispatcherPriority.ApplicationIdle, null);
        }
    }
}
