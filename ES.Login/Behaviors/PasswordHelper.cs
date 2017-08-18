using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using ES.Common.Helpers;

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
                //if (_passwordBox != null) _passwordBox.Password = Password.ToUnsecureString();
            }
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof (SecureString), typeof (PasswordBoxBindingBehavior), new PropertyMetadata(OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (e.OldValue == null)
            //{
            //    ((PasswordBoxBindingBehavior) d).Password = (SecureString)e.NewValue;
            //}
        }

        private void OnPasswordBoxValueChanged(object sender, RoutedEventArgs e)
        {
            Password = AssociatedObject.SecurePassword;
            if (_passwordBox == null) _passwordBox = sender as PasswordBox;
            return;
            //var binding = BindingOperations.GetBindingExpression(this, PasswordProperty);
            //if (binding != null)
            //{
            //    PropertyInfo property = binding.DataItem.GetType().GetProperty(binding.ParentBinding.Path.Path);
            //    if (property != null)
            //        property.SetValue(binding.DataItem, AssociatedObject.SecurePassword, null);
            //}
        }

        public static void SetIsClear(DependencyObject target, bool value)
        {
            target.SetValue(IsClearProperty, value);
        }

        public static readonly DependencyProperty IsClearProperty = DependencyProperty.RegisterAttached("IsClear", typeof(bool), typeof(PasswordBoxBindingBehavior), new PropertyMetadata(false, OnIsClear));

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

        public PasswordBoxBindingBehavior()
        {
            
        }
    }
}
