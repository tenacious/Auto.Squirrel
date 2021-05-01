using System;
using System.Diagnostics;
using System.Windows;

namespace AutoSquirrel
{
    /// <summary>
    /// Logica di interazione per WebConnectionEdit.xaml
    /// </summary>
    public partial class WebConnectionEdit : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebConnectionEdit"/> class.
        /// </summary>
        public WebConnectionEdit() => InitializeComponent();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) =>
            Process.Start("http://docs.aws.amazon.com/awscloudtrail/latest/userguide/cloudtrail-s3-bucket-naming-requirements.html");
    }
}
