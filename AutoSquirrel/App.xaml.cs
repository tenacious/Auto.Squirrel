using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Squirrel;

namespace AutoSquirrel
{
    /// <summary>
    /// Application Entry
    /// </summary>
    public partial class App : Application
    {
        private const string traceFilename = "Log.txt";

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnStartup(StartupEventArgs e)
        {
            Trace.AutoFlush = true;

            try
            {
                var log = new TextWriterTraceListener(File.Create(traceFilename))
                {
                    Filter = new EventTypeFilter(SourceLevels.Information),
                    TraceOutputOptions = TraceOptions.None
                };

                Trace.Listeners.Add(log);
            }
            catch
            {
            }

            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) => ShowUnhandeledException(e);

        private void ShowUnhandeledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var errorMessage =
                string.Format(
                    "An application error occurred.\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.\n\nError:{0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)",

                    e.Exception.Message + (e.Exception.InnerException != null
                        ? "\n" +
                          e.Exception.InnerException.Message
                        : null));

            Trace.TraceError(errorMessage);
            Trace.TraceError("Stack Trace " + e.Exception.StackTrace);
            Trace.TraceError("Source " + e.Exception.Source);
            Trace.TraceError("Inner Exception3 " + e.Exception.InnerException);
            Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// App Bootstrapper
    /// </summary>
    /// <seealso cref="Caliburn.Micro.BootstrapperBase"/>
    public class AppBootstrapper : BootstrapperBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppBootstrapper"/> class.
        /// </summary>
        public AppBootstrapper()
        {
            using (var mgr = new UpdateManager(@"https://s3-eu-west-1.amazonaws.com/autosquirrel", "AutoSquirrel"))
            {
                // We have to re-implement the things Squirrel does for normal applications, because
                // we're marked as Squirrel-aware
                SquirrelAwareApp.HandleEvents(
                    onInitialInstall: v => mgr.CreateShortcutForThisExe(),
                    onAppUpdate: v =>
                    {
                        mgr.CreateShortcutForThisExe();

                        // Update the shortcuts
                        mgr.CreateShortcutsForExecutable("AutoSquirrel.exe", ShortcutLocation.AppRoot, false);
                    },
                    onAppUninstall: v => mgr.RemoveShortcutForThisExe());
            }
            Initialize();
        }

        /// <summary>
        /// Override this to add custom behavior to execute after the application starts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        protected override void OnStartup(object sender, StartupEventArgs e) => DisplayRootViewFor<ShellViewModel>();
    }
}