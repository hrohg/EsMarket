using System.ComponentModel;


namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    public interface ILayoutElement : INotifyPropertyChanged, INotifyPropertyChanging
    {
        ILayoutContainer Parent { get; }
        ILayoutRoot Root { get; }
    }
}
