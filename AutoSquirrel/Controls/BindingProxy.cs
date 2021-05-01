using System.Windows;

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
            get => GetValue(DataProperty); set => SetValue(DataProperty, value);
        }

        /// <summary>
        /// When implemented in a derived class, creates a new instance of the <see
        /// cref="T:System.Windows.Freezable"/> derived class.
        /// </summary>
        /// <returns>The new instance.</returns>
        protected override Freezable CreateInstanceCore() => new BindingProxy();
    }
}
