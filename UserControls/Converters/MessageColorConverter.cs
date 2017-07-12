using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ES.Business.Models;
using ES.Common.Enumerations;

namespace UserControls.Converters
{
    public class MessageTypeToColorConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (targetType != typeof(MessageTypeEnum))
            { return Color.Black;}
            var type = (MessageTypeEnum)value;
            
            switch (type)
            {
                case MessageTypeEnum.Information:
                    return Color.Blue;
                    break;
                case MessageTypeEnum.Success:
                    return Color.Green;
                    break;
                case MessageTypeEnum.Error:
                    return Color.Red;
                    break;
                case MessageTypeEnum.Warning:
                    return Color.Purple;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            throw new InvalidOperationException("Converter can only convert to value of type Visibility.");
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new Exception("Invalid call - one way only");
        }
    }
}
