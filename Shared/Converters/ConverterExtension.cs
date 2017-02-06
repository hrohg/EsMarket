using System;
using System.Windows.Markup;
using System.Windows.Data;
using System.Collections.Generic;

namespace Shared.Converters
{
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public abstract class ConverterMarkupExtension<T> : MarkupExtension, IValueConverter where T : class, new()
    {
        private static T m_converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return m_converter ?? (m_converter = new T());
        }

        #region IValueConverter Members

        public abstract object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture);
        public abstract object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture);

        #endregion
    }

    public class ConvertToExtension: MarkupExtension
    {
        private string type;
        private static Dictionary<string, object> Converters;        

        static ConvertToExtension()
        {
            Converters = new Dictionary<string, object>();

			//Converters.Add("int", new StringToUniversalValueConverter<int>());
			//Converters.Add("long", new StringToUniversalValueConverter<long>());
			//Converters.Add("double", new StringToUniversalValueConverter<double>());
			//Converters.Add("float", new StringToUniversalValueConverter<float>());
			//Converters.Add("short", new StringToUniversalValueConverter<short>());
			//Converters.Add("decimal", new StringToUniversalValueConverter<decimal>());
            //Converters.Add("visibility", new BoolToVisibilityConverter());
            //Converters.Add("!visibility", new BoolToVisibilityInvertConverter());
			//Converters.Add("datetimeshort", new DateTimeToShortDateStringConverter());
			//Converters.Add("datetime", new DateTimeToStringConverter());
			//Converters.Add("datetimelong", new DateTimeToLongStringConverter());
        }

        public ConvertToExtension(string type)
        {
            this.type = (type ?? "").ToLower();            

            if (!Converters.ContainsKey(this.type))
            {
				Type t = System.Reflection.Assembly.GetExecutingAssembly().GetType("Shared.Converters." + type, false, true);
                if (t != null && (t.GetInterface("IValueConverter") != null || t.GetInterface("IMultiValueConverter") != null))
                {
                    Converters.Add(this.type, Activator.CreateInstance(t));
                    return;
                }

                throw new ArgumentException("Тип " + this.type + " не поддерживается! Добавте его в список.", "type");
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Converters.ContainsKey(type)) return Converters[type];

            throw new ArgumentException("Тип " + type.ToLower() + " не поддерживается! Добавте его в список.", "type");
        }
    }
}