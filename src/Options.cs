using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace ShowTheShortcut
{
    public class Options : DialogPage
    {
        [Category("General")]
        [DisplayName("Timeout")]
        [Description("Specify for how many seconds the to show the shortcut in the status bar. Set to 0 (zero) to never hide.")]
        [DefaultValue(5)]
        public int Timeout { get; set; }

        [Category("General")]
        [DisplayName("Show on shortcut")]
        [Description("Determines if the status bar should show shortcuts for commands that were invoked by the shortcut.")]
        [DefaultValue(false)]
        public bool ShowOnShortcut { get; set; }

        [Category("General")]
        [DisplayName("Log to Output Window")]
        [Description("Log all keyboard shortcuts for commands captured to the Output Window.")]
        [DefaultValue(true)]
        public bool LogToOutputWindow { get; set; } = true;

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            Saved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fires when the options have been saved.
        /// </summary>
        public event EventHandler Saved;
    }
}
