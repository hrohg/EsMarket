using System;

namespace ES.Common.Interfaces
{
    public interface ILayoutDocumentTabItem
    {
        int Index { get; set; }
        bool IsFloating { get; set; }
        IToolsPanelViewModel ToolsPanel { get; set; }
        //event EventHandler<IToolsPanelViewModel> ToolsPanelChanged;
    }
}
