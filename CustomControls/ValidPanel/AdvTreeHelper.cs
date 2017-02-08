//========================================================================
//
// Author: Voitsekhovskiy Fyodor
// Date: 11.08.2009   
//
//========================================================================
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CustomControls.ValidPanel
{
	/// <summary>
	/// Операции с LogicalTreeHelper и VisualTreeHelper
	/// </summary>
	public static class  AdvTreeHelper
	{
		/// <summary>
		/// получить логический дочерний элемент по типу
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="el"></param>
		/// <returns></returns>
		public static T GetLogicalChildElement<T>(DependencyObject el)
		{
			if (el == null) return default(T);
			foreach (object obj in LogicalTreeHelper.GetChildren(el))
			{
				if (obj == null || !(obj is DependencyObject) ) continue;

				if (obj is T)
				{
					return (T)obj;
				}
				object result = GetLogicalChildElement<T>((DependencyObject) obj);
				if (result is T)
				{
					return (T)result;
				}
			}
			return default(T);
		}
        
		/// <summary>
		/// получить дочерний визуальный элемент по типу
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="el"></param>
		/// <returns></returns>
		public static T GetVisualChildElement<T>(DependencyObject el)
		{
			if (el == null) return default(T);
			int count = VisualTreeHelper.GetChildrenCount(el);
			for (int i = 0; i < count; i++)
			{
				DependencyObject obj = VisualTreeHelper.GetChild(el, i);

				if (obj == null) continue;

				if (obj is T)
				{
					return (T)(object)obj;
				}
				object result = GetVisualChildElement<T>(obj);
				if (result is T)
				{
					return (T)result;
				}
			}
			return default(T);
		}

		/// <summary>
		/// получить логический родительский элемент по типу
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="el"></param>
		/// <returns></returns>
		public static T GetLogicalParentElement<T>(DependencyObject el)
		{
			if (el == null) return default(T);
			DependencyObject obj = LogicalTreeHelper.GetParent(el);

			if (obj == null) return default(T);

			if (obj is T) return (T)(object)obj;

			return GetLogicalParentElement<T>(obj);
		}
        
		/// <summary>
		/// получить визуальный родительский элемент по типу
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="el"></param>
		/// <returns></returns>
		public static T GetVisualParentElement<T>(DependencyObject el)
		{
			if (el == null) return default(T);
			DependencyObject obj = VisualTreeHelper.GetParent(el);

			if (obj == null) return default(T);

			if (obj is T) return (T)(object)obj;

			return GetVisualParentElement<T>(obj);
		}

		/// <summary>
		/// получить дочерний визуальный элемент способный принять фокус
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="el"></param>
		/// <returns></returns>
		public static DependencyObject GetVisualFocusableChildElement(DependencyObject el)
		{
			if (el == null) return null;
			int count = VisualTreeHelper.GetChildrenCount(el);
			for (int i = 0; i < count; i++)
			{
				DependencyObject obj = VisualTreeHelper.GetChild(el, i);

				if (obj == null) continue;

				if (obj.GetValue(UIElement.FocusableProperty) != null && (bool) obj.GetValue(UIElement.FocusableProperty))
				{
					if (((UIElement)obj).IsVisible && ((UIElement)obj).IsEnabled) return obj;
				}
				DependencyObject result = GetVisualFocusableChildElement(obj);
				if (result == null) continue;
				if (result.GetValue(UIElement.FocusableProperty) != null && (bool)result.GetValue(UIElement.FocusableProperty))
				{
					if (((UIElement)result).IsVisible && ((UIElement)result).IsEnabled) return result;
				}
			}
			return null;
		}

		private static void GetVisualFocusableChildsRealisation(DependencyObject el, ICollection<IInputElement> list)
		{
			if (el == null) return;
			int count = VisualTreeHelper.GetChildrenCount(el);
			for (int i = 0; i < count; i++)
			{
				DependencyObject obj = VisualTreeHelper.GetChild(el, i);

				if (obj == null) continue;

				if (obj.GetValue(UIElement.FocusableProperty) != null && (bool)obj.GetValue(UIElement.FocusableProperty))
				{
					if (((UIElement)obj).IsVisible && ((UIElement)obj).IsEnabled)
					{
						list.Add((IInputElement) obj);
					}
				}
				GetVisualFocusableChildsRealisation(obj, list);
			}
			return;
		}

		/// <summary>
		/// получить коллекцию дочерних визуальных элементов способный принять фокус
		/// </summary>
		/// <param name="el"></param>
		/// <returns></returns>
		public static List<IInputElement> GetVisualFocusableChilds(DependencyObject el)
		{
			List<IInputElement> list = new List<IInputElement>();
			GetVisualFocusableChildsRealisation(el, list);
			return list;
		}
	}
}