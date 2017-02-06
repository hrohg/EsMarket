using System.Windows;
using System.Windows.Controls;
using ES.Common.ViewModels.Base;

namespace ES.Common.Controls
{
    public class PanesStyleSelector : StyleSelector
    {
        public Style TabStyle
        {
            get;
            set;
        }

       public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is TabViewModel)
                return TabStyle;

            return base.SelectStyle(item, container);
        }
    }
}
