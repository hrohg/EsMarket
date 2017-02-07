using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls
{
    public class ExtendedLayoutAnchorablePane : LayoutAnchorablePane
    {
        public ExtendedLayoutAnchorablePane()
        {
            DockMinWidth = 200;
            DockMinHeight = 150;
        }
    }
}
