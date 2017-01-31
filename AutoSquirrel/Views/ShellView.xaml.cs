namespace AutoSquirrel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using Squirrel;

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

        /// <summary>
        /// When implemented in a derived class, creates a new instance of the <see
        /// cref="T:System.Windows.Freezable"/> derived class.
        /// </summary>
        /// <returns>The new instance.</returns>
        protected override Freezable CreateInstanceCore() => new BindingProxy();
    }

    /// <summary>
    /// Shell View
    /// </summary>
    /// <seealso cref="MahApps.Metro.Controls.MetroWindow"/>
    /// <seealso cref="System.Windows.Markup.IComponentConnector"/>
    public partial class ShellView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellView"/> class.
        /// </summary>
        public ShellView()
        {
            InitializeComponent();

            Loaded += this.MainWindow_Loaded;

            this.PackageTreeview.PreviewMouseRightButtonDown += this.OnPreviewMouseRightButtonDown;

            Closing += this.ShellView_Closing;
        }

        private static MultiSelectTreeView VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is MultiSelectTreeView))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as MultiSelectTreeView;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                using (var mgr = new UpdateManager(@"https://s3-eu-west-1.amazonaws.com/autosquirrel", "AutoSquirrel"))
                {
                    try
                    {
                        if (mgr.IsInstalledApp)
                        {
                            var updates = await mgr.CheckForUpdate();
                            if (updates.ReleasesToApply.Any())
                            {
                                var lastVersion = updates.ReleasesToApply.OrderBy(x => x.Version).Last();
                                await mgr.DownloadReleases(new[] { lastVersion });
                                await mgr.ApplyReleases(updates);
                                await mgr.UpdateApp();

                                if (MessageBox.Show("The application has been updated - please restart.", "Restart?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    var ignore = Task.Run(() => UpdateManager.RestartApp());
                                }
                            }
                        }
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
            MultiSelectTreeView treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem == null)
            {
                return;
            }

            treeViewItem.Focus();

            //e.Handled = true;
        }

        private void PackageTreeview_OnSelecting(object sender, EventArgs e)
        {
            IList<ItemLink> items = new List<ItemLink>();
            foreach (var item in (sender as MultiSelectTreeView).SelectedItems)
            {
                items.Add(item as ItemLink);
            }

            ((ShellViewModel)this.DataContext).Model.SetSelectedItem(items);
        }

        private void ShellView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var askSave = MessageBox.Show("Do you want save?", "Exit Application", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (askSave == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (askSave == MessageBoxResult.Yes)
            {
                ((ShellViewModel)this.DataContext).Save();
            }
        }
    }
}