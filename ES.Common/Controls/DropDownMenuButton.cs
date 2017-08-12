using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ES.Common.Controls
{
    public class DropDownMenuButton : ToggleButton
    {
        #region DropDownContextMenu

        public static readonly DependencyProperty DropDownContextMenuProperty =
            DependencyProperty.Register("DropDownContextMenu", typeof(ContextMenu), typeof(DropDownMenuButton),
                new FrameworkPropertyMetadata((ContextMenu)null, new PropertyChangedCallback(OnDropDownContextMenuChanged)));

        public ContextMenu DropDownContextMenu
        {
            get { return (ContextMenu)GetValue(DropDownContextMenuProperty); }
            set { SetValue(DropDownContextMenuProperty, value); }
        }

        private static void OnDropDownContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DropDownMenuButton)d).OnDropDownContextMenuChanged(e);
        }

        protected virtual void OnDropDownContextMenuChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldContextMenu = e.OldValue as ContextMenu;
            if (oldContextMenu != null && IsChecked.GetValueOrDefault())
                oldContextMenu.Closed -= new RoutedEventHandler(OnContextMenuClosed);
        }

        #endregion

        protected override void OnClick()
        {
            if (DropDownContextMenu != null)
            {
                DropDownContextMenu.PlacementTarget = this;
                DropDownContextMenu.Placement = PlacementMode.Bottom;
                DropDownContextMenu.Closed += new RoutedEventHandler(OnContextMenuClosed);
                DropDownContextMenu.IsOpen = true;
            }

            base.OnClick();
        }

        void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            var ctxMenu = sender as ContextMenu;
            if (ctxMenu == null) return;
            ctxMenu.Closed -= new RoutedEventHandler(OnContextMenuClosed);
            IsChecked = false;
        }
    }
}
