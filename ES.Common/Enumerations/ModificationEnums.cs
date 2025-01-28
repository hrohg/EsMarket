using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ES.Common.Enumerations
{
    [Flags]
    public enum ModificationTypeEnum : short
    {
        Added = 1,
        Modified = 2,
        Removed = 4,
        ReCreated = 8,
    }
}
