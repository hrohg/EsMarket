using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Interfaces
{
    public interface IExtendedLayoutUpdateStrategy : ILayoutUpdateStrategy
    {
        bool BeforeInsertAnchorable(
            LayoutRoot layout,
            ExtendedLayoutAnchorable anchorableToShow,
            ILayoutContainer destinationContainer);
    }
}