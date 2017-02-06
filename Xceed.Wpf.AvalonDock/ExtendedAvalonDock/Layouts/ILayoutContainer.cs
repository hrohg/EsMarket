using System.Collections.Generic;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    public interface ILayoutContainer : ILayoutElement
    {
        IEnumerable<ILayoutElement> Children { get; }
        void RemoveChild(ILayoutElement element);
        void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement);
        int ChildrenCount { get; }
    }
}
