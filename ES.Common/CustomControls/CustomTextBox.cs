using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ES.Common.CustomControls
{
    public class CustomCodeTextBox:TextBox
    {
        public static readonly DependencyProperty IsProductExistsProperty = DependencyProperty.Register(
            "IsProductExists", typeof (bool), typeof (CustomCodeTextBox), new PropertyMetadata(false));

        public bool IsProductExists
        {
            get { return (bool) GetValue(IsProductExistsProperty); }
            set { SetValue(IsProductExistsProperty, value); }
        }
        public CustomCodeTextBox()
        {
            
        }
    }
}
