using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using ES.Common.Helpers;

namespace ES.Common.Behaviors
{
    public class PasswordBoxBindingBehavior : Behavior<PasswordBox>
    {
        private PasswordBox _passwordBox;
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PasswordChanged += OnPasswordBoxValueChanged;
            _passwordBox = AssociatedObject;
            _passwordBox.Password = Password.ToUnsecureString();
        }
        protected override void OnDetaching()
        {
            AssociatedObject.PasswordChanged -= OnPasswordBoxValueChanged;
            base.OnDetaching();
        }

        public SecureString Password
        {
            get { return (SecureString)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof(SecureString), typeof(PasswordBoxBindingBehavior), new PropertyMetadata(OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == null)
            {
                PasswordBoxBindingBehavior behavior = d as PasswordBoxBindingBehavior;
                if (behavior != null)
                {
                    PasswordBox item = behavior.AssociatedObject as PasswordBox;
                    if (item == null) return;

                    var value = e.NewValue as string;
                    if (item.Password != value && value != null)
                    {
                        item.Password = value;
                    }
                }
            }
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

                if (passwordBox != null && passwordBox._passwordBox != null)
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
