using System;
using System.Collections;
using System.Collections.Generic;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Helpers
{
    internal static class Extentions
    {
        public static bool Contains(this IEnumerable collection, object item)
        {
            foreach (var o in collection)
                if (o == item)
                    return true;

            return false;
        }


        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T v in collection)
                action(v);
        }


        public static int IndexOf<T>(this T[] array, T value) where T : class
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == value)
                    return i;

            return -1;
        }

        public static TV GetValueOrDefault<TV>(this WeakReference wr)
        {
            if (wr == null || !wr.IsAlive)
                return default(TV);
            return (TV)wr.Target;
        }
    }
}
