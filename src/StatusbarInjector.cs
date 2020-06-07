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
			_panel = _statusBar.Parent as DockPanel;
		}

		public async System.Threading.Tasks.Task InjectControlAsync(FrameworkElement pControl)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			pControl.SetValue(DockPanel.DockProperty, Dock.Left);
			_panel.Children.Insert(1, pControl);
		}

		//public bool IsInjected(FrameworkElement pControl)
		//{
		//	bool flag2 = false;

		//	_panel.Dispatcher.Invoke(() =>
		//	{
		//		bool flag = _panel.Children.Contains(pControl);
		//		bool flag1 = flag;
		//		flag2 = flag;
		//		return flag1;
		//	});

		//	return flag2;
		//}

		//public void UninjectControl(FrameworkElement pControl)
		//{
		//	_panel.Dispatcher.Invoke(() => _panel.Children.Remove(pControl));
		//}

		private void WindowInitialized(object sender, EventArgs e)
		{
		}
	}
}