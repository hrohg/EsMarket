using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Helpers;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls
{
    public class ExtendedLayoutContentPresenter : ContentPresenter
    {
        private LayoutDocumentControl _layoutDocumentControl;

        public ExtendedLayoutContentPresenter()
        {
            Loaded += OnLoaded;
        }

        public static readonly DependencyProperty IsFloatingProperty = DependencyProperty.Register(
            "IsFloating", typeof(bool), typeof(ExtendedLayoutContentPresenter), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public bool IsFloating
        {
            get { return (bool) GetValue(IsFloatingProperty); }
            set { SetValue(IsFloatingProperty, value); }
        }
        public static readonly DependencyProperty CanFloatProperty = DependencyProperty.Register(
            "CanFloat", typeof (bool), typeof (ExtendedLayoutContentPresenter), new PropertyMetadata(true, OnCanFloatChanged));
        public bool CanFloat
        {
            get { return (bool) GetValue(CanFloatProperty); }
            set { SetValue(CanFloatProperty, value); }
        }
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _layoutDocumentControl = this.FindParent<LayoutDocumentControl>();
            if (_layoutDocumentControl == null) return;
            var model = _layoutDocumentControl.Model;
            if (model == null) return;
            model.CanFloat = CanFloat;
            if (IsFloating == model.IsFloating) return;
            SetValue(IsFloatingProperty, model.IsFloating);
        }
        private static void OnCanFloatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var customContentPresenter = d as ExtendedLayoutContentPresenter;
            if(customContentPresenter == null) return;
            var layoutDocument = customContentPresenter.DataContext as LayoutDocument;
            if (layoutDocument == null) return;
            layoutDocument.SetCurrentValue(CanFloatProperty, customContentPresenter.CanFloat);
        }
    }
}
