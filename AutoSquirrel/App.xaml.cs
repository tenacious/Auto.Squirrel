using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;

namespace AutoSquirrel
{
    /// <summary>
    /// Logica di interazione per App.xaml
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
            catch (Exception)
            {
            }

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowUnhandeledException(e);
        }

        private void ShowUnhandeledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            string errorMessage =
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

            //if (
            //    MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error, MessageBoxResult.No) ==
            //    MessageBoxResult.Yes)
            {
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <seealso cref="Caliburn.Micro.BootstrapperBase"/>
    public class AppBootstrapper : BootstrapperBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppBootstrapper"/> class.
        /// </summary>
        public AppBootstrapper()
        {
            Initialize();
        }

        /// <summary>
        /// Override this to add custom behavior to execute after the application starts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}