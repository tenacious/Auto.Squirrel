using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Squirrel;

namespace AutoSquirrel
{
    /// <summary>
    /// Binding Proxy
    /// </summary>
    /// <seealso cref="System.Windows.Freezable"/>
    public class BindingProxy : Freezable
    {
        /// <summary>
        /// The data property
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        protected override Freezable CreateInstanceCore() => new BindingProxy();
    }

    /// <summary>
    /// Shell View
    /// </summary>
    /// <seealso cref="MahApps.Metro.Controls.MetroWindow"/>
    /// <seealso cref="System.Windows.Markup.IComponentConnector"/>
    public partial class ShellView
    {
        public ShellView()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            KeyDown += ShellView_KeyDown;

            PackageTreeview.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;

            Closing += ShellView_Closing;
        }

        private static TreeViewExItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewExItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewExItem;
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
                        MessageBox.Show("From Update Manager : " + Environment.NewLine + ex.InnerException.Message + Environment.NewLine + ex.Message);
                    }
                }
            });
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewExItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem == null) return;

            treeViewItem.Focus();

            //e.Handled = true;
        }

        private void PackageTreeview_OnSelecting(object sender, SelectionChangedCancelEventArgs e)
        {
            IList<ItemLink> items = new List<ItemLink>();
            foreach (var item in (sender as TreeViewEx).SelectedItems)
            {
                items.Add(item as ItemLink);
            }

            ((ShellViewModel)DataContext)._model.SetSelectedItem(items as IList<ItemLink>);
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

        private void ShellView_KeyDown(object sender, KeyEventArgs e)
        {
        }
    }
}