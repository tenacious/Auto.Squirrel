using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AutoSquirrel
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string traceFilename = "Log.txt";

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
            catch (Exception ex)
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

    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
