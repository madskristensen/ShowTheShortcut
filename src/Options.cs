using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace ShowTheShortcut
{
    public class Options : DialogPage
    {
        [Category("General")]
        [DisplayName("Show on shortcut")]
        [Description("Determines if the status bar should show shortcuts for commands that were invoked by a shortcut.")]
        [DefaultValue(false)]
        public bool ShowOnShortcut { get; set; }

        [Category("Status Bar")]
        [DisplayName("Status bar timeout")]
        [Description("Specify for how many seconds the to show the shortcut in the status bar. Set to 0 (zero) to never hide.")]
        [DefaultValue(5)]
        public int Timeout { get; set; } = 5;

        [Category("Status Bar")]
        [DisplayName("Log to status bar")]
        [Description("Log all keyboard shortcuts for commands captured to the status bar.")]
        [DefaultValue(true)]
        public bool LogToStatusBar { get; set; } = true;

        [Category("Status Bar")]
        [DisplayName("Show tooltip")]
        [Description("Determines if a tooltiop should be enabled. This can be useful if you want to know more information about the commands.")]
        [DefaultValue(false)]
        public bool ShowTooltip { get; set; }

        [Category("Output Window")]
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
