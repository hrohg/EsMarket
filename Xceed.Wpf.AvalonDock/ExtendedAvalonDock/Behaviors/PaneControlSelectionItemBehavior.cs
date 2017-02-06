using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Helpers;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Behaviors
{
    public class PaneControlSelectionItemBehavior : DependencyObject
    {
        private static double _anchorablePaneTabItemHeight = 0;
        public static readonly DependencyProperty ExtendPaneControlSelectionProperty = DependencyProperty.RegisterAttached(
            "ExtendPaneControlSelection", typeof(bool), typeof(PaneControlSelectionItemBehavior), new PropertyMetadata(false, OnExtendPaneControlSelection));
        public bool ExtendPaneControlSelection
        {
            get { return (bool)GetValue(ExtendPaneControlSelectionProperty); }
            set { SetValue(ExtendPaneControlSelectionProperty, value); }
        }
        public static readonly DependencyProperty IsMiddlePaneControlProperty = DependencyProperty.Register(
            "IsMiddlePaneControl", typeof (bool), typeof (PaneControlSelectionItemBehavior), new PropertyMetadata(false));
        public bool IsMiddlePaneControl
        {
            get { return (bool) GetValue(IsMiddlePaneControlProperty); }
            set { SetValue(IsMiddlePaneControlProperty, value); }
        }
        public static readonly DependencyProperty SelectedContentIndexProperty = DependencyProperty.Register(
            "SelectedContentIndex", typeof (int), typeof (PaneControlSelectionItemBehavior), new PropertyMetadata(-1));
        public int SelectedContentIndex
        {
            get { return (int) GetValue(SelectedContentIndexProperty); }
            set { SetValue(SelectedContentIndexProperty, value); }
        }
        private static void OnExtendPaneControlSelection(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var paneControl = d as LayoutAnchorablePaneControl;
            if (paneControl == null) return;
            if (_anchorablePaneTabItemHeight < 0.1)
                _anchorablePaneTabItemHeight =(double)paneControl.FindResource("AnchorablePaneTabItemHeight");
            paneControl.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => OnLoadCompleted(paneControl)));
            if (paneControl.Items == null) return;
            paneControl.SelectionChanged -= PaneControlOnSelectionChanged;
            paneControl.SelectionChanged += PaneControlOnSelectionChanged;
        }
        private static void PaneControlOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var paneControl = sender as LayoutAnchorablePaneControl;
            if (paneControl == null) return;
            var model = paneControl.Model as LayoutAnchorablePane;
            if (model == null) return;
            var selectedContent = model.SelectedContent as ExtendedLayoutAnchorable;
            if (selectedContent != null && !selectedContent.IsLayoutVisible)
            {
                SelectFirsVisibleChildren(model);
            }
            else if (e.AddedItems == null || e.AddedItems.Count == 0)
            {
                if (model.SelectedContentIndex == -1 && !paneControl.IsLoaded)
                    UpdateSelectedIndex(model);
            }
            else if (model.SelectedContentIndex != -1)
            {
                var ExtendedLayoutAnchorable = e.AddedItems[0] as ExtendedLayoutAnchorable;
                if (ExtendedLayoutAnchorable != null && ExtendedLayoutAnchorable.IsLayoutVisible)
                {
                    model.SetValue(SelectedContentIndexProperty, model.SelectedContentIndex);
                }
                else if(model.Children != null)
                {
                    if(!paneControl.IsLoaded || UpdateSelectedIndex(model) || SelectFirsVisibleChildren(model)) return;
                    model.SelectedContentIndex = -1;
                }
            }
        }
        public static bool SelectFirsVisibleChildren(LayoutAnchorablePane model)
        {
            for (int i = 0; i < model.Children.Count; i++)
            {
                var item = model.Children[i] as ExtendedLayoutAnchorable;
                if (item == null || !item.IsLayoutVisible) continue;
                model.SelectedContentIndex = i;
                return true;
            }
            return false;
        }
        private static bool UpdateSelectedIndex(LayoutAnchorablePane model)
        {
            var selectedIndex = (int) model.GetValue(SelectedContentIndexProperty);
            if (selectedIndex == -1 || model.Children.Count <= selectedIndex) return false;
            var item = model.Children[selectedIndex] as ExtendedLayoutAnchorable;
            if (item == null || !item.IsLayoutVisible) return false;
            if (model.SelectedContentIndex != selectedIndex)
                model.SelectedContentIndex = selectedIndex;
            return true;
        }
        private static void OnLoadCompleted(LayoutAnchorablePaneControl paneControl)
        {
            if (paneControl != null)
            {
                var layoutPanelControl = paneControl.FindParent<LayoutPanelControl>();
                var isMiddlePaneControl = layoutPanelControl != null && (layoutPanelControl.Children.OfType<LayoutDocumentPaneControl>().Any() || layoutPanelControl.Children.OfType<LayoutDocumentPaneGroupControl>().Any());
                if (isMiddlePaneControl)
                    paneControl.SetValue(IsMiddlePaneControlProperty, true);
                
                var model = paneControl.Model as LayoutAnchorablePane;
                if(model != null)
                    model.SetValue(IsMiddlePaneControlProperty, isMiddlePaneControl);
                if (model != null && model.Children != null && model.Children.Count > 0)
                {
                    foreach (var layoutAnchorable in model.Children)
                    {
                        layoutAnchorable.CanHide = !isMiddlePaneControl;
                        var cAnchorable = layoutAnchorable as ExtendedLayoutAnchorable;
                        if (cAnchorable != null && !cAnchorable.CanHide) cAnchorable.IsPinned = true;
                    }
                    var ExtendedLayoutAnchorable = model.Children.First() as ExtendedLayoutAnchorable;
                    if (ExtendedLayoutAnchorable != null) ExtendedLayoutAnchorable.OnParentLoaded();
                }
            }
        }
    }
}
