using System;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using System.IO;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Commands;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Behaviors;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Helpers;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls
{
    public class ExtendedDockingManager : DockingManager
    {
        public override bool IsAutoHiding { get; set; }
        public override bool IsFloating { get; set; }
        public override bool IsMiddleBottomSideEnabled { get { return true; } }


        public ICommand AutoHideCommand { get; set; }
        public ICommand HideCommand { get; set; }

        static ExtendedDockingManager()
        {
            
        }
        
        public ExtendedDockingManager()
        {
            AutoHideCommand = new RelayCommand<ExtendedLayoutAnchorable>(OnAutoHideCommand);
            HideCommand = new RelayCommand<ExtendedLayoutAnchorable>(OnHideCommand);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        


        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                DeserializeAndLoad(new List<AnchorableType>() { AnchorableType.WorkSpace });
                IsAutoHiding = false;
            }));
        }
        public override bool CanForceDeserialization()
        {
            return Layout == null;
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SerializeAndUnLoad();
        }

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
