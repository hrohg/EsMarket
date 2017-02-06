using System.Windows.Controls;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    public interface ILayoutOrientableGroup : ILayoutGroup
    {
        Orientation Orientation { get; set; }
    }
}
