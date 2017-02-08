//========================================================================
//
// Author: Voitsekhovskiy Fyodor
// Date: 30.04.2009
// Description: Initial Draft
//
//========================================================================

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CustomControls.ValidPanel
{
	/// <summary>
	/// Контрол на базе StackPanel, дает возможность определить имеются ли ошибки валидации в контролах 
	/// расположенных внутри
	/// 
	/// Пример:
	/// 
	/// <ValidPanel:ValidPanel Name="validator">
	///     <TextBox Text="{Binding Path=SomeObject, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}"
	///     <Button x:Name="btSave" IsEnabled="{Binding ElementName=validator, Path=IsValid}"/>
	/// </ValidPanel:ValidPanel>
	/// 
	/// SomeObject должен реализовывать интерфейсы INotifyPropertyChanged и IDataErrorInfo
	/// Для корректной работы, контрол котрый будет проходить валидацию, должен иметь стиль содержащий:
	/// 
	///    <Setter Property="Validation.ErrorTemplate" Value="{StaticResource ValidBorderErrorTemplate}" />
	///    <Style.Triggers>
	///        <Trigger Property="Validation.HasError" Value="true">
	///            <Setter Property="ToolTip"
	///                    Value="{Binding RelativeSource={RelativeSource Self}, Path=(Validation.Errors)[0].ErrorContent}" />
	///        </Trigger>
	///    </Style.Triggers>
	/// 
	/// </summary>
	public class ValidPanel : StackPanel
	{
		private ValidPanel parentPanel;
		public ObservableCollection<ValidPanel> ChildPanels = new ObservableCollection<ValidPanel>();
		public event EventHandler AfterValidationStateChanged;

		public ValidPanel()
		{
			IsValid = true;
			Loaded += BoValidLoaded;
		}

		public bool HasErrors
		{
			get { return (bool)GetValue(HasErrorsProperty); }
			set { SetValue(HasErrorsProperty, value); }
		}

		public static readonly DependencyProperty HasErrorsProperty = DependencyProperty.Register("HasErrors", typeof(bool), typeof(ValidPanel));

		public bool HasSubPanelErrors
		{
			get { return (bool)GetValue(HasSubPanelErrorsProperty); }
			set
			{
				HasErrors = ErrorCount != 0 || value;
				IsValid = !HasErrors;

				if (parentPanel != null) parentPanel.CheckSubPanels();

				SetValue(HasSubPanelErrorsProperty, value);
			}
		}

		public void CheckSubPanels()
		{
			bool result = false;
			foreach (ValidPanel panel in ChildPanels)
			{
				if (panel.HasErrors) result = true;
			}
			HasSubPanelErrors = result;
		}

		public static readonly DependencyProperty HasSubPanelErrorsProperty = DependencyProperty.Register("HasSubPanelErrors", typeof(bool), typeof(ValidPanel));

		public bool IsValid
		{
			get { return (bool)GetValue(IsValidProperty); }
			set { SetValue(IsValidProperty, value); }
		}

		public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register("IsValid", typeof(bool), typeof(ValidPanel), new PropertyMetadata(true));

		public bool LocalValidation
		{
			get { return (bool)GetValue(LocalValidationProperty); }
			set { SetValue(LocalValidationProperty, value); }
		}

		public static readonly DependencyProperty LocalValidationProperty = DependencyProperty.Register("LocalValidation", typeof(bool), typeof(ValidPanel), new PropertyMetadata(false));

		/// <summary>
		/// Количество ошибок в этой панели
		/// </summary>
		public int ErrorCount
		{
			get { return (int)GetValue(ErrorCountProperty); }
			set
			{
				bool oldHasErrorsValue = HasErrors;
				HasErrors = value != 0 || HasSubPanelErrors;
				IsValid = !HasErrors;
				if (oldHasErrorsValue != HasErrors)
				{
					if (AfterValidationStateChanged != null) AfterValidationStateChanged(this, new EventArgs());
				}

				if (parentPanel != null) parentPanel.CheckSubPanels();

				SetValue(ErrorCountProperty, value);
			}
		}

		public static readonly DependencyProperty ErrorCountProperty = DependencyProperty.Register("ErrorCount", typeof(int), typeof(ValidPanel));

		public static void SetUpdateBE(UIElement element, bool? value)
		{
			element.SetValue(UpdateBEProperty, value);
		}

		public static bool? GetUpdateBE(UIElement element)
		{
			return (bool?)element.GetValue(UpdateBEProperty);
		}

		public static readonly DependencyProperty UpdateBEProperty = DependencyProperty.RegisterAttached("UpdateBE",
		                                                                                                 typeof(bool?), typeof(ValidPanel));

		void BoValidLoaded(object sender, RoutedEventArgs e)
		{
			if (!LocalValidation)
			{
				parentPanel = (ValidPanel)GetParentPanel(this);
				if (parentPanel != null) parentPanel.ChildPanels.Add(this);
			}
			UpdateBE(this);
			if (AfterValidationStateChanged != null) AfterValidationStateChanged(this, new EventArgs());
		}

		private DependencyObject GetParentPanel(DependencyObject child)
		{
			var obj = VisualTreeHelper.GetParent(child);

			if (obj == null) return null;

			if (obj is ValidPanel && obj != this) return obj;

			return GetParentPanel(obj);
		}

		private static void UpdateBE(FrameworkElement el)
		{
			if (el.GetValue(UpdateBEProperty) != null && (bool)el.GetValue(UpdateBEProperty))
			{
				if (el.IsVisible)
				{
					UpdateSource(el);
				}
				else 
				{
					el.IsVisibleChanged += ElIsVisibleChanged;
				}
			}
			if (el is Panel)
			{
				Panel panel = (Panel)el;
				for (int i = 0; i < panel.Children.Count; i++)
				{
					if (panel.Children[i] is FrameworkElement)
						UpdateBE((FrameworkElement)panel.Children[i]);
				}
			}
			if (el is ContentControl)
			{
				ContentControl cc = (ContentControl)el;
				if (cc.Content is FrameworkElement)
					UpdateBE((FrameworkElement)cc.Content);
			}
			if (el is Decorator)
			{
				Decorator dec = (Decorator)el;
				if (dec.Child is FrameworkElement)
					UpdateBE((FrameworkElement)dec.Child);
			}
		}

		static void ElIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			UpdateSource((FrameworkElement)sender);
		}

		private static void UpdateSource(FrameworkElement el)
		{
			if (el.IsVisible)
			{
				LocalValueEnumerator lve = el.GetLocalValueEnumerator();
				while (lve.MoveNext())
				{
					DependencyProperty dp = lve.Current.Property;
					if (dp.ReadOnly) continue; 

					BindingExpression expr = el.GetBindingExpression(dp);
					if (expr != null) expr.UpdateSource(); 
				}
				el.IsVisibleChanged -= ElIsVisibleChanged;
			}
		}

		public void AddError()
		{
			ErrorCount++;
		}

		public void DelError()
		{
			ErrorCount--;
		}

		public override string ToString()
		{
			return string.Format("Name: {0}; ErrorCount: {1}", Name, ErrorCount);
		}
	}
}