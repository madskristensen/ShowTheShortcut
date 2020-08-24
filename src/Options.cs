using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace ShowTheShortcut
{
    public class Options : DialogPage
    {
        // General
        private const string GeneralCategory = "General";

        [Category(GeneralCategory)]
        [DisplayName("Show on shortcut")]
        [Description("Determines if the status bar should show shortcuts for commands that were invoked by a shortcut.")]
        [DefaultValue(false)]
        public bool ShowOnShortcut { get; set; }

        // Status Bar
        private const string StatusBarCategory = "Statur Bar";

        [Category(StatusBarCategory)]
        [DisplayName("Log to Status Bar")]
        [Description("Log all keyboard shortcuts for commands captured to the status bar.")]
        [DefaultValue(true)]
        public bool LogToStatusBar { get; set; } = true;

        [Category(StatusBarCategory)]
        [DisplayName("Timeout")]
        [Description("Specify for how many seconds the to show the shortcut in the status bar. Set to 0 (zero) to never hide.")]
        [DefaultValue(5)]
        public int Timeout { get; set; } = 5;

        [Category(StatusBarCategory)]
        [DisplayName("Show tooltip")]
        [Description("Determines if a tooltiop should be enabled. This can be useful if you want to know more information about the commands.")]
        [DefaultValue(false)]
        public bool ShowTooltip { get; set; }

        // Output Window
        private const string OutputWindowCategory = "Output Window";

        [Category(OutputWindowCategory)]
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
