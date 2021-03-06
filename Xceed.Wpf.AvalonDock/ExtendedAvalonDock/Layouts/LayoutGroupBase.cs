﻿using System;
using System.Xml.Serialization;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    [Serializable]
    public abstract class LayoutGroupBase : LayoutElement
    {
        [field: NonSerialized]
        [field: XmlIgnore]
        public event EventHandler ChildrenCollectionChanged;

        protected virtual void OnChildrenCollectionChanged()
        {
            if (ChildrenCollectionChanged != null)
                ChildrenCollectionChanged(this, EventArgs.Empty);
        }

        protected void NotifyChildrenTreeChanged(ChildrenTreeChange change)
        {
            OnChildrenTreeChanged(change);
            var parentGroup = Parent as LayoutGroupBase;
            if (parentGroup != null)
                parentGroup.NotifyChildrenTreeChanged(ChildrenTreeChange.TreeChanged);
        }

        [field: NonSerialized]
        [field: XmlIgnore]
        public event EventHandler<ChildrenTreeChangedEventArgs> ChildrenTreeChanged;

        protected virtual void OnChildrenTreeChanged(ChildrenTreeChange change)
        {
            if (ChildrenTreeChanged != null)
                ChildrenTreeChanged(this, new ChildrenTreeChangedEventArgs(change));
        }


    }
}
