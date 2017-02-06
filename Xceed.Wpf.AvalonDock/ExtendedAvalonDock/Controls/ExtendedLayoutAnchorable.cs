using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls
{
    public enum AnchorableType
    {
        None,
        WorkSpace,
        Help,
        Profiler,
        Library,
        ToolsPanel
    };
    public class ExtendedLayoutAnchorable:LayoutAnchorable
    {
        private bool _isAnchorPaneLoaded;
        protected bool FreezeTabItemSelectionChange;
        public bool IsPinned { get; set; }
        public bool IsToggleAutoHide { get; set; }
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(AnchorableType), typeof(ExtendedLayoutAnchorable));
        public AnchorableType Type
        {
            get { return (AnchorableType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        /*this property used for external binding, it changed visibility when binded property value changed*/
        public static readonly DependencyProperty IsLayoutVisibleProperty = DependencyProperty.Register(
            "IsLayoutVisible", typeof(bool), typeof(ExtendedLayoutAnchorable), new PropertyMetadata(true, OnLayoutVisibilityChanged));
        public bool IsLayoutVisible
        {
            get { return (bool)GetValue(IsLayoutVisibleProperty); }
            set { SetValue(IsLayoutVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsAnchorablePaneAutoHiddenProperty = DependencyProperty.Register(
            "IsAnchorablePaneAutoHidden", typeof(bool), typeof(ExtendedLayoutAnchorable), new PropertyMetadata(false));
        public bool IsAnchorablePaneAutoHidden
        {
            get { return (bool)GetValue(IsAnchorablePaneAutoHiddenProperty); }
            set { SetValue(IsAnchorablePaneAutoHiddenProperty, value); }
        }
        public static readonly DependencyProperty IsTabItemSelectedProperty = DependencyProperty.Register(
            "IsTabItemSelected", typeof(bool), typeof(ExtendedLayoutAnchorable), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTabItemSelectionChanged));
        public bool IsTabItemSelected
        {
            get { return (bool)GetValue(IsTabItemSelectedProperty); }
            set { SetValue(IsTabItemSelectedProperty, value); }
        }

        private static void OnTabItemSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var customLayoutAnchorable = d as ExtendedLayoutAnchorable;
            if (customLayoutAnchorable == null || customLayoutAnchorable.FreezeTabItemSelectionChange || !customLayoutAnchorable.IsLayoutVisible) return;
            var pane = customLayoutAnchorable.Parent as LayoutAnchorablePane;
            if (pane == null || pane.Children == null)
            {
                customLayoutAnchorable.ToggleAutoHide();
                customLayoutAnchorable.IsSelected = true;
                return;
            }
            for (int i = 0; i < pane.Children.Count; i++)
            {
                if (!ReferenceEquals(pane.Children[i], customLayoutAnchorable)) continue;
                pane.SelectedContentIndex = i;
                break;
            }
        }

        private static void OnLayoutVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var customLayoutAnchorable = d as ExtendedLayoutAnchorable;
            if (customLayoutAnchorable == null) return;
            customLayoutAnchorable.UpdateAnchorablePaneAutoHidden();
        }

        private void UpdateAnchorablePaneAutoHidden()
        {
            var isAnchorablePaneAutoHidden = IsAnchorablePaneAutoHidden;
            if (IsAnchorablePaneAutoHidden)
            {
                IsAnchorablePaneAutoHidden = false;
                if (IsLayoutVisible && IsAutoHidden)
                    ToggleAutoHide();
            }
            var pane = Parent as ILayoutAnchorablePane;
            if (pane == null || pane.Children == null) return;
            if (isAnchorablePaneAutoHidden)
            {
                foreach (var child in pane.Children.Cast<ExtendedLayoutAnchorable>())
                {
                    if (child == null) continue;
                    child.IsAnchorablePaneAutoHidden = false;
                }
            }
            if (IsLayoutVisible || IsAutoHidden) return;
            foreach (var child in pane.Children.Cast<ExtendedLayoutAnchorable>())
            {
                if (child == null || !child.IsLayoutVisible) continue;
                return;
            }
            foreach (var child in pane.Children.Cast<ExtendedLayoutAnchorable>())
            {
                if (child == null) continue;
                child.IsAnchorablePaneAutoHidden = true;
            }
            if (!IsAutoHidden)
                ToggleAutoHide();
        }
        public void OnParentLoaded()
        {
            if (_isAnchorPaneLoaded) return;
            _isAnchorPaneLoaded = true;
            if (IsLayoutVisible) return;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateAnchorablePaneAutoHidden));
        }

        public static void GetAllAnchorableItems(ILayoutContainer model, List<LayoutAnchorableItem> anchorableItems)
        {
            if (model == null || anchorableItems == null) return;
            if (model is LayoutAnchorablePane)
            {
                var children = model.Children.OfType<LayoutContent>().ToList();
                anchorableItems.AddRange(children.Select(layoutContent => layoutContent.Root.Manager.GetLayoutItemFromModel(layoutContent)).OfType<LayoutAnchorableItem>());
            }
            else
            {
                var children = model.Children.ToList();
                foreach (var child in children)
                    GetAllAnchorableItems(child as ILayoutContainer, anchorableItems);
            }
        }
        public static void OnDockLayoutAnchorableItems(List<LayoutAnchorableItem> anchorableItems)
        {
            foreach (var layoutAnchorableItem in anchorableItems)
            {
                if (layoutAnchorableItem == null) continue;
                var customLayoutAnchorable = layoutAnchorableItem.LayoutElement as ExtendedLayoutAnchorable;
                if (customLayoutAnchorable == null) continue;
                layoutAnchorableItem.DockCommand.Execute(customLayoutAnchorable);
                SelectVisibleAnchorable(null, customLayoutAnchorable.Parent as LayoutAnchorablePane);
            }
        }

        private static void SelectVisibleAnchorable(LayoutAnchorablePane oldParent, LayoutAnchorablePane newParent)
        {
            for (int i = 0; i < 2; i++)
            {
                var parent = i == 0 ? oldParent : newParent;
                if (parent != null)
                {
                    var customLayoutAnchorable = parent.SelectedContent as ExtendedLayoutAnchorable;
                    if (customLayoutAnchorable == null || !customLayoutAnchorable.IsLayoutVisible)
                    {
                        foreach (var anchorable in parent.Children.Cast<ExtendedLayoutAnchorable>())
                        {
                            if (anchorable == null || !anchorable.IsLayoutVisible) continue;
                            anchorable.IsSelected = true;
                            break;
                        }
                    }
                }
            }
        }
        protected void UpdateBindings(DependencyObject target, DependencyProperty property)
        {
            var binding = BindingOperations.GetBinding(target, property);
            if (binding != null)
            {
                BindingOperations.SetBinding(this, property, binding);
                return;
            }
            var multiBinding = BindingOperations.GetMultiBinding(target, property);
            if (multiBinding != null)
                BindingOperations.SetBinding(this, property, multiBinding);
        }
        public virtual void SetCustomProperties(ExtendedLayoutAnchorable notDeserializedAnchorable)
        {
            Type = notDeserializedAnchorable.Type;
            Title = notDeserializedAnchorable.Title;
            ToolTip = notDeserializedAnchorable.ToolTip;
            UpdateBindings(notDeserializedAnchorable, IsLayoutVisibleProperty);
            UpdateBindings(notDeserializedAnchorable, IsTabItemSelectedProperty);
        }
    }
}
