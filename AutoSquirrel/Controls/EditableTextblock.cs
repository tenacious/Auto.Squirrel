using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AutoSquirrel
{
    // Code from : http://weblogs.asp.net/thomaslebrun/wpf-how-to-develop-and-editable-textblock

    /// <summary>
    /// Editable Text Block
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Control"/>
    [TemplatePart(Type = typeof(Grid), Name = EditableTextBlock.GRID_NAME)]
    [TemplatePart(Type = typeof(TextBlock), Name = EditableTextBlock.TEXTBLOCK_DISPLAYTEXT_NAME)]
    [TemplatePart(Type = typeof(TextBox), Name = EditableTextBlock.TEXTBOX_EDITTEXT_NAME)]
    public class EditableTextBlock : Control
    {
        /// <summary>
        /// The text block background color property
        /// </summary>
        public static readonly DependencyProperty TextBlockBackgroundColorProperty = DependencyProperty.Register("TextBlockBackgroundColor", typeof(Brush), typeof(EditableTextBlock), new UIPropertyMetadata(null));

        /// <summary>
        /// The text block foreground color property
        /// </summary>
        public static readonly DependencyProperty TextBlockForegroundColorProperty = DependencyProperty.Register("TextBlockForegroundColor", typeof(Brush), typeof(EditableTextBlock), new UIPropertyMetadata(null));

        /// <summary>
        /// The text box background color property
        /// </summary>
        public static readonly DependencyProperty TextBoxBackgroundColorProperty = DependencyProperty.Register("TextBoxBackgroundColor", typeof(Brush), typeof(EditableTextBlock), new UIPropertyMetadata(null));

        /// <summary>
        /// The text box foreground color property
        /// </summary>
        public static readonly DependencyProperty TextBoxForegroundColorProperty = DependencyProperty.Register("TextBoxForegroundColor", typeof(Brush), typeof(EditableTextBlock), new UIPropertyMetadata(null));

        /// <summary>
        /// The text property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock), new UIPropertyMetadata(string.Empty));

        private const string GRID_NAME = "PART_GridContainer";
        private const string TEXTBLOCK_DISPLAYTEXT_NAME = "PART_TbDisplayText";
        private const string TEXTBOX_EDITTEXT_NAME = "PART_TbEditText";

        private Grid m_GridContainer;
        private TextBlock m_TextBlockDisplayText;
        private TextBox m_TextBoxEditText;

        static EditableTextBlock() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTextBlock), new FrameworkPropertyMetadata(typeof(EditableTextBlock)));

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// Gets or sets the color of the text block background.
        /// </summary>
        /// <value>The color of the text block background.</value>
        public Brush TextBlockBackgroundColor
        {
            get => (Brush)GetValue(TextBlockBackgroundColorProperty); set => SetValue(TextBlockBackgroundColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the color of the text block foreground.
        /// </summary>
        /// <value>The color of the text block foreground.</value>
        public Brush TextBlockForegroundColor
        {
            get => (Brush)GetValue(TextBlockForegroundColorProperty); set => SetValue(TextBlockForegroundColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the color of the text box background.
        /// </summary>
        /// <value>The color of the text box background.</value>
        public Brush TextBoxBackgroundColor
        {
            get => (Brush)GetValue(TextBoxBackgroundColorProperty); set => SetValue(TextBoxBackgroundColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the color of the text box foreground.
        /// </summary>
        /// <value>The color of the text box foreground.</value>
        public Brush TextBoxForegroundColor
        {
            get => (Brush)GetValue(TextBoxForegroundColorProperty); set => SetValue(TextBoxForegroundColorProperty, value);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.m_GridContainer = this.Template.FindName(GRID_NAME, this) as Grid;
            if (this.m_GridContainer != null) {
                this.m_TextBlockDisplayText = this.m_GridContainer.Children[0] as TextBlock;
                this.m_TextBoxEditText = this.m_GridContainer.Children[1] as TextBox;
                this.m_TextBoxEditText.LostFocus += this.OnTextBoxLostFocus;
                this.m_TextBoxEditText.LostKeyboardFocus += this.OnTextBoxLostFocus;
            }
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.KeyDown"/> attached
        /// event reaches an element in its route that is derived from this class. Implement this
        /// method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="T:System.Windows.Input.KeyEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                this.m_TextBlockDisplayText.Visibility = Visibility.Visible;
                this.m_TextBoxEditText.Visibility = Visibility.Hidden;
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Controls.Control.MouseDoubleClick"/> routed event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            this.m_TextBlockDisplayText.Visibility = Visibility.Hidden;
            this.m_TextBoxEditText.Visibility = Visibility.Visible;
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            this.m_TextBlockDisplayText.Visibility = Visibility.Visible;
            this.m_TextBoxEditText.Visibility = Visibility.Hidden;
        }
    }
}