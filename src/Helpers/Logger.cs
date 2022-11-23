using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;

using task = System.Threading.Tasks.Task;

internal static class Logger
{
	private static string _name;
	private static Guid _guid = new Guid();
	private static IVsOutputWindowPane _pane;
	private static IVsOutputWindow _output;

	public static async task InitializeAsync(AsyncPackage package, string name)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
		_output = await package.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;
		_name = name;
	}

	public static async task LogAsync(object message)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		try
		{
			if (EnsurePane())
			{
				_pane.OutputStringThreadSafe(DateTime.Now.ToShortTimeString() + ": " + message + Environment.NewLine);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.Write(ex);
		}
	}

	public static async task DeletePaneAsync()
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		if (_output != null)
		{
			_output.DeletePane(_guid);
		}
	}

	private static bool EnsurePane()
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (_pane == null && _output != null)
		{
			_output.CreatePane(ref _guid, _name, 1, 1);
			_output.GetPane(ref _guid, out _pane);
		}

		return _pane != null;
	}
}