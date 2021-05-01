using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AutoSquirrel
{
    /// <summary>
    /// Select Text On Focus
    /// </summary>
    /// <seealso cref="System.Windows.DependencyObject"/>
    public class SelectTextOnFocus : DependencyObject
    {
        /// <summary>
        /// The active property
        /// </summary>
        public static readonly DependencyProperty ActiveProperty = DependencyProperty.RegisterAttached(
            "Active",
            typeof(bool),
            typeof(SelectTextOnFocus),
            new PropertyMetadata(false, ActivePropertyChanged));

        /// <summary>
        /// Gets the active.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForChildrenAttribute(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static bool GetActive(DependencyObject @object) => (bool)@object.GetValue(ActiveProperty);

        /// <summary>
        /// Sets the active.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetActive(DependencyObject @object, bool value) => @object.SetValue(ActiveProperty, value);

        private static void ActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox) {
                if ((e.NewValue as bool?).GetValueOrDefault(false)) {
                    textBox.GotKeyboardFocus += OnKeyboardFocusSelectText;
                    textBox.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                } else {
                    textBox.GotKeyboardFocus -= OnKeyboardFocusSelectText;
                    textBox.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
                }
            }
        }

        private static DependencyObject GetParentFromVisualTree(object source)
        {
            DependencyObject parent = source as UIElement;
            while (parent != null && !(parent is TextBox)) {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }

        private static void OnKeyboardFocusSelectText(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.OriginalSource is TextBox textBox) {
                textBox.SelectAll();
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dependencyObject = GetParentFromVisualTree(e.OriginalSource);

            if (dependencyObject == null) {
                return;
            }

            var textBox = (TextBox)dependencyObject;
            if (!textBox.IsKeyboardFocusWithin) {
                textBox.Focus();
                e.Handled = true;
            }
        }
    }
}
