using System;

namespace ES.Common.Helpers
{
    public class ImageState
    {
        public Uri Selected { get; private set; }
        public Uri Normal { get; private set; }
        public Uri MouseOver { get; private set; }
        public Uri Pressed { get; private set; }
        public Uri InAccessible { get; private set; }

        public ImageState(Uri normal, Uri selected, Uri mouseOver)
        {
            Normal = normal;
            Selected = selected;
            MouseOver = mouseOver;
        }

        public ImageState(Uri normal, Uri selected, Uri mouseOver, Uri pressed)
            : this(normal, selected, mouseOver)
        {
            Pressed = pressed;
        }
        public ImageState(Uri normal, Uri selected, Uri mouseOver, Uri pressed, Uri inAccessible)
            : this(normal, selected, mouseOver, pressed)
        {
            InAccessible = inAccessible;
        }
    }
}
