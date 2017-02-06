using System;


namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    public interface ILayoutGroup : ILayoutContainer
    {
        int IndexOfChild(ILayoutElement element);
        void InsertChildAt(int index, ILayoutElement element);
        void RemoveChildAt(int index);
        void ReplaceChildAt(int index, ILayoutElement element);
        event EventHandler ChildrenCollectionChanged;
    }
}
