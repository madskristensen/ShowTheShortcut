using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EnvDTE;
using EnvDTE80;

namespace ShowTheShortcut
{
    class StatusbarControl : TextBlock
    {
        private Options _options;
        private DTE2 _dte;

        public StatusbarControl(Options options, DTE2 dte)
        {
            _options = options;
            _dte = dte;

            Foreground = Brushes.White;
            Margin = new Thickness(5, 4, 10, 0);
            Visibility = Visibility.Collapsed;

            ToolTipOpening += (s, e) =>
            {
                if (_options.ShowTooltip)
                    Cursor = Cursors.Hand;
            };

            ToolTipClosing += (s, e) =>
            {
                if (_options.ShowTooltip)
                    Cursor = Cursors.Arrow;
            };

            MouseLeftButtonUp += (s, e) =>
            {
                string tooltip = ToolTip as string;

                if (!string.IsNullOrWhiteSpace(tooltip))
                {
                    Clipboard.SetText(tooltip);
                    _dte.StatusBar.Text = "Shortcut info copied to clipboard";
                }
            };
        }

        public void SetTooltip(Command cmd)
        {
            ToolTip = $"Name: {cmd.Name}" + Environment.NewLine +
                      $"Localized: {cmd.LocalizedName}" + Environment.NewLine +
                      $"GUID: {cmd.Guid}" + Environment.NewLine +
                      $"ID: {cmd.ID}";
        }

        public void SetVisibility(Visibility visibility)
        {
            Dispatcher.Invoke(() =>
            {
                Visibility = visibility;
            });
        }
    }
}