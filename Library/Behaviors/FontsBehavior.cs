using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace ResourceLibrary.Behaviors
{
    public enum DefinedFontSizeStyles { Small, Large }
    public sealed class FontsBehavior : DependencyObject, INotifyPropertyChanged
    {
        private static readonly FontsBehavior _instance = new FontsBehavior();
        private double _globalSmallFontSizes;
        private double _globalMiddleFontSizes;
        private double _globalLargeFontSizes;

        private FontsBehavior()
        {
            _globalSmallFontSizes = 10.0;
            _globalMiddleFontSizes = 11.0;
            _globalLargeFontSizes = 12.0;
        }

        public static FontsBehavior Instance { get { return _instance; } }

        public static readonly DependencyProperty DefinedFontSizeStylesProperty = DependencyProperty.Register(
            "DefinedFontSizeStyles", typeof(DefinedFontSizeStyles), typeof(FontsBehavior), new PropertyMetadata(default(DefinedFontSizeStyles)));
        public DefinedFontSizeStyles DefinedFontSizeStyles
        {
            get { return (DefinedFontSizeStyles)GetValue(DefinedFontSizeStylesProperty); }
            set { SetValue(DefinedFontSizeStylesProperty, value); }
        }

        public double GlobalSmallFontSizes
        {
            get { return _globalSmallFontSizes; }

            set
            {
                if (value.Equals(_globalSmallFontSizes)) return;
                _globalSmallFontSizes = value;
                NotifyPropertyChanged();
            }
        }
        public double GlobalMiddleFontSizes
        {
            get { return _globalMiddleFontSizes; }
            set
            {
                if (value.Equals(_globalMiddleFontSizes)) return;
                _globalMiddleFontSizes = value;
                NotifyPropertyChanged();
            }
        }
        public double GlobalLargeFontSizes
        {
            get { return _globalLargeFontSizes; }
            set
            {
                if (value.Equals(_globalLargeFontSizes)) return;
                _globalLargeFontSizes = value;
                NotifyPropertyChanged();
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;


        private void NotifyPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
