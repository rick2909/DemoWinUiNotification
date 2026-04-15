using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using System;
using System.Runtime.InteropServices;

namespace DemoWinUiNotification
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static bool _isRedirected;
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            EnsureSingleInstance();
            if (_isRedirected)
            {
                return;
            }

            try
            {
                AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
                AppNotificationManager.Default.Register();
            }
            catch (COMException)
            {
                // App draait zonder geregistreerde notification COM server (bv. unpackaged debug-profiel).
                // De app blijft bruikbaar; native toasts worden dan overgeslagen.
            }
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            if (_isRedirected)
            {
                return;
            }

            _window ??= new MainWindow();
            _window.Activate();
        }

        private void EnsureSingleInstance()
        {
            var current = AppInstance.GetCurrent();
            var mainInstance = AppInstance.FindOrRegisterForKey("main");

            if (!mainInstance.IsCurrent)
            {
                _isRedirected = true;
                var activatedArgs = current.GetActivatedEventArgs();
                mainInstance.RedirectActivationToAsync(activatedArgs).AsTask().GetAwaiter().GetResult();
                Environment.Exit(0);
                return;
            }

            mainInstance.Activated += OnMainInstanceActivated;
        }

        private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            ActivateMainWindow();
        }

        private void OnMainInstanceActivated(object? sender, AppActivationArguments args)
        {
            ActivateMainWindow();
        }

        private void ActivateMainWindow()
        {
            _window?.DispatcherQueue.TryEnqueue(() => _window.Activate());
        }
    }
}
