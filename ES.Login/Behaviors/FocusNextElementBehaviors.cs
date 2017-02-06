using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ES.Login.Behaviors
{
    class FocusNextElementBehaviors:Behavior<UIElement>    
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
}
