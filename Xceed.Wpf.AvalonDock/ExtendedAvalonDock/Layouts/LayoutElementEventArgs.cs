using System;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    public class LayoutElementEventArgs : EventArgs
    {
        public LayoutElementEventArgs(LayoutElement element)
        {
            Element = element;
        }


        public LayoutElement Element
        {
            get;
            private set;
        }
    }
}
