using EnvDTE;

using EnvDTE80;

using Microsoft.VisualStudio.Shell;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ShowTheShortcut
{
	internal class StatusbarControl : TextBlock
	{
		private readonly Options _options;
		private readonly DTE2 _dte;

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
				{
					Cursor = Cursors.Hand;
				}
			};

			ToolTipClosing += (s, e) =>
			{
				if (_options.ShowTooltip)
				{
					Cursor = Cursors.Arrow;
				}
			};

			MouseLeftButtonUp += (s, e) =>
			{
				Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
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
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

			ToolTip = $"Name: {cmd.Name}" + Environment.NewLine +
					  $"Localized: {cmd.LocalizedName}" + Environment.NewLine +
					  $"GUID: {cmd.Guid}" + Environment.NewLine +
					  $"ID: {cmd.ID}";
		}

		public async System.Threading.Tasks.Task SetVisibilityAsync(Visibility visibility)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			try
			{
				Visibility = visibility;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Write(ex);
			}
		}
	}
}