using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ResourceLibrary.Behaviors
{
    public class ListBoxSelectionBringIntoViewBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += AssociatedObjectSelectionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= AssociatedObjectSelectionChanged;
        }
        private void AssociatedObjectSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0) AssociatedObject.ScrollIntoView(e.AddedItems[e.AddedItems.Count - 1]);
        }
    }
}
