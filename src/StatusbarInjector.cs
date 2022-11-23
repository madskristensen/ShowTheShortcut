using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShowTheShortcut
{
	internal static class StatusBarInjector
	{
		private static Panel _panel;

		private static DependencyObject FindChild(DependencyObject parent, string childName)
		{
			if (parent == null)
			{
				return null;
			}

			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

			for (int i = 0; i < childrenCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);

				if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
				{
					return frameworkElement;
				}

				child = FindChild(child, childName);

				if (child != null)
				{
					return child;
				}
			}

			return null;
		}

		private static async Task EnsureUIAsync()
		{
			while (_panel is null)
			{
				_panel = FindChild(Application.Current.MainWindow, "StatusBarPanel") as DockPanel;
				if (_panel is null)
				{
					// Start window is showing. Need to wait for status bar render.
					await Task.Delay(5000);
				}
			}
		}

		public static async Task InjectControlAsync(FrameworkElement element)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			await EnsureUIAsync();

			element.SetValue(DockPanel.DockProperty, Dock.Left);
			_panel.Children.Insert(1, element);
		}
	}
}