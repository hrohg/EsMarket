using System.Collections.ObjectModel;

using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    public interface ILayoutRoot
    {
        ExtendedDocingManager Manager { get; }

        LayoutPanel RootPanel { get; }

        LayoutAnchorSide TopSide { get; }
        LayoutAnchorSide LeftSide { get; }
        LayoutAnchorSide RightSide { get; }
        LayoutAnchorSide BottomSide { get; }

        LayoutContent ActiveContent { get; set; }

        void CollectGarbage();

        ObservableCollection<LayoutFloatingWindow> FloatingWindows { get; }
        ObservableCollection<LayoutAnchorable> Hidden { get; }
    }
}
