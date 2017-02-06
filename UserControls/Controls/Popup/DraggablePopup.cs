using System.Windows;
using System.Windows.Controls.Primitives;
using Shared.Helpers;

namespace UserControls.Controls.Popup
{
    public class DraggablePopup:System.Windows.Controls.Primitives.Popup
    {
        private Thumb _thumb;
        private Thumb Thumb { get { return _thumb ?? (_thumb = Child.FindChild<Thumb>(null, true)); } }
        public DraggablePopup()
        {
            Loaded += OnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (Thumb == null) return;
            MouseLeftButtonDown += (s, e) =>
            {
                Thumb.RaiseEvent(e);
            };

            Thumb.DragDelta += OnDragDelta;

            Loaded -= OnLoaded;
        }
        private void OnDragDelta(object sender, DragDeltaEventArgs args)
        {
            HorizontalOffset += args.HorizontalChange;

            if (SystemParameters.WorkArea.Width < HorizontalOffset + this.PlacementRectangle.Width &&
                SystemParameters.WorkArea.Width > HorizontalOffset)
                HorizontalOffset = SystemParameters.WorkArea.Width + 10;
            VerticalOffset += args.VerticalChange;
        }
    }
}
