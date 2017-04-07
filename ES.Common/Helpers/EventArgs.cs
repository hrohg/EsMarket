using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ES.Common.Helpers
{
    public class ActivityChangedEventArgs
    {
        public bool NewValue { get; private set; }
        public bool OldValue { get; private set; }
        public bool Value { get { return NewValue; } }
        public ActivityChangedEventArgs(bool newValue, bool oldValue)
        {
            NewValue = newValue;
            OldValue = oldValue;
        }
        public ActivityChangedEventArgs(bool value):this(value, !value)
        {

        }
    }
}
