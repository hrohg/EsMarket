using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using ES.Common.CustomControls;

namespace ES.Common.Behaviors
{
    public class FocusNextElementBehaviors : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyDown += AssociatedObjectLostKeyDown;
        }

        private void AssociatedObjectLostKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            var request = new TraversalRequest(FocusNavigationDirection.Next) { Wrapped = false };
            AssociatedObject.MoveFocus(request);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.KeyDown -= AssociatedObjectLostKeyDown;
            base.OnDetaching();
        }
    }

    public class CodeTextBoxFocusNextElementBehaviors : Behavior<CustomCodeTextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyDown += AssociatedObjectLostKeyDown;
        }

        private void AssociatedObjectLostKeyDown(object sender, KeyEventArgs e)
        {
            if(!((CustomCodeTextBox)sender).IsProductExists) return;
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            var request = new TraversalRequest(FocusNavigationDirection.Next) { Wrapped = false };
            AssociatedObject.MoveFocus(request);

        }

        protected override void OnDetaching()
        {
            AssociatedObject.KeyDown -= AssociatedObjectLostKeyDown;
            base.OnDetaching();
        }
    }
}
