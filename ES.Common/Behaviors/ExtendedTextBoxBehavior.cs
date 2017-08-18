using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ES.Common.Behaviors
{
    public class TextBoxBehavior : Behavior<TextBox>
    {
        #region Internal fields
        private bool _isSelectedAll;
        #endregion Internal fields

        #region Dependency properties
        public static readonly DependencyProperty IsFocusOnLoadProperty = DependencyProperty.Register("IsFocusOnLoad", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(false));
        public bool IsFocusOnLoad
        {
            get { return (bool)GetValue(IsFocusOnLoadProperty); }
            set { SetValue(IsFocusOnLoadProperty, value); }
        }

        public static readonly DependencyProperty IsFocusOnCleanProperty = DependencyProperty.Register("IsFocusOnClean", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(false));
        public bool IsFocusOnClean
        {
            get { return (bool)GetValue(IsFocusOnCleanProperty); }
            set { SetValue(IsFocusOnCleanProperty, value); }
        }

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

        public static readonly DependencyProperty IsFocusOnKeyboardFocusedProperty = DependencyProperty.Register("IsFocusOnKeyboardFocused", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(false));
        public bool IsFocusOnKeyboardFocused
        {
            get { return (bool)GetValue(IsFocusOnKeyboardFocusedProperty); }
            set { SetValue(IsFocusOnKeyboardFocusedProperty, value); }
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
        #endregion Dependency properties

        protected override void OnAttached()
        {
            if (IsFocusOnLoad) AssociatedObject.Loaded += OnLoaded;
            if (IsLeaveOnEnter) AssociatedObject.PreviewKeyUp += OnPreviewKeyUp;
            if (IsSelectTextOnFocus)
            {
                AssociatedObject.GotFocus += GotFocus;
                AssociatedObject.PreviewMouseUp += OnPreviewMouseUp;
            }
            if (IsFocusOnEnable) AssociatedObject.IsEnabledChanged += OnEnabledChanged;
            if (IsFocusOnTextChanged) AssociatedObject.TextChanged += TextChanged;
            if (IsFocusOnKeyboardFocused)
            {
                AssociatedObject.GotKeyboardFocus += OnGotKeyboardFocus;
            }
            if (IsFocusOnTextChanged)
            {
                AssociatedObject.TextChanged += OnTextChanged;
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                OnFocus(textBox);
            }
        }

        protected override void OnDetaching()
        {
            if (IsFocusOnLoad) AssociatedObject.Loaded -= OnLoaded;
            if (IsLeaveOnEnter) AssociatedObject.PreviewKeyUp -= OnPreviewKeyUp;
            if (IsSelectTextOnFocus)
            {
                AssociatedObject.GotFocus -= GotFocus;
                AssociatedObject.PreviewMouseUp -= OnPreviewMouseUp;
            }
            if (IsFocusOnEnable) AssociatedObject.IsEnabledChanged -= OnEnabledChanged;
            if (IsFocusOnTextChanged) AssociatedObject.TextChanged -= TextChanged;
            if (IsFocusOnTextChanged) AssociatedObject.TextChanged -= OnTextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            OnFocus(sender as TextBox);
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
        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            OnFocus(sender as TextBox);
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
