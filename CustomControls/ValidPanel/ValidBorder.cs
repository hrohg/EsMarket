using System.Windows;
using System.Windows.Controls;

namespace CustomControls.ValidPanel
{
	public class ValidBorder : Border
	{
		private ValidPanel parentPanel;
		private bool needDecrementErrorsCountOnUnloaded = true;

		public ValidBorder()
		{
			Loaded += ValidBorderLoaded;
			Unloaded += ValidBorderUnloaded;
		}

		void ValidBorderUnloaded(object sender, RoutedEventArgs e)
		{
			if (needDecrementErrorsCountOnUnloaded)
			{
				if (parentPanel != null)
				{
					parentPanel.ErrorCount--;
					needDecrementErrorsCountOnUnloaded = false;
				}
			}
		}

		void ValidBorderLoaded(object sender, RoutedEventArgs e)
		{
			SetErrorState();
		}

		private void SetErrorState()
		{
			AdornedElementPlaceholder aep = AdvTreeHelper.GetVisualChildElement<AdornedElementPlaceholder>(this);
			UIElement ctrl = (aep ?? new AdornedElementPlaceholder()).AdornedElement;
            
			if (ctrl != null && ctrl.IsVisible)
			{
				ValidPanel panel = AdvTreeHelper.GetVisualParentElement<ValidPanel>(ctrl);

				if (panel != null)
				{
					parentPanel = panel;
					parentPanel.ErrorCount++;
				}
			}
			IsVisibleChanged += CtrlIsVisibleChanged;
		}

		void CtrlIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if ((bool) e.NewValue)
			{
				if (parentPanel == null)
				{
					AdornedElementPlaceholder aep = AdvTreeHelper.GetVisualChildElement<AdornedElementPlaceholder>(this);
					UIElement ctrl = (aep ?? new AdornedElementPlaceholder()).AdornedElement;
					if (ctrl != null)
					{
						ValidPanel panel = AdvTreeHelper.GetVisualParentElement<ValidPanel>(ctrl);

						if (panel != null)
						{
							parentPanel = panel;
						}
					}
				}
				if (parentPanel != null) parentPanel.ErrorCount++;
			}
			else 
			{
				if (parentPanel == null)
				{
					AdornedElementPlaceholder aep = AdvTreeHelper.GetVisualChildElement<AdornedElementPlaceholder>(this);
					UIElement ctrl = (aep ?? new AdornedElementPlaceholder()).AdornedElement;
					if (ctrl != null)
					{
						ValidPanel panel = AdvTreeHelper.GetVisualParentElement<ValidPanel>(ctrl);

						if (panel != null)
						{
							parentPanel = panel;
							parentPanel.ErrorCount++;
						}
					}
				}
				if (parentPanel != null)
				{
					parentPanel.ErrorCount--;
					needDecrementErrorsCountOnUnloaded = false;
				}
			}
		}

	}
}