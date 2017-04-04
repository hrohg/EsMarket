using System;
using System.ComponentModel;
using System.Reflection;

namespace Shared.Helpers
{
    public class Enumerations
    {
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

    }
    public enum FiltersUsage
    {
        WithFilters,
        WithoutFilters
    }

    public enum PrintModeEnum
    {
        Normal,
        Small,
        Large
    }

    public enum ExportImportEnum
    {
        Xml, 
        Excel
    }
}
