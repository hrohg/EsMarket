using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ES.Common.Behaviors
{
    public class GetFocusBehavior:Behavior<TextBox>
    {
        public static readonly DependencyProperty IsFocusOnTextChangedProperty = DependencyProperty.Register("IsFocusOnTextChanged", typeof(bool), typeof(TextBoxBehavior));
        public bool IsFocusOnTextChanged
        {
            get { return (bool)GetValue(IsFocusOnTextChangedProperty); }
            set { SetValue(IsFocusOnTextChangedProperty, value); }
        }
        protected override void OnAttached()
        {
            AssociatedObject.TextChanged += OnTextChanged;
            AssociatedObject.GotFocus += GotFocus;
        }

        private void GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && IsFocusOnTextChanged && textBox.IsFocused)
            {
                textBox.Focus();
                textBox.SelectAll();
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= OnTextChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox!=null && IsFocusOnTextChanged && textBox.IsFocused)
            {
                textBox.Focus();
                textBox.SelectAll();
            }
        }
        
    }
}
