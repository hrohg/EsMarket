using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Interfaces;
using Xceed.Wpf.AvalonDock.Layout;

namespace ES.Common.ViewModels.Base
{
    public class ToolsViewModel : PaneViewModel, IExtendedAnchorableBase
    {
        #region External properties
        public AnchorSide AnchorSide { get; set; }
        #endregion External properties
    }
}
