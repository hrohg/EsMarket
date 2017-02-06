using System.Windows;
using System.Windows.Controls;
using ES.Common.ViewModels.Base;
using Xceed.Wpf.AvalonDock.Layout;

namespace ES.Common.Controls
{
    public class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {

        }


        public DataTemplate TabViewTemplate
        {
            get;
            set;
        }
       

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is TabViewModel)
                return TabViewTemplate;
            return base.SelectTemplate(item, container);
        }
    }
}
