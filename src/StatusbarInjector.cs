using Microsoft.VisualStudio.Shell;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShowTheShortcut
{
	internal class StatusBarInjector
	{
		private readonly Window _window;
		private FrameworkElement _statusBar;
		private Panel _panel;

		public StatusBarInjector(Window pWindow)
		{
			_window = pWindow;
			_window.Initialized += new EventHandler(WindowInitialized);

			FindStatusBar();
		}

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

		private void FindStatusBar()
		{
			_statusBar = FindChild(_window, "StatusBarContainer") as FrameworkElement;
			_panel = _statusBar?.Parent as DockPanel;
		}

		public async System.Threading.Tasks.Task InjectControlAsync(FrameworkElement element)
		{
			if (_panel == null)
			{
				return;
			}

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			element.SetValue(DockPanel.DockProperty, Dock.Left);
			_panel.Children.Insert(1, element);
		}

		private void WindowInitialized(object sender, EventArgs e)
		{
		}
	}
}