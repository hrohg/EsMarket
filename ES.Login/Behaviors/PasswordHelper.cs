using System;
using System.Reflection;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace ES.Login.Behaviors
{
    public class PasswordBoxBindingBehavior : Behavior<PasswordBox>
    {
        private PasswordBox _passwordBox;
        protected override void OnAttached()
        {
            AssociatedObject.PasswordChanged += OnPasswordBoxValueChanged;
        }

        public SecureString Password
        {
            get { return (SecureString) GetValue(PasswordProperty); }
            set
            {
                SetValue(PasswordProperty, value);
                if (_passwordBox != null) _passwordBox.Clear();
            }
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof (SecureString), typeof (PasswordBoxBindingBehavior),
                new PropertyMetadata(null));

        
        private void OnPasswordBoxValueChanged(object sender, RoutedEventArgs e)
        {
            var binding = BindingOperations.GetBindingExpression(this, PasswordProperty);
            if (binding != null)
            {
                PropertyInfo property = binding.DataItem.GetType().GetProperty(binding.ParentBinding.Path.Path);
                if (property != null)
                    property.SetValue(binding.DataItem, AssociatedObject.SecurePassword, null);
            }
            if(_passwordBox==null) _passwordBox = sender as PasswordBox;
        }

        public static void SetIsClear(DependencyObject target, bool value)
        {
            target.SetValue(IsClearProperty, value);
        }

        public static readonly DependencyProperty IsClearProperty =
                                                  DependencyProperty.RegisterAttached("IsClear",
                                                  typeof(bool),
                                                  typeof(PasswordBoxBindingBehavior), new PropertyMetadata(false, OnIsClear)                                                  );

        private static void OnIsClear(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool && ((bool)e.NewValue))
            {
                PasswordBoxBindingBehavior passwordBox = sender as PasswordBoxBindingBehavior;

                if (passwordBox != null && passwordBox._passwordBox!=null)
                {
                    passwordBox._passwordBox.Clear();
                }
            }
        }        
    }
}
