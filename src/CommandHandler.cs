using EnvDTE;

using EnvDTE80;

using Microsoft.VisualStudio.Shell;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;

using task = System.Threading.Tasks.Task;

namespace ShowTheShortcut
{
	internal class CommandHandler
	{
		private readonly Options _options;
		private readonly CommandEvents _events;
		private readonly DTE2 _dte;
		private static readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
		private readonly Key[] _keys = { Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift };
		private bool _showShortcut;
		private readonly Timer _timer;
		private readonly StatusbarControl _control;
		private static readonly string[] _ignoreCmd =
		{
			"Edit.GoToFindCombo",
			"Debug.LocationToolbar.ProcessCombo",
			"Debug.LocationToolbar.ThreadCombo",
			"Debug.LocationToolbar.StackFrameCombo",
			"Build.SolutionPlatforms",
			"Build.SolutionConfigurations"
		};

		private CommandHandler(DTE2 dte, Options options)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			_options = options;
			_dte = dte;
			_control = new StatusbarControl(options, _dte);
			_events = _dte.Events.CommandEvents;

			_events.AfterExecute += AfterExecute;
			_events.BeforeExecute += BeforeExecute;

			StatusBarInjector.InjectControlAsync(_control).ConfigureAwait(false);

			_timer = new Timer();
			_timer.Elapsed += (s, e) =>
			{
				_timer.Stop();
				_control.SetVisibilityAsync(Visibility.Collapsed).ConfigureAwait(false);
			};

			_options.Saved += (s, e) =>
			{
				SetTimeout();

				if (!_options.LogToStatusBar)
				{
					_control.SetVisibilityAsync(Visibility.Collapsed).ConfigureAwait(false);
				}

				if (!_options.LogToOutputWindow)
				{
					Logger.DeletePaneAsync().ConfigureAwait(false);
				}
			};

			SetTimeout();
		}

		private void SetTimeout()
		{
			if (_options.Timeout > 0)
			{
				_timer.Interval = _options.Timeout * 1000;
				_timer.Start();
			}
			else
			{
				_timer.Stop();
			}
		}

		public static CommandHandler Instance { get; private set; }

		public static async task InitializeAsync(AsyncPackage package, Options options)
		{
			DTE2 dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
			Instance = new CommandHandler(dte, options);
		}

		private void BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
		{
			_showShortcut = _options.ShowOnShortcut || !_keys.Any(key => Keyboard.IsKeyDown(key));
		}

		private void AfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (!_showShortcut || !(CustomIn is null) || !(CustomOut is null))
			{
				return;
			}

			try
			{
				Command cmd = null;
				try
				{
					cmd = _dte.Commands.Item(Guid, ID);
				}
				catch (ArgumentException)
				{
					return;
				}

				if (string.IsNullOrWhiteSpace(cmd?.Name) || ShouldCommandBeIgnored(cmd))
				{
					return;
				}

				var shortcut = GetShortcut(cmd);

				if (string.IsNullOrWhiteSpace(shortcut))
				{
					return;
				}

				if (_options.LogToStatusBar)
				{
					string prettyName = Prettify(cmd);
					string text = $"{prettyName} ({shortcut})";

					_control.SetVisibilityAsync(Visibility.Visible).ConfigureAwait(false);
					_control.Text = text;
				}

				if (_options.ShowTooltip)
				{
					_control.SetTooltip(cmd);
				}

				if (_options.LogToOutputWindow)
				{
					Logger.LogAsync($"{cmd.Name} ({shortcut})").ConfigureAwait(false);
				}

				if (_options.Timeout > 0)
				{
					_timer.Stop();
					_timer.Start();
				}
			}
			catch (Exception ex)
			{
				Logger.LogAsync(ex).ConfigureAwait(false);
			}
		}

		private static string GetShortcut(Command cmd)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (cmd == null || string.IsNullOrEmpty(cmd.Name))
			{
				return null;
			}

			string key = cmd.Guid + cmd.ID;

			if (_cache.ContainsKey(key))
			{
				return _cache[key];
			}

			string bindings = ((object[])cmd.Bindings).FirstOrDefault() as string;

			if (!string.IsNullOrEmpty(bindings))
			{
				int index = bindings.IndexOf(':') + 2;
				string shortcut = bindings.Substring(index);

				if (!IsShortcutInteresting(shortcut))
				{
					shortcut = null;
				}

				if (!_cache.ContainsKey(key))
				{
					_cache.Add(key, shortcut);
				}

				return shortcut;
			}

			return null;
		}

		private static string Prettify(Command cmd)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (cmd.LocalizedName.Length < 40)
			{
				return cmd.LocalizedName;
			}

			int index = cmd.LocalizedName.LastIndexOf('.') + 1;
			return cmd.LocalizedName.Substring(index);
		}

		private static string Prettify(Guid guid)
		{
			return $"Guid={guid:B}, ID=0x{2343:x4}";
		}

		private static bool IsShortcutInteresting(string shortcut)
		{
			if (string.IsNullOrWhiteSpace(shortcut))
			{
				return false;
			}

			if (!shortcut.Contains("Ctrl") && !shortcut.Contains("Alt") && !shortcut.Contains("Shift"))
			{
				return false;
			}

			return true;
		}

		private static bool ShouldCommandBeIgnored(Command cmd)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			if (_ignoreCmd.Contains(cmd.Name, StringComparer.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}
	}
}
