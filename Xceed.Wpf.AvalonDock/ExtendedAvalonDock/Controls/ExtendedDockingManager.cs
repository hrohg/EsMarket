using System;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using System.IO;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Commands;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Behaviors;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Helpers;
using Extentions = Xceed.Wpf.AvalonDock.Controls.Extentions;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls
{
    public class ExtendedDockingManager : DockingManager
    {
        //private IEventAggregator _eventAggregator;
        private const string DefaultLayoutPath = "ES.Common.DefaultLayout.config";

        private string GetEsmTempPath
        {
            get
            {
                var path = Path.Combine(Path.GetTempPath(), "ESM");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        private const string LayoutFileName = "Layout.config";
        #region Constructors

        public ExtendedDockingManager()
        {
            AutoHideCommand = new RelayCommand<ExtendedLayoutAnchorable>(OnAutoHideCommand);
            HideCommand = new RelayCommand<ExtendedLayoutAnchorable>(OnHideCommand);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public override bool IsAutoHiding { get; set; }
        public override bool IsFloating { get; set; }
        public override bool IsMiddleBottomSideEnabled { get { return true; } }

        public ExtendedLayoutAnchorable ProfilerAnchorable
        {
            get { return AnchorableChildPanes.FirstOrDefault(x => x.Type == AnchorableType.Profiler); }
        }


        private LayoutAnchorControl GetCurrentLayoutAnchorControl(ExtendedLayoutAnchorable pane)
        {
            var controls = Extentions.FindVisualChildren<ExtendedLayoutBorder>(this).ToList();
            foreach (var control in controls)
            {
                var contentPresenter = control.FindChild<ContentPresenter>();
                if (contentPresenter != null)
                {
                    if (ReferenceEquals(contentPresenter.DataContext, pane))
                    {
                        return contentPresenter.FindParent<LayoutAnchorControl>();
                    }
                }
            }
            return null;
        }
        private void OnExecuteProfiler(object o)
        {
            var pane = AnchorableChildPanes.FirstOrDefault(x => x.Type == AnchorableType.Profiler);
            if (pane == null) return;
            var content = pane.Content as FrameworkElement;
            if (content == null) return;
            if (!content.IsVisible)
            {
                //if (!pane.IsAutoHidden) pane.ToggleAutoHide();
                //else pane.IsActive = true;

                if (!pane.IsAutoHidden) pane.IsSelected = true;
            }
        }
        //private void OnShowAnchorablePane(AnchorableType type)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        var pane = AnchorableChildPanes.FirstOrDefault(x => x.Type == type);
        //        if (pane == null) return;
        //        if (!pane.IsVisible)
        //        {
        //            pane.IsVisible = true;
        //            return;
        //        }
        //        var content = pane.Content as FrameworkElement;
        //        if (content == null) return;
        //        if (content.IsVisible) return;
        //        pane.IsSelected = true;
        //        ShowAnchorablePaneControl(pane);
        //    });
        //}
        //private void OnPinToolsPanelAnchorablePane(AnchorableToolsPanelType type)
        //{
        //    var anch = AnchorableToolsPanels.FirstOrDefault(x => x.ToolsPanelType == type);
        //    if (anch == null) return;
        //    HideShowAnchorable(anch, true);
        //    if (!anch.IsSelected)
        //        anch.IsSelected = true;
        //}
        private void ShowAnchorablePaneControl(ExtendedLayoutAnchorable pane)
        {
            var control = GetCurrentLayoutAnchorControl(pane);
            if (control == null) return;
            MethodInfo dynMethod = GetType().GetMethod("ShowAutoHideWindow", BindingFlags.NonPublic | BindingFlags.Instance);
            if (dynMethod != null) dynMethod.Invoke(this, new object[] { control });
        }

        #endregion

        #region Internal Fields

        #endregion

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            //_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            //_eventAggregator.GetEvent<ShowAnchorablePaneContent>().Subscribe(OnShowAnchorablePane);
            //_eventAggregator.GetEvent<PinToolsPanelAnchorablePane>().Subscribe(OnPinToolsPanelAnchorablePane);
            //_eventAggregator.GetEvent<ExecuteProfilerEvent>().Subscribe(OnExecuteProfiler);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                DeserializeAndLoad(new List<AnchorableType>() { AnchorableType.WorkSpace });
                IsAutoHiding = false;
            }));
        }
        public override bool CanForceDeserialization()
        {
            if (Layout != null && Layout.RootVersion != null)
                return MessageBox.Show(Layout.RootVersion.ErrorMessage, Layout.RootVersion.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK;
            return true;
        }
        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SerializeAndUnLoad();
        }
        public override void AnchorableTabItemPreviewMouseDown(ExtendedLayoutAnchorable anchorable, MouseButtonEventArgs e)
        {
            if (!anchorable.CanHide || !anchorable.IsAutoHidden) return;
            //var side = anchorable.GetSide();
            if (!anchorable.IsPinned)
            {
                anchorable.OnTabItemPreviewMouseDown();
                return;
            }
            anchorable.ToggleAutoHide();
            anchorable.IsSelected = true;
            e.Handled = true;
        }
        //public override int GetTabItemIndex(object tabItem)
        //{
        //    var layoutDocumentTabItem = tabItem as ILayoutDocumentTabItem;
        //    if (layoutDocumentTabItem == null) return -1;
        //    return layoutDocumentTabItem.Index;
        //}
        //public override void SetTabItemIndex(object tabItem, int index)
        //{
        //    var layoutDocumentTabItem = tabItem as ILayoutDocumentTabItem;
        //    if (layoutDocumentTabItem != null && index >= 0)
        //        layoutDocumentTabItem.Index = index;
        //}
        public override void PaneContentLoaded(ExtendedLayoutAnchorable anchorable, bool isLoaded)
        {
            if (anchorable == null) return;
            var side = anchorable.GetSide();
            if (isLoaded)
            {
                var content = anchorable.Content as FrameworkElement;
                if (side == AnchorSide.Bottom || side == AnchorSide.Top)
                    return;
                if (content != null && content.IsVisible && AutoHideWindow.Model != null)
                {
                    if (side == AnchorSide.Right)
                    {
                        if (anchorable.IsAutoHidden || RightSidePanel.Children.Count == 0)
                            RightSidePanel.MaxWidth = 0.1;
                    }
                    else if (side == AnchorSide.Left)
                    {
                        if (anchorable.IsAutoHidden || LeftSidePanel.Children.Count == 0)
                            LeftSidePanel.MaxWidth = 0.1;
                    }
                }
                //if (_eventAggregator != null) _eventAggregator.GetEvent<AnchorableVisibilityChangedEvent>().Publish(new AnchorableInfo(anchorable, true));
            }
            else
            {
                if (side == AnchorSide.Bottom) return;
                //if (_eventAggregator != null) _eventAggregator.GetEvent<AnchorableVisibilityChangedEvent>().Publish(new AnchorableInfo(anchorable, false));
                if (anchorable.IsAutoHidden)
                {
                    if (side == AnchorSide.Right)
                        RightSidePanel.MaxWidth = double.PositiveInfinity;
                    else if (side == AnchorSide.Left)
                        LeftSidePanel.MaxWidth = double.PositiveInfinity;
                }
            }
        }
        public override void PreviewToggleAutoHide(ExtendedLayoutAnchorable extendedLayoutAnchorable)
        {
            IsAutoHiding = true;
        }
        public override void ToggleAutoHided(ExtendedLayoutAnchorable extendedLayoutAnchorable)
        {
            Dispatcher.BeginInvoke(new Action(() => { IsAutoHiding = false; }), DispatcherPriority.Normal, null);
        }
        protected override string GetLayoutPath() { return Path.Combine(GetEsmTempPath, LayoutFileName); }
        protected override string GetDefaultLayoutPath() { return DefaultLayoutPath; }
        protected override void SetDefaultLayout()
        {
            base.SetDefaultLayout();
            MessageBox.Show("The layout will be reset after restart of the application.", "Switch to default layout", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        protected override void OnActiveContentChanged(DependencyPropertyChangedEventArgs e)
        {
            //var oldLayoutDocument = e.OldValue as ILayoutDocumentTabItem;
            //if (oldLayoutDocument != null)
            //    oldLayoutDocument.ToolsPanelChanged -= OnToolsPanelChanged;
            if (!IsLoaded)
            {
                base.OnActiveContentChanged(e);
                return;
            }
            var currentActive = e.NewValue as ContentControl;
        //    var newLayoutDocument = e.NewValue as ILayoutDocumentTabItem;
        //    if (newLayoutDocument != null)
        //       newLayoutDocument.ToolsPanelChanged += OnToolsPanelChanged;

        //    if (currentActive != null && (e.OldValue is TabBase) && !currentActive.IsVisible)
        //    {
        //        ActiveContent = e.OldValue;
        //    }
        //    else
        //    {
        //        base.OnActiveContentChanged(e);
        //        if (!IsFloating) UpdateCustomToolsPanelLayoutAnchorable(newLayoutDocument);
        //    }
        //    if (newLayoutDocument != null)
        //    {
        //        var anchorablePanes = Layout.Descendents().OfType<LayoutAnchorablePane>().ToList();
        //        foreach (var anchorablePane in anchorablePanes)
        //        {
        //            var selectedItem = anchorablePane.SelectedContent as ExtendedLayoutAnchorable;
        //            if (selectedItem == null || !selectedItem.IsLayoutVisible)
        //                PaneControlSelectionItemBehavior.SelectFirsVisibleChildren(anchorablePane);
        //        }
        //    }
        }
        //private void OnToolsPanelChanged(object sender, IToolsPanelViewModel e)
        //{
        //    UpdateCustomToolsPanelLayoutAnchorable(sender as ILayoutDocumentTabItem);
        //}
        //private void UpdateCustomToolsPanelLayoutAnchorable(ILayoutDocumentTabItem layoutDocument)
        //{
        //    if (layoutDocument == null) return;
        //    foreach (var extendedLayoutAnchorable in AnchorableToolsPanels)
        //    {
        //        if (extendedLayoutAnchorable.ToolsPanelType == AnchorableToolsPanelType.None) continue;
        //        UpdateCustomToolsPanelLayoutAnchorable(extendedLayoutAnchorable, layoutDocument.ToolsPanel, layoutDocument);
        //    }
        //}
        //private static void UpdateCustomToolsPanelLayoutAnchorable(CustomToolsPanelLayoutAnchorable layoutAnchorable, IToolsPanelViewModel toolsPanel, ILayoutDocumentTabItem layoutDocument)
        //{
        //    if (layoutAnchorable == null) return;
        //    var diffLayoutAnchorable = layoutAnchorable as CustomDiffToolsPanelLayoutAnchorable;
        //    var diffSpliter = layoutDocument as IDiffSplitterViewModel;
        //    layoutAnchorable.SetToolsPanel(toolsPanel);
        //    if (diffLayoutAnchorable != null)
        //        diffLayoutAnchorable.SetParentDiffSplitter(diffSpliter);
        //    var content = layoutAnchorable.Content as FrameworkElement;
        //    if (content != null)
        //        content.DataContext = layoutAnchorable;

        //    if (toolsPanel == null || toolsPanel.Parent == null)
        //    {
        //        layoutAnchorable.IsToolsPanelLayoutVisible = false;
        //    }
        //    else if (diffLayoutAnchorable != null)
        //    {
        //        layoutAnchorable.IsToolsPanelLayoutVisible = diffSpliter != null;
        //    }
        //    else
        //    {
        //        switch (toolsPanel.Parent.DesignerType)
        //        {
        //            case DesignerType.SqlQuery:
        //                layoutAnchorable.IsToolsPanelLayoutVisible = layoutAnchorable.ToolsPanelType != AnchorableToolsPanelType.Symbols;
        //                break;
        //            case DesignerType.None:
        //            case DesignerType.EvaluationResult:
        //            case DesignerType.StartPage:
        //            case DesignerType.SymbolExplorer:
        //                layoutAnchorable.IsToolsPanelLayoutVisible = false;
        //                break;
        //            default:
        //                layoutAnchorable.IsToolsPanelLayoutVisible = true;
        //                break;
        //        }
        //    }
        //}
        public ICommand AutoHideCommand { get; set; }
        public ICommand HideCommand { get; set; }

        private void OnAutoHideCommand(ExtendedLayoutAnchorable layoutAnchorable)
        {
            if (layoutAnchorable == null) return;
            var side = layoutAnchorable.GetSide();
            if (side == AnchorSide.Right)
            {
                RightSidePanel.MaxWidth = double.PositiveInfinity;
            }
            else if (side == AnchorSide.Left)
            {
                LeftSidePanel.MaxWidth = double.PositiveInfinity;
            }
            var layoutAnchorablePane = layoutAnchorable.Parent as LayoutAnchorablePane;
            var layoutAnchorableGroup = layoutAnchorable.Parent as LayoutAnchorGroup;
            if (layoutAnchorablePane != null)
            {
                foreach (var anchorable in layoutAnchorablePane.Children.Cast<ExtendedLayoutAnchorable>())
                    anchorable.IsPinned = false;
            }
            else if (layoutAnchorableGroup != null)
            {
                foreach (var anchorable in layoutAnchorableGroup.Children.Cast<ExtendedLayoutAnchorable>())
                    anchorable.IsPinned = true;
            }
            layoutAnchorable.ToggleAutoHide();
            layoutAnchorable.IsSelected = !layoutAnchorable.IsAutoHidden;
        }
        private void OnHideCommand(ExtendedLayoutAnchorable layoutAnchorable)
        {
            HideShowAnchorable(layoutAnchorable);
        }
        public void HideVisibleAndAutoHiddenPanes()
        {
            var activePanes = AnchorableChildPanes.Where(x => x.IsAutoHidden && ((FrameworkElement)x.Content).IsVisible).ToList();
            if (activePanes.Any())
            {
                var rightPanes = activePanes.Where(x => x.GetSide() == AnchorSide.Right).ToList();
                if (rightPanes.Any())
                {
                    HideCommand.Execute(rightPanes.FirstOrDefault());
                }
                var leftPanes = activePanes.Where(x => x.GetSide() == AnchorSide.Left).ToList();
                if (leftPanes.Any())
                {
                    HideCommand.Execute(leftPanes.FirstOrDefault());
                }
            }
        }
    }
}
