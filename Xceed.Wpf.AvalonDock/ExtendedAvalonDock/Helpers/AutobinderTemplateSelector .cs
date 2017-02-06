using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Interfaces;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Helpers
{
    public class AutobinderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DocumentTemplate { get; set; }
        public DataTemplate ToolTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            //check if the item is an instance of TestViewModel
            if (item is IDockToolBar)
                return DocumentTemplate;
            else if (item is IDockDocument)
                return ToolTemplate;

            //delegate the call to base class
            return base.SelectTemplate(item, container);
        }
    }

    public class AutobinderTemplate : DataTemplate
    {

    }
}
