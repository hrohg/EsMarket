

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    public interface ILayoutPane : ILayoutContainer, ILayoutElementWithVisibility
    {
        void MoveChild(int oldIndex, int newIndex);

        void RemoveChildAt(int childIndex);
    }
}
