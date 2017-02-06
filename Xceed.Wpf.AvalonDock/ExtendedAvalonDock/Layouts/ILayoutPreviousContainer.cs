namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    interface ILayoutPreviousContainer
    {
        ILayoutContainer PreviousContainer { get; set; }

        string PreviousContainerId { get; set; }
    }
}
