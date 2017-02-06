using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls
{
    public class ExtendedLayoutBorder : Border
    {
        public static readonly DependencyProperty LayoutAnchorControlProperty = DependencyProperty.Register(
            "LayoutAnchorControl", typeof(LayoutAnchorControl), typeof(ExtendedLayoutBorder), new PropertyMetadata(null, LayoutAnchorChanged));
        public LayoutAnchorControl LayoutAnchorControl
        {
            get { return (LayoutAnchorControl)GetValue(LayoutAnchorControlProperty); }
            set { SetValue(LayoutAnchorControlProperty, value); }
        }
        public static readonly DependencyProperty IsShowAnchorOnMouseOverProperty = DependencyProperty.RegisterAttached(
            "IsShowAnchorOnMouseOver", typeof(bool), typeof(ExtendedLayoutBorder), new PropertyMetadata(true, IsShowAnchorOnMouseOverChanged));
        public bool IsShowAnchorOnMouseOver
        {
            get { return (bool)GetValue(IsShowAnchorOnMouseOverProperty); }
            set { SetValue(IsShowAnchorOnMouseOverProperty, value); }
        }
        private static void IsShowAnchorOnMouseOverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var customBorder = d as ExtendedLayoutBorder;
            if (customBorder == null || customBorder.IsShowAnchorOnMouseOver) return;
            customBorder.OnUpdateMouseOver();
        }
        private static void LayoutAnchorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var customBorder = d as ExtendedLayoutBorder;
            if (customBorder == null || customBorder.IsShowAnchorOnMouseOver) return;
            customBorder.OnUpdateMouseOver();
        }
        private void OnUpdateMouseOver()
        {
            if (LayoutAnchorControl == null) return;
            LayoutAnchorControl.MouseEnter -= AnchorOnMouseEnter;
            LayoutAnchorControl.MouseEnter += AnchorOnMouseEnter;
        }
        private static void AnchorOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            var anchor = sender as LayoutAnchorControl;
            if (anchor == null) return;
            var field = GetInstanceField(typeof(LayoutAnchorControl), anchor, "_openUpTimer") as DispatcherTimer;
            if (field == null) return;
            field.Stop();
        }
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (LayoutAnchorControl != null)
            {
                var anchorable = LayoutAnchorControl.Model as ExtendedLayoutAnchorable;
                if (anchorable == null) return;
                var side = anchorable.GetSide();
                if (anchorable.Root != null && anchorable.Root.Manager != null)
                {
                    var manager = anchorable.Root.Manager as DockingManagerBase;
                    if (manager != null)
                        manager.AnchorableTabItemPreviewMouseDown(anchorable, e);
                }
                if (side != AnchorSide.Bottom && side != AnchorSide.Top) return;
                if (anchorable.CanHide || !anchorable.IsAutoHidden || e.Handled) return;
                e.Handled = true;
                anchorable.ToggleAutoHide();
                anchorable.IsSelected = true;
            }
            else
            {
                base.OnPreviewMouseDown(e);
            }
        }

        /// <summary>
        /// Uses reflection to get the field value from an object.
        /// </summary>
        ///
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        ///
        /// <returns>The field value from the object.</returns>
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            return field != null ? field.GetValue(instance) : null;
        }
    }
}
