using Squirrel;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.WebRequestMethods;

namespace AutoSquirrel
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class ShellView
    {

        public ShellView()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            PackageTreeview.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;

            Closing += ShellView_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                using (var mgr = new UpdateManager(@"https://s3-eu-west-1.amazonaws.com/squirrelpackager"))
                {
                    try
                    {
                        await mgr.UpdateApp();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });
        }

        private void ShellView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var askSave = MessageBox.Show("Do you want save ?", "Exit Application", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (askSave == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (askSave == MessageBoxResult.Yes)
                ((ShellViewModel)DataContext).Save();
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem == null) return;

            treeViewItem.Focus();
            //e.Handled = true;
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

       
    }

    public class BindingProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }




}
