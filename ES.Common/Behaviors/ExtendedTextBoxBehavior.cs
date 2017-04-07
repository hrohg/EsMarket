using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ES.Common.Behaviors
{
    public class TextBoxBehavior : Behavior<TextBox>
    {
        private bool _isSelectedAll;

        public static readonly DependencyProperty IsLeaveOnEnterProperty = DependencyProperty.Register("IsLeaveOnEnter", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(IsLeaveOnEnterChanged));
        public bool IsLeaveOnEnter
        {
            get { return (bool)GetValue(IsLeaveOnEnterProperty); }
            set { SetValue(IsLeaveOnEnterProperty, value); }
        }

        public static readonly DependencyProperty IsSelectTextOnFocusProperty = DependencyProperty.Register("IsSelectTextOnFocus", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(false));
        public bool IsSelectTextOnFocus
        {
            get { return (bool)GetValue(IsSelectTextOnFocusProperty); }
            set { SetValue(IsSelectTextOnFocusProperty, value); }
        }

        public static readonly DependencyProperty IsFocusOnEnableProperty = DependencyProperty.Register("IsFocusOnEnable", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(false));
        public bool IsFocusOnEnable
        {
            get { return (bool)GetValue(IsFocusOnEnableProperty); }
            set { SetValue(IsFocusOnEnableProperty, value); }
        }

        public static readonly DependencyProperty IsFocusOnTextChangedProperty = DependencyProperty.Register("IsFocusOnTextChanged", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(false));
        public bool IsFocusOnTextChanged
        {
            get { return (bool)GetValue(IsFocusOnTextChangedProperty); }
            set { SetValue(IsFocusOnTextChangedProperty, value); }
        }

        public static readonly DependencyProperty CaretIndexProperty = DependencyProperty.RegisterAttached("CaretIndex", typeof(int), typeof(TextBoxBehavior), new PropertyMetadata(0, OnCaretIndexChanged));
        public int CaretIndex
        {
            get { return (int)GetValue(CaretIndexProperty); }
            set { SetValue(CaretIndexProperty, value); }
        }
        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.RegisterAttached("SelectionStart", typeof(int), typeof(TextBoxBehavior), new PropertyMetadata(0, OnSelectionChanged));
        public int SelectionStart
        {
            get { return (int)GetValue(SelectionStartProperty); }
            set { SetValue(SelectionStartProperty, value); }
        }
        public static readonly DependencyProperty SelectionLengthProperty = DependencyProperty.RegisterAttached("SelectionLength", typeof(int), typeof(TextBoxBehavior), new PropertyMetadata(0, OnSelectionChanged));
        public int SelectionLength
        {
            get { return (int)GetValue(SelectionLengthProperty); }
            set { SetValue(SelectionLengthProperty, value); }
        }

        protected override void OnAttached()
        {
            if (IsLeaveOnEnter) AssociatedObject.PreviewKeyUp += OnPreviewKeyUp;
            if (IsSelectTextOnFocus)
            {
                AssociatedObject.GotFocus += GotFocus;
                AssociatedObject.PreviewMouseUp += OnPreviewMouseUp;
            }
            if (IsFocusOnEnable) AssociatedObject.IsEnabledChanged += OnEnabledChanged;
            if (IsFocusOnTextChanged) AssociatedObject.TextChanged += TextChanged;
        }
        
        protected override void OnDetaching()
        {
            if (IsLeaveOnEnter) AssociatedObject.PreviewKeyUp -= OnPreviewKeyUp;
            if (IsSelectTextOnFocus)
            {
                AssociatedObject.GotFocus -= GotFocus;
                AssociatedObject.PreviewMouseUp -= OnPreviewMouseUp;
            }
            if (IsFocusOnEnable) AssociatedObject.IsEnabledChanged -= OnEnabledChanged;
            if (IsFocusOnTextChanged) AssociatedObject.TextChanged -= TextChanged;
        }
        
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            OnFocus(sender as TextBox);
        }
        private void OnEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            OnFocus(textBox);
        }

        private void OnFocus(TextBox textBox)
        {
            if (textBox != null && textBox.IsEnabled)
            {
                textBox.Focus();
            }
        }
        private static void IsLeaveOnEnterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as TextBoxBehavior;
            if (behavior == null || behavior.AssociatedObject == null) return;

            behavior.AssociatedObject.PreviewKeyUp -= behavior.OnPreviewKeyUp;
            if ((bool)e.NewValue)
                behavior.AssociatedObject.PreviewKeyUp += behavior.OnPreviewKeyUp;
        }
        private static void OnCaretIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as TextBox;
            if (textBox != null)
            {
                textBox.CaretIndex = (int)e.NewValue;
                return;
            }
            var behavior = d as TextBoxBehavior;
            if (behavior != null && behavior.AssociatedObject != null) behavior.AssociatedObject.CaretIndex = behavior.CaretIndex;
        }
        private bool _hasSelection;
        private bool _isEnabled;
        private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as TextBoxBehavior;
            if (behavior == null || behavior.AssociatedObject == null) return;
            if (!behavior._hasSelection && behavior.SelectionLength != 0)
            {
                behavior._isEnabled = behavior.AssociatedObject.IsEnabled;
                behavior._hasSelection = true;
                behavior.AssociatedObject.SetCurrentValue(UIElement.IsEnabledProperty, true);
                if (!behavior._isEnabled)
                {
                    behavior.AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
                    behavior.AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
                }
            }
            else if (behavior._hasSelection && behavior.SelectionLength == 0)
            {
                behavior._hasSelection = false;
                behavior.AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
                behavior.AssociatedObject.SetCurrentValue(UIElement.IsEnabledProperty, behavior._isEnabled);
            }
            behavior.AssociatedObject.Select(behavior.SelectionStart, behavior.SelectionLength);
            behavior.AssociatedObject.Focus();
        }
        private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            textBox.SetCurrentValue(UIElement.IsEnabledProperty, false);
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var request = new TraversalRequest(FocusNavigationDirection.Next) { Wrapped = true };

            var a = AssociatedObject.MoveFocus(request);
        }

        private void GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
                _isSelectedAll = true;
            }
        }
        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && _isSelectedAll)
            {
                _isSelectedAll = false;
                textBox.SelectAll();
                textBox.ReleaseMouseCapture();

            }
        }


    }
}
