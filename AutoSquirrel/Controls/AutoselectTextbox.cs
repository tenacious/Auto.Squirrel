using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AutoSquirrel
{
    /// <summary>
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
        public static bool GetActive(DependencyObject @object)
        {
            return (bool)@object.GetValue(ActiveProperty);
        }

        /// <summary>
        /// Sets the active.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetActive(DependencyObject @object, bool value)
        {
            @object.SetValue(ActiveProperty, value);
        }

        private static void ActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as TextBox;

            if (textBox != null)
            {
                if ((e.NewValue as bool?).GetValueOrDefault(false))
                {
                    textBox.GotKeyboardFocus += OnKeyboardFocusSelectText;
                    textBox.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                }
                else
                {
                    textBox.GotKeyboardFocus -= OnKeyboardFocusSelectText;
                    textBox.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
                }
            }
        }

        private static DependencyObject GetParentFromVisualTree(object source)
        {
            DependencyObject parent = source as UIElement;
            while (parent != null && !(parent is TextBox))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }

        private static void OnKeyboardFocusSelectText(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = e.OriginalSource as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dependencyObject = GetParentFromVisualTree(e.OriginalSource);

            if (dependencyObject == null)
            {
                return;
            }

            var textBox = (TextBox)dependencyObject;
            if (!textBox.IsKeyboardFocusWithin)
            {
                textBox.Focus();
                e.Handled = true;
            }
        }
    }
}