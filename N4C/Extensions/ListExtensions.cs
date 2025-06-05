using N4C.Models;
using System.Data;
using System.Reflection;

namespace N4C.Extensions
{
    public static class ListExtensions
    {
        public static DataTable ConvertToDataTable<T>(this List<T> list, string culture = default) where T : class, new()
        {
            culture = string.IsNullOrWhiteSpace(culture) ? Defaults.TR : culture;
            DataTable dataTable = null;
            DataRow row;
            PropertyInfo propertyInfo;
            object propertyValue;
            List<string> propertyNames;
            List<string> displayNames;
            var properties = ObjectExtensions.GetProperties<T>().ToList();
            if (properties is not null && properties.Any())
            {
                propertyNames = properties.Select(p => p.Name).ToList();
                displayNames = properties.Select(p => p.DisplayName).ToList();
                dataTable = new DataTable();
                for (int i = 0; i < properties.Count; i++)
                {
                    propertyInfo = ObjectExtensions.GetPropertyInfo<T>(properties[i].Name);
                    displayNames[i] = displayNames[i].GetDisplayName(propertyInfo.Name, culture);
                    dataTable.Columns.Add(displayNames[i], Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                }
                if (list is not null && list.Any())
                {
                    foreach (var item in list)
                    {
                        row = dataTable.NewRow();
                        for (int i = 0; i < properties.Count; i++)
                        {
                            propertyValue = ObjectExtensions.GetPropertyInfo(propertyNames[i], item).GetValue(item);
                            row[displayNames[i]] = propertyValue ?? DBNull.Value;
                        }
                        dataTable.Rows.Add(row);
                    }
                }
            }
            return dataTable;
        }
    }
}
