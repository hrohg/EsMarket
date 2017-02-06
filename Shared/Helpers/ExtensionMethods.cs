using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Shared.Helpers
{
	public static class ExtensionMethods
	{
		public static T FindVisualChild<T>(this UserControl userControl, DependencyObject obj) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child != null && child is T)
				{
					return (T)child;
				}

				T childOfChild = userControl.FindVisualChild<T>(child);
				if (childOfChild != null)
				{
					return childOfChild;
				}
			}
			return null;
		}

		public static bool IsEmpty(this DateTime dt)
		{
			return (dt == DateTime.MinValue);
		}
	}
}
