using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ES.Business.Helpers
{
    public class DataTranslator
    {
        private static Dictionary<string, Dictionary<string, PropertyInfo>> propertyMap;
        private static object locker;

        static DataTranslator()
        {
            propertyMap = new Dictionary<string, Dictionary<string, PropertyInfo>>();
            locker = new object();
        }

        public static T Convert<T>(DataRow row)
        {
            Dictionary<string, PropertyInfo> propMap = GetPropertiesWithSettersMap<T>();
            return Convert<T>(row, propMap);
        }

        private static T Convert<T>(DataRow row, Dictionary<string, PropertyInfo> propMap)
        {
            T retval = Activator.CreateInstance<T>();
            Type tp = typeof(T);

            foreach (DataColumn column in row.Table.Columns)
            {
                PropertyInfo prop = null;
                if (propMap == null)
                    tp.GetProperty(column.ColumnName);
                else if (propMap.ContainsKey(column.ColumnName))
                    prop = propMap[column.ColumnName];
                if (prop == null)
                    continue;
                SetPropertyValue(retval, prop, row[column.ColumnName], column.DataType);
            }
            return retval;
        }
        public static DataTable ConvertListToDataTable<T>(List<T> list)
        {
            // New table.
            DataTable table = new DataTable();

            // Get max columns.
            int columns = 6;
            //foreach (var array in list)
            //{
            //    if (array > columns)
            //    {
            //        columns = array.Length;
            //    }
            //}

            // Add columns.
            for (int i = 0; i < columns; i++)
            {
                table.Columns.Add();
            }

            // Add rows.
            foreach (var array in list)
            {
                table.Rows.Add(array);
            }

            return table;
        }


      
        public static List<T> Convert<T>(DataTable table)
        {
            List<T> retval = new List<T>();
            if (table == null || table.Rows.Count == 0)
                return new List<T>();
            Type tp = typeof(T);
            Dictionary<string, PropertyInfo> propMap = GetPropertiesWithSettersMap<T>();
            foreach (DataRow row in table.Rows)
            {
                T tmpObj = Convert<T>(row, propMap);
                retval.Add(tmpObj);
            }
            return retval;
        }
        public static List<T> Convert<T>(IDataReader reader)
        {
            List<T> retval = new List<T>();
            Dictionary<string, PropertyInfo> propMap = GetPropertiesWithSettersMap<T>();
            while (reader.Read())
            {
                retval.Add(Convert<T>((IDataRecord)reader, propMap));
            }
            return retval;
        }

        private static T Convert<T>(IDataRecord record, Dictionary<string, PropertyInfo> propMap)
        {
            T retval = Activator.CreateInstance<T>();
            Type tp = typeof(T);
            for (int i = 0; i < record.FieldCount; i++)
            {
                PropertyInfo prop = null;
                string fieldName = record.GetName(i);
                if (propMap == null)
                    tp.GetProperty(fieldName);
                else if (propMap.ContainsKey(fieldName))
                    prop = propMap[fieldName];
                if (prop == null)
                    continue;
                SetPropertyValue(retval, prop, record[i], record.GetFieldType(i));
            }
            return retval;
        }

        public static void ClearCache()
        {
            lock (locker)
            {
                propertyMap.Clear();
            }
        }

        private static void SetPropertyValue(object propOwner, PropertyInfo prop, object value, Type valueType)
        {
            if (propOwner == null || value == null || value is DBNull || prop == null)
                return;
            if (valueType == null)
                valueType = value.GetType();
            if (prop.PropertyType.Equals(valueType) || prop.PropertyType.IsEnum)
                prop.SetValue(propOwner, value, null);
            else
            {
                if (prop.PropertyType.IsGenericType)
                {
                    prop.SetValue(propOwner, System.Convert.ChangeType(value, prop.PropertyType.GetGenericArguments()[0], CultureInfo.CurrentCulture), null);
                    if (prop.PropertyType.Equals(typeof(DateTime?)))
                    {
                        DateTime? tmpval = prop.GetValue(propOwner, null) as DateTime?;
                        if (tmpval.HasValue && tmpval.Value.Equals(DateTime.MinValue))
                            prop.SetValue(propOwner, null, null);
                    }

                }
                else
                {
                    object tovalue = System.Convert.ChangeType(value, prop.PropertyType, CultureInfo.CurrentCulture);
                    prop.SetValue(propOwner, tovalue, null);
                }
            }
        }

        private static Dictionary<string, PropertyInfo> GetPropertiesWithSettersMap<T>()
        {
            Type tp = typeof(T);
            Dictionary<string, PropertyInfo> retval;
            lock (locker)
            {
                if (propertyMap.TryGetValue(tp.FullName, out retval))
                    return retval;
            }
            PropertyInfo[] props = tp.GetProperties();
            retval = props.Where(s => s.CanWrite).ToDictionary(s => s.Name);
            lock (locker)
            {
                if (!propertyMap.ContainsKey(tp.FullName))
                    propertyMap.Add(tp.FullName, retval);
            }
            return retval;
        }
    }
}
