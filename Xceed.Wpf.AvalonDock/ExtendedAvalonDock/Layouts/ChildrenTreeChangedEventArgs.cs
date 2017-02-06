using System;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    public enum ChildrenTreeChange
    {
        /// <summary>
        /// Direct insert/remove operation has been perfomed to the group
        /// </summary>
        DirectChildrenChanged,

        /// <summary>
        /// An element below in the hierarchy as been added/removed
        /// </summary>
        TreeChanged
    }

    public class ChildrenTreeChangedEventArgs : EventArgs
    {
        public ChildrenTreeChangedEventArgs(ChildrenTreeChange change)
        {
            Change = change;
        }

        public ChildrenTreeChange Change { get; private set; }
    }
}
