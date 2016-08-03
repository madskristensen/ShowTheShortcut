using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;

namespace ShowTheShortcut
{
    class CommandHandler
    {
        private Options _options;
        private CommandEvents _events;
        private DTE2 _dte;
        private static Dictionary<string, string> _cache = new Dictionary<string, string>();
        private Key[] _keys = { Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift };
        private bool _showShortcut;
        private Timer _timer;
        private StatusbarControl _control;
        private static string[] _ignoreCmd =
        {
            "Edit.GoToFindCombo",
            "Debug.LocationToolbar.ProcessCombo",
            "Debug.LocationToolbar.ThreadCombo",
            "Debug.LocationToolbar.StackFrameCombo"
        };

        private CommandHandler(DTE2 dte, Options options)
        {
            _options = options;
            _dte = dte;
            _control = new StatusbarControl(options, _dte);
            _events = _dte.Events.CommandEvents;

            _events.AfterExecute += AfterExecute;
            _events.BeforeExecute += BeforeExecute;

            var injector = new StatusBarInjector(Application.Current.MainWindow);
            injector.InjectControl(_control);

            _timer = new Timer();
            _timer.Elapsed += (s, e) =>
            {
                _timer.Stop();
                _control.SetVisibility(Visibility.Collapsed);
            };

            _options.Saved += (s, e) =>
            {
                SetTimeout();

                if (!_options.LogToStatusBar)
                    _control.SetVisibility(Visibility.Collapsed);

                if (!_options.LogToOutputWindow)
                    Logger.DeletePane();
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

        public static void Initialize(IServiceProvider provider, Options options)
        {
            var dte = provider.GetService(typeof(DTE)) as DTE2;
            Instance = new CommandHandler(dte, options);
        }

        private void BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            _showShortcut = _options.ShowOnShortcut ? true : !_keys.Any(key => Keyboard.IsKeyDown(key));
        }

        private void AfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            if (!_showShortcut)
                return;

            try
            {
                var cmd = _dte.Commands.Item(Guid, ID);

                if (string.IsNullOrWhiteSpace(cmd?.Name) || ShouldCommandBeIgnored(cmd))
                    return;

                string shortcut = GetShortcut(cmd);

                if (!string.IsNullOrWhiteSpace(shortcut))
                {
                    if (_options.LogToStatusBar)
                    {
                        string prettyName = Prettify(cmd);
                        string text = $"{prettyName} ({shortcut})";

                        _control.SetVisibility(Visibility.Visible);
                        _control.Text = text;
                    }

                    if (_options.ShowTooltip)
                        _control.SetTooltip(cmd);

                    if (_options.LogToOutputWindow)
                        Logger.Log($"{cmd.Name} ({shortcut})");

                    if (_options.Timeout > 0)
                    {
                        _timer.Stop();
                        _timer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private static string GetShortcut(Command cmd)
        {
            if (cmd == null || string.IsNullOrEmpty(cmd.Name))
                return null;

            string key = cmd.Guid + cmd.ID;

            if (_cache.ContainsKey(key))
                return _cache[key];

            var bindings = ((object[])cmd.Bindings).FirstOrDefault() as string;

            if (!string.IsNullOrEmpty(bindings))
            {
                int index = bindings.IndexOf(':') + 2;
                string shortcut = bindings.Substring(index);

                if (!IsShortcutInteresting(shortcut))
                    shortcut = null;

                if (!_cache.ContainsKey(key))
                    _cache.Add(key, shortcut);

                return shortcut;
            }

            return null;
        }

        private static string Prettify(Command cmd)
        {
            if (cmd.LocalizedName.Length < 40)
                return cmd.LocalizedName;

            int index = cmd.LocalizedName.LastIndexOf('.') + 1;
            return cmd.LocalizedName.Substring(index);
        }

        private static bool IsShortcutInteresting(string shortcut)
        {
            if (string.IsNullOrWhiteSpace(shortcut))
                return false;

            if (!shortcut.Contains("Ctrl") && !shortcut.Contains("Alt") && !shortcut.Contains("Shift"))
                return false;

            return true;
        }

        private static bool ShouldCommandBeIgnored(Command cmd)
        {
            if (_ignoreCmd.Contains(cmd.Name, StringComparer.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
