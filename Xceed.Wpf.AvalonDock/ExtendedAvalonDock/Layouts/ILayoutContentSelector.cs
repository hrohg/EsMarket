

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    public interface ILayoutContentSelector
    {
        int SelectedContentIndex { get; set; }

        int IndexOf(LayoutContent content);

        LayoutContent SelectedContent { get; }
    }
}
