﻿using System;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts
{
    [Flags]
    public enum AnchorableShowStrategy : uint
    {
        Most  = 0x0001,
        Left  = 0x0002,
        Right = 0x0004,
        Top   = 0x0010,
        Bottom= 0x0020,
    }
}
