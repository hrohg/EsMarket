using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ES.Common.Behaviors
{
    public static class ComboBoxItemHighlightBehavior
    {
        public static readonly DependencyProperty HighlightedItemProperty =
            DependencyProperty.RegisterAttached(
                "HighlightedItem",
                typeof(object),
                typeof(ComboBoxItemHighlightBehavior));

        public static void SetHighlightedItem(UIElement element, object value)
        {
            element.SetValue(HighlightedItemProperty, value);
        }

        public static object GetHighlightedItem(UIElement element)
        {
            return element.GetValue(HighlightedItemProperty);
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            var comboBoxItem = sender as ComboBoxItem;
            if (comboBoxItem == null)
            {
                return;
            }

            SetHighlightedItem(comboBoxItem, comboBoxItem.DataContext);
        }

        public static readonly DependencyProperty EnabledProperty =
    DependencyProperty.RegisterAttached(
        "Enabled",
        typeof(bool),
        typeof(ComboBoxItemHighlightBehavior),
        new PropertyMetadata(false, OnEnabledChange));

        public static void SetEnabled(UIElement element, bool value)
        {
            element.SetValue(EnabledProperty, value);
        }

        public static bool EnabledItem(UIElement element)
        {
            return (bool)element.GetValue(EnabledProperty);
        }

        private static void OnEnabledChange(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var comboBoxItem = d as ComboBoxItem;
            if (comboBoxItem==null)
            {
                return;
            }

            comboBoxItem.MouseMove -= OnMouseMove;
            if (e.NewValue is true)
            {
                comboBoxItem.MouseMove += OnMouseMove;
            }
        }
    }
}
