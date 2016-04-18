using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EnvDTE;
using EnvDTE80;

namespace ShowTheShortcut
{
    class CommandHandler
    {
        private Options _options;
        private CommandEvents _events;
        private DTE2 _dte;
        private Key[] _keys = { Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift };
        private bool _showShortcut;
        private StatusBarInjector _injector;
        private Timer _timer;
        private TextBlock _control = new TextBlock
        {
            Foreground = Brushes.White,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(5, 4, 10, 0),
            Visibility = Visibility.Collapsed
        };

        private CommandHandler(IServiceProvider provider, Options options)
        {
            _options = options;
            _dte = (DTE2)provider.GetService(typeof(DTE));
            _events = _dte.Events.CommandEvents;

            _events.AfterExecute += AfterExecute;
            _events.BeforeExecute += BeforeExecute;

            _injector = new StatusBarInjector(Application.Current.MainWindow);
            _injector.InjectControl(_control);

            _timer = new Timer();

            SetTimeout();

            _timer.Elapsed += (s, e) =>
            {
                _timer.Stop();
                _control.Dispatcher.Invoke(() =>
                {
                    _control.Visibility = Visibility.Collapsed;
                });
            };

            _options.Saved += (s, e) =>
            {
                SetTimeout();
            };
        }

        private void SetTimeout()
        {
            if (_options.Timeout > 0)
                _timer.Interval = _options.Timeout * 1000;
            else
                _timer.Stop();
        }

        public static CommandHandler Instance { get; private set; }

        public static void Initialize(IServiceProvider provider, Options options)
        {
            Instance = new CommandHandler(provider, options);
        }

        private void BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            _showShortcut = _options.ShowOnShortcut ? true : !_keys.Any(key => Keyboard.IsKeyDown(key));
        }

        private void AfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            if (!_showShortcut)
                return;

            var cmd = _dte.Commands.Item(Guid, ID);
            string shortcut = GetShortcut(cmd);

            if (!string.IsNullOrEmpty(shortcut))
            {
                _control.Visibility = Visibility.Visible;
                _control.Text = $"{cmd.Name} ({shortcut})";
                _timer.Stop();
                _timer.Start();
            }
        }

        private static string GetShortcut(Command cmd)
        {
            if (cmd == null || string.IsNullOrEmpty(cmd.Name))
                return null;

            var bindings = ((object[])cmd.Bindings).FirstOrDefault() as string;

            if (!string.IsNullOrEmpty(bindings))
            {
                int index = bindings.IndexOf(':') + 2;
                return bindings.Substring(index);
            }

            return null;
        }
    }
}
