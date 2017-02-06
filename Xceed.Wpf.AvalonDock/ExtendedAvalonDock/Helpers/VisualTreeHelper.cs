using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Helpers
{
    public static class VisualTreeHelperEx
    {
        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;
            var parent = VisualTreeHelper.GetParent(child);
            return parent as T ?? FindParent<T>(parent);
        }

        public static T FindChild<T>(this DependencyObject parent, string childName = null, bool isFindInContentToo = false)
                where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            if (isFindInContentToo && childrenCount == 0 && parent is ContentControl)
            {
                foundChild = FindChild<T>(((ContentControl)parent).Content as DependencyObject, childName, true);
            }
            else
            {
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    // If the child is not of the request child type child
                    var childType = child as T;
                    if (childType == null)
                    {
                        // recursively drill down the tree
                        foundChild = FindChild<T>(child, childName, isFindInContentToo);

                        // If the child is found, break so we do not overwrite the found child.
                        if (foundChild != null) break;
                    }
                    else if (!String.IsNullOrEmpty(childName))
                    {
                        var frameworkElement = child as FrameworkElement;
                        // If the child's name is set for search
                        if (frameworkElement != null && frameworkElement.Name == childName)
                        {
                            // if the child's name is of the request name
                            foundChild = (T)child;
                            break;
                        }
                        // recursively drill down the tree
                        foundChild = FindChild<T>(child, childName, isFindInContentToo);

                        // If the child is found, break so we do not overwrite the found child.
                        if (foundChild != null) break;
                    }
                    else
                    {
                        // child element found.
                        foundChild = (T)child;
                        break;
                    }
                }
            }

            return foundChild;
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var children = child as T;
                if (children != null)
                    yield return children;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
}
