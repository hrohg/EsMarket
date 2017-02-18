using System;
using System.Linq;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Interfaces;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Helpers
{
    public class LayoutUpdateStrategy : ILayoutUpdateStrategy
    {
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        { }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        { }

        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            //AD wants to add the anchorable into destinationContainer
            //just for test provide a new anchorablepane 
            //if the pane is floating let the manager go ahead
            LayoutAnchorablePane destPane = destinationContainer as LayoutAnchorablePane;
            if (destinationContainer != null &&
                destinationContainer.FindParent<LayoutFloatingWindow>() != null)
                return false;

            var vm = anchorableToShow.Content as IExtendedAnchorableBase;
            if (vm == null) return false;
            AnchorableShowStrategy showSide;
            
            switch (vm.AnchorSide)
            {
                case AnchorSide.Top:
                    showSide = AnchorableShowStrategy.Top;
                    break;
                case AnchorSide.Left:
                    showSide = AnchorableShowStrategy.Left;
                    anchorableToShow.AutoHideWidth = 250;
                    anchorableToShow.FloatingWidth = 250;
                    
                    break;
                case AnchorSide.Bottom:
                    showSide = AnchorableShowStrategy.Bottom;
                    break;
                case AnchorSide.Right: 
                              default:
                    showSide = AnchorableShowStrategy.Right;
                    break;
                    
            }
            
            anchorableToShow.AddToLayout(layout.Manager, showSide);
            return true;
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }
    }
}
