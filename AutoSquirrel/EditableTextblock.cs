using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoSquirrel
{
    //
    // Code from : http://weblogs.asp.net/thomaslebrun/wpf-how-to-develop-and-editable-textblock

    [TemplatePart(Type = typeof(Grid), Name = EditableTextBlock.GRID_NAME)]
    [TemplatePart(Type = typeof(TextBlock), Name = EditableTextBlock.TEXTBLOCK_DISPLAYTEXT_NAME)]
    [TemplatePart(Type = typeof(TextBox), Name = EditableTextBlock.TEXTBOX_EDITTEXT_NAME)]
    public class EditableTextBlock : Control
    {
        #region Constants
        private const string GRID_NAME = "PART_GridContainer";
        private const string TEXTBLOCK_DISPLAYTEXT_NAME = "PART_TbDisplayText";
        private const string TEXTBOX_EDITTEXT_NAME = "PART_TbEditText";
        #endregion
        #region Member Fields
        private Grid m_GridContainer;
        private TextBlock m_TextBlockDisplayText;
        private TextBox m_TextBoxEditText;
        #endregion
        #region Dependency Properties
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock), new UIPropertyMetadata(string.Empty));
        public Brush TextBlockForegroundColor
        {
            get { return (Brush)GetValue(TextBlockForegroundColorProperty); }
            set { SetValue(TextBlockForegroundColorProperty, value); }
        }
        public static readonly DependencyProperty TextBlockForegroundColorProperty = DependencyProperty.Register("TextBlockForegroundColor", typeof(Brush), typeof(EditableTextBlock), new UIPropertyMetadata(null));
        public Brush TextBlockBackgroundColor
        {
            get { return (Brush)GetValue(TextBlockBackgroundColorProperty); }
            set { SetValue(TextBlockBackgroundColorProperty, value); }
        }
        public static readonly DependencyProperty TextBlockBackgroundColorProperty = DependencyProperty.Register("TextBlockBackgroundColor", typeof(Brush), typeof(EditableTextBlock), new UIPropertyMetadata(null));
        public Brush TextBoxForegroundColor
        {
            get { return (Brush)GetValue(TextBoxForegroundColorProperty); }
            set { SetValue(TextBoxForegroundColorProperty, value); }
        }
        public static readonly DependencyProperty TextBoxForegroundColorProperty = DependencyProperty.Register("TextBoxForegroundColor", typeof(Brush), typeof(EditableTextBlock), new UIPropertyMetadata(null));
        public Brush TextBoxBackgroundColor
        {
            get { return (Brush)GetValue(TextBoxBackgroundColorProperty); }
            set { SetValue(TextBoxBackgroundColorProperty, value); }
        }
        public static readonly DependencyProperty TextBoxBackgroundColorProperty = DependencyProperty.Register("TextBoxBackgroundColor", typeof(Brush), typeof(EditableTextBlock), new UIPropertyMetadata(null));
        #endregion
        #region Constructor
        static EditableTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTextBlock), new FrameworkPropertyMetadata(typeof(EditableTextBlock)));
        }
        #endregion
        #region Overrides Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.m_GridContainer = this.Template.FindName(GRID_NAME, this) as Grid;
            if (this.m_GridContainer != null)
            {
                this.m_TextBlockDisplayText = this.m_GridContainer.Children[0] as TextBlock;
                this.m_TextBoxEditText = this.m_GridContainer.Children[1] as TextBox;
                this.m_TextBoxEditText.LostFocus += this.OnTextBoxLostFocus;
                this.m_TextBoxEditText.LostKeyboardFocus += this.OnTextBoxLostFocus;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                this.m_TextBlockDisplayText.Visibility = Visibility.Visible;
                this.m_TextBoxEditText.Visibility = Visibility.Hidden;
            }
            base.OnKeyDown(e);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            this.m_TextBlockDisplayText.Visibility = Visibility.Hidden;
            this.m_TextBoxEditText.Visibility = Visibility.Visible;
        }
        #endregion
        #region Event Handlers
        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            this.m_TextBlockDisplayText.Visibility = Visibility.Visible;
            this.m_TextBoxEditText.Visibility = Visibility.Hidden;
        }
        #endregion
    }
}
