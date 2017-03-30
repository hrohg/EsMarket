using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using Xceed.Wpf.AvalonDock.Commands;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Behaviors;
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
    [Serializable]
    public class ExtendedLayoutAnchorable : LayoutAnchorable
    {
        private bool _isAnchorPaneLoaded;
        protected bool FreezeTabItemSelectionChange;
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
        public static readonly DependencyProperty IsTabItemSelectedProperty = DependencyProperty.Register(
            "IsTabItemSelected", typeof(bool), typeof(ExtendedLayoutAnchorable), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTabItemSelectionChanged));
        public bool IsTabItemSelected
        {
            get { return (bool)GetValue(IsTabItemSelectedProperty); }
            set { SetValue(IsTabItemSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register(
            "IsSelectable", typeof(bool), typeof(ExtendedLayoutAnchorable), new PropertyMetadata(true));
        public bool IsSelectable
        {
            get { return (bool)GetValue(IsSelectableProperty); }
            set { SetValue(IsSelectableProperty, value); }
        }
        public static readonly DependencyProperty TitleContentProperty = DependencyProperty.Register(
            "TitleContent", typeof(FrameworkElement), typeof(ExtendedLayoutAnchorable), new PropertyMetadata(null));
        public FrameworkElement TitleContent
        {
            get { return (FrameworkElement)GetValue(TitleContentProperty); }
            set { SetValue(TitleContentProperty, value); }
        }
        public static readonly DependencyProperty AnchorSideProperty = DependencyProperty.Register(
            "AnchorSide", typeof(AnchorSide?), typeof(ExtendedLayoutAnchorable), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public AnchorSide? AnchorSide
        {
            get { return (AnchorSide?)GetValue(AnchorSideProperty); }
            set { SetValue(AnchorSideProperty, value); }
        }
        public static readonly DependencyProperty IsAnchorablePaneAutoHiddenProperty = DependencyProperty.Register(
            "IsAnchorablePaneAutoHidden", typeof(bool), typeof(ExtendedLayoutAnchorable), new PropertyMetadata(false));
        public bool IsAnchorablePaneAutoHidden
        {
            get { return (bool)GetValue(IsAnchorablePaneAutoHiddenProperty); }
            set { SetValue(IsAnchorablePaneAutoHiddenProperty, value); }
        }
        public static readonly DependencyProperty BindableTitleProperty = DependencyProperty.Register(
            "BindableTitle", typeof(string), typeof(ExtendedLayoutAnchorable), new PropertyMetadata(null, OnBindeableTitleChanged));
        public string BindableTitle
        {
            get { return (string)GetValue(BindableTitleProperty); }
            set { SetValue(BindableTitleProperty, value); }
        }
        public static readonly DependencyProperty MiddleTitleProperty = DependencyProperty.Register(
            "MiddleTitle", typeof(string), typeof(ExtendedLayoutAnchorable), new PropertyMetadata(""));
        public string MiddleTitle
        {
            get { return (string)GetValue(MiddleTitleProperty); }
            set { SetValue(MiddleTitleProperty, value); }
        }
        public static readonly DependencyProperty BindableTitlePostFixProperty = DependencyProperty.Register(
            "BindableTitlePostFix", typeof(string), typeof(ExtendedLayoutAnchorable), new PropertyMetadata(""));
        public string BindableTitlePostFix
        {
            get { return (string)GetValue(BindableTitlePostFixProperty); }
            set { SetValue(BindableTitlePostFixProperty, value); }
        }
        public bool IsPinned { get; set; }
        public bool IsToggleAutoHide { get; set; }
        public ICommand FloatCommand { get; set; }
        public ICommand DockCommand { get; set; }
        public ExtendedLayoutAnchorable()
        {
            FloatCommand = new RelayCommand((p) => OnFloatCommand(p), (p) => CanExecuteFloatCommand(p));
            DockCommand = new RelayCommand((p) => OnDockCommand(p), (p) => CanExecuteDockCommand(p));
            if (this.Title == null)
            {
                this.ToggleAutoHided();
            }
        }
        private bool CanExecuteFloatCommand(object anchorable)
        {
            return CanFloat && this.FindParent<LayoutFloatingWindow>() == null;
        }
        private void OnFloatCommand(object layoutItem)
        {
            var layoutAnchorableItem = layoutItem as LayoutAnchorableItem;
            if (layoutAnchorableItem == null || layoutAnchorableItem.FloatCommand == null) return;
            OnStartFloating();
            layoutAnchorableItem.FloatCommand.Execute(this);
        }
        private bool CanExecuteDockCommand(object layoutItem)
        {
            return this.FindParent<LayoutAnchorableFloatingWindow>() != null;
        }
        private void OnDockCommand(object layoutItem)
        {
            var layoutAnchorableItem = layoutItem as LayoutAnchorableItem;
            if (layoutAnchorableItem == null || layoutAnchorableItem.DockCommand == null) return;
            var oldParent = Parent as LayoutAnchorablePane;
            var isDockAllChildren = false;
            if (oldParent != null)
            {
                foreach (var anchorable in oldParent.Children.Cast<ExtendedLayoutAnchorable>())
                {
                    if (anchorable == null || ReferenceEquals(anchorable, this)) continue;
                    if (anchorable.IsLayoutVisible) break;
                    isDockAllChildren = true;
                }
            }
            if (isDockAllChildren)
            {
                var anchorableItems = new List<LayoutAnchorableItem>();
                GetAllAnchorableItems(oldParent, anchorableItems);
                OnDockLayoutAnchorableItems(anchorableItems);
            }
            else
            {
                layoutAnchorableItem.DockCommand.Execute(this);
                SelectVisibleAnchorable(oldParent, Parent as LayoutAnchorablePane);
            }
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
                var extendedLayoutAnchorable = layoutAnchorableItem.LayoutElement as ExtendedLayoutAnchorable;
                if (extendedLayoutAnchorable == null) continue;
                layoutAnchorableItem.DockCommand.Execute(extendedLayoutAnchorable);
                SelectVisibleAnchorable(null, extendedLayoutAnchorable.Parent as LayoutAnchorablePane);
            }
        }
        public void OnStartFloating()
        {
            var children = Parent.Children;
            if (children == null) return;
            ExtendedLayoutAnchorable layoutAnchorable = null;
            foreach (var child in children.Cast<ExtendedLayoutAnchorable>())
            {
                if (child == null || child.AnchorSide != AnchorSide
                    || ReferenceEquals(child, this)) continue;
                if (child.IsLayoutVisible) return;
                layoutAnchorable = child;
            }
            if (layoutAnchorable != null && !layoutAnchorable.IsAutoHidden)
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => layoutAnchorable.ToggleAutoHide()));
        }
        private static void SelectVisibleAnchorable(LayoutAnchorablePane oldParent, LayoutAnchorablePane newParent)
        {
            for (int i = 0; i < 2; i++)
            {
                var parent = i == 0 ? oldParent : newParent;
                if (parent != null)
                {
                    var extendedLayoutAnchorable = parent.SelectedContent as ExtendedLayoutAnchorable;
                    if (extendedLayoutAnchorable == null || !extendedLayoutAnchorable.IsLayoutVisible)
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
        private static void OnLayoutVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var extendedLayoutAnchorable = d as ExtendedLayoutAnchorable;
            if (extendedLayoutAnchorable == null) return;
            extendedLayoutAnchorable.UpdateAnchorablePaneAutoHidden();
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
        private static void OnBindeableTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var extendedLayoutAnchorable = d as ExtendedLayoutAnchorable;
            if (extendedLayoutAnchorable == null) return;
            extendedLayoutAnchorable.Title = extendedLayoutAnchorable.BindableTitle;
        }
        private static void OnTabItemSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var extendedLayoutAnchorable = d as ExtendedLayoutAnchorable;
            if (extendedLayoutAnchorable == null || extendedLayoutAnchorable.FreezeTabItemSelectionChange || !extendedLayoutAnchorable.IsLayoutVisible) return;
            var pane = extendedLayoutAnchorable.Parent as LayoutAnchorablePane;
            if (pane == null || pane.Children == null)
            {
                extendedLayoutAnchorable.ToggleAutoHide();
                extendedLayoutAnchorable.IsSelected = true;
                return;
            }
            for (int i = 0; i < pane.Children.Count; i++)
            {
                if (!ReferenceEquals(pane.Children[i], extendedLayoutAnchorable)) continue;
                pane.SelectedContentIndex = i;
                break;
            }
        }
        protected override void OnIsSelectedChanged(bool oldValue, bool newValue)
        {
            base.OnIsSelectedChanged(oldValue, newValue);
            var manager = GetManager();
            if (manager != null && !manager.IsLoaded) return;
            FreezeTabItemSelectionChange = true;
            IsTabItemSelected = IsSelected;
            FreezeTabItemSelectionChange = false;
        }
        private void UpdateAnchorSide(LayoutAnchorablePane pane, AnchorSide? anchorSide)
        {
            if (pane == null) return;
            foreach (var child in pane.Children.Cast<ExtendedLayoutAnchorable>())
                child.AnchorSide = anchorSide;
        }
        protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
        {
            base.OnParentChanged(oldValue, newValue);
            if (newValue != null)
            {
                var layoutAnchorablePane = newValue as LayoutAnchorablePane;
                if (layoutAnchorablePane != null)
                {
                    foreach (var child in layoutAnchorablePane.Children.Cast<ExtendedLayoutAnchorable>())
                    {
                        if (child == null || ReferenceEquals(child, this)) continue;
                        IsPinned = child.IsPinned;
                        break;
                    }
                    UpdateAnchorSide(layoutAnchorablePane, layoutAnchorablePane.Root != null ? GetSide() : (AnchorSide?)null);
                    CanHide = !((bool)layoutAnchorablePane.GetValue(PaneControlSelectionItemBehavior.IsMiddlePaneControlProperty));
                    if (!CanHide) IsPinned = true;
                }
                else
                {
                    AnchorSide = null;
                }
            }
        }
        protected override void OnContentChanged(object oldVale, object newValue)
        {
            base.OnContentChanged(oldVale, newValue);

            var paneContent = oldVale as FrameworkElement;
            if (paneContent != null)
            {
                paneContent.Loaded -= PaneContentOnloaded;
                paneContent.Unloaded -= PaneContentOnUnloaded;
            }
            paneContent = newValue as FrameworkElement;
            if (paneContent != null)
            {
                paneContent.Loaded -= PaneContentOnloaded;
                paneContent.Unloaded -= PaneContentOnUnloaded;
                paneContent.Loaded += PaneContentOnloaded;
                paneContent.Unloaded += PaneContentOnUnloaded;
            }
        }
        protected override void PreviewToggleAutoHide()
        {
            base.PreviewToggleAutoHide();
            var manager = GetManager();
            if (manager != null)
                manager.PreviewToggleAutoHide(this);
        }
        protected override void ToggleAutoHided()
        {
            base.ToggleAutoHided();
            var manager = GetManager();
            if (manager != null)
                manager.ToggleAutoHided(this);
        }

        private void PaneContentOnUnloaded(object sender, RoutedEventArgs e)
        {
            PaneContentLoaded(false);
            if (Root != null && Root.Manager != null && !Root.Manager.IsLoaded) return;
            FreezeTabItemSelectionChange = true;
            IsTabItemSelected = false;
            FreezeTabItemSelectionChange = false;
        }
        private void PaneContentOnloaded(object sender, RoutedEventArgs e)
        {
            PaneContentLoaded(true);
        }
        private DockingManagerBase GetManager()
        {
            if (Root == null || Root.Manager == null) return null;
            return Root.Manager;
        }
        private void PaneContentLoaded(bool isLoaded)
        {
            var manager = GetManager();
            if (manager != null)
                manager.PaneContentLoaded(this, isLoaded);
        }
        public void OnTabItemPreviewMouseDown()
        {
            FreezeTabItemSelectionChange = true;
            IsTabItemSelected = true;
            FreezeTabItemSelectionChange = false;
        }
        public void OnParentLoaded()
        {
            if (_isAnchorPaneLoaded) return;
            _isAnchorPaneLoaded = true;
            if (IsLayoutVisible) return;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateAnchorablePaneAutoHidden));
        }
        public AnchorSide GetSide()
        {
            #region Anchorable is already auto hidden

            if (IsAutoHidden)
            {
                var parentGroup = Parent as LayoutAnchorGroup;
                if (parentGroup != null)
                {
                    var layoutAnchorSide = parentGroup.Parent as LayoutAnchorSide;
                    if (layoutAnchorSide != null)
                        return layoutAnchorSide.Side;
                }
            }
            #endregion
            #region Anchorable is docked

            var pane = Parent as LayoutAnchorablePane;
            if (pane != null)
            {
                UpdateAnchorSide(pane, pane.GetSide());
                return AnchorSide ?? Layout.AnchorSide.Bottom;
            }
            return Layout.AnchorSide.Bottom;

            #endregion
        }
        public override void WriteXml(XmlWriter writer)
        {
            if (IsPinned) writer.WriteAttributeString("IsPinned", IsPinned.ToString());
            if (IsToggleAutoHide) writer.WriteAttributeString("IsToggleAutoHide", IsToggleAutoHide.ToString());
            base.WriteXml(writer);
        }
        public override void ReadXml(XmlReader reader)
        {
            bool boolValue;
            if (reader.MoveToAttribute("IsPinned"))
            {
                bool.TryParse(reader.Value, out boolValue);
                IsPinned = boolValue;
            }
            if (reader.MoveToAttribute("IsToggleAutoHide"))
            {
                bool.TryParse(reader.Value, out boolValue);
                IsToggleAutoHide = boolValue;
            }

            base.ReadXml(reader);
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
            UpdateBindings(notDeserializedAnchorable, BindableTitleProperty);
            UpdateBindings(notDeserializedAnchorable, MiddleTitleProperty);
            UpdateBindings(notDeserializedAnchorable, BindableTitlePostFixProperty);
        }
    }
}
