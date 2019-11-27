using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ES.Common.Behaviors
{
    /// <summary>
    /// A sync behaviour for a multiselector.
    /// </summary>
    public static class MultiSelectorBehaviours
    {
        public static readonly DependencyProperty SynchronizedSelectedItems = DependencyProperty.RegisterAttached(
            "SynchronizedSelectedItems", typeof(IList), typeof(MultiSelectorBehaviours));
        public static readonly DependencyProperty IsSynchronizeSelectedItemsProperty = DependencyProperty.RegisterAttached(
            "IsSynchronizeSelectedItems", typeof(bool), typeof(MultiSelectorBehaviours), new PropertyMetadata(false, OnIsSynchronizeSelectedItemsChanged));
        public static readonly DependencyProperty SynchronizedSelectedItemsChangedCommandProperty = DependencyProperty.RegisterAttached(
            "SynchronizedSelectedItemsChangedCommand", typeof(ICommand), typeof(MultiSelectorBehaviours), new PropertyMetadata(default(ICommand)));

        /// <summary>
        /// Gets the synchronized selected items.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns>The list that is acting as the sync list.</returns>
        public static IList GetSynchronizedSelectedItems(DependencyObject dependencyObject)
        {
            return (IList)dependencyObject.GetValue(SynchronizedSelectedItems);
        }
        /// <summary>
        /// Sets the synchronized selected items.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="value">The value to be set as synchronized items.</param>
        public static void SetSynchronizedSelectedItems(DependencyObject dependencyObject, IList value)
        {
            dependencyObject.SetValue(SynchronizedSelectedItems, value);
        }
        /// <summary>
        /// Gets the state of synchronization.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns>The state of synchronization.</returns>
        public static bool GetIsSynchronizeSelectedItems(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(IsSynchronizeSelectedItemsProperty);
        }
        /// <summary>
        /// Sets the state to allow or not synchronization of selected items.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="value">The value to be set to allow or not synchronization.</param>
        public static void SetIsSynchronizeSelectedItems(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(IsSynchronizeSelectedItemsProperty, value);
        }
        /// <summary>
        /// Gets the synchronized selected items command.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns>The command that is acting as the sync list.</returns>
        public static ICommand GetSynchronizedSelectedItemsChangedCommand(DependencyObject dependencyObject)
        {
            return (ICommand)dependencyObject.GetValue(SynchronizedSelectedItemsChangedCommandProperty);
        }
        /// <summary>
        /// Sets the synchronized selected items command.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="value">The value to be set as synchronized items command.</param>
        public static void SetSynchronizedSelectedItemsChangedCommand(DependencyObject dependencyObject, ICommand value)
        {
            dependencyObject.SetValue(SynchronizedSelectedItemsChangedCommandProperty, value);
        }

        private static void OnIsSynchronizeSelectedItemsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var selector = dependencyObject as Selector;

            if (selector != null && (bool)selector.GetValue(IsSynchronizeSelectedItemsProperty))
            {
                selector.SelectionChanged -= SelectorOnSelectionChanged;
                selector.SelectionChanged += SelectorOnSelectionChanged;
                selector.Loaded -= SelectorOnLoaded;
                selector.Loaded += SelectorOnLoaded;
            }
        }
        private static void SelectorOnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateSynchronizedSelectedItems(sender);
        }
        private static void SelectorOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            UpdateSynchronizedSelectedItems(sender);
            var element = sender as FrameworkElement;
            if (element != null)
            {
                var command = element.GetValue(SynchronizedSelectedItemsChangedCommandProperty) as ICommand;
                if (command != null)
                    command.Execute(selectionChangedEventArgs);
            }
        }
        private static void UpdateSynchronizedSelectedItems(object sender)
        {
            IList selectedItems;
            if (sender is ListBox)
            {
                var selector = (Selector)sender;
                if (!(selector is ListBox) || !(bool)selector.GetValue(IsSynchronizeSelectedItemsProperty)) return;
                selectedItems = (IList)selector.GetValue(ListBox.SelectedItemsProperty);
                if (selectedItems != null) selector.SetValue(SynchronizedSelectedItems, selectedItems);
            }
            else
            {
                var multiSelector = sender as MultiSelector;
                if (multiSelector == null || !((bool)multiSelector.GetValue(IsSynchronizeSelectedItemsProperty))) return;
                selectedItems = multiSelector.SelectedItems;
                multiSelector.SetValue(SynchronizedSelectedItems, selectedItems);
            }
        }
    }
}
