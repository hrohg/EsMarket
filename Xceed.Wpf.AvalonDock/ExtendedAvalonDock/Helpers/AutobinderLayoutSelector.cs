using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Interfaces;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Helpers
{
    public class AutobinderLayoutSelector : StyleSelector
    {
        public Style DocumentStyle { get; set; }
        public Style ToolStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            //check if the item is an instance of TestViewModel
            if (item is IDockToolBar)
                return DocumentStyle;
            else if (item is IDockDocument)
                return ToolStyle;

            //delegate the call to base class
            return base.SelectStyle(item, container);
        }
    }
}
