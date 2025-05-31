using N4C.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace N4C.Extensions
{
    public static class ObjectExtensions
    {
        public static PropertyInfo GetPropertyInfo<T>(string propertyName, T instance = default) where T : class, new()
        {
            return instance is null ? typeof(T).GetProperty(propertyName) : instance.GetType().GetProperty(propertyName);
        }

        public static List<PropertyInfo> GetPropertyInfo<T>(T instance = default) where T : class, new()
        {
            return instance is null ? typeof(T).GetProperties().ToList() : instance.GetType().GetProperties().ToList();
        }

        public static Property GetProperty<T>(string propertyName, bool includeExcelIgnored = true, T instance = default) where T : class, new()
        {
            var displayName = string.Empty;
            bool excelIgnoreAttribute = false;
            var propertyInfo = GetPropertyInfo(propertyName, instance);
            if (propertyInfo is not null)
            {
                var customAttributes = propertyInfo.GetCustomAttributes().ToList();
                if (customAttributes is not null)
                {
                    excelIgnoreAttribute = customAttributes.Any(customAttribute => customAttribute.GetType() == typeof(Attributes.ExcelIgnoreAttribute));
                    if (!excelIgnoreAttribute || includeExcelIgnored)
                    {
                        foreach (var customAttribute in customAttributes)
                        {
                            if (customAttribute.GetType() == typeof(Attributes.DisplayNameAttribute))
                            {
                                displayName = ((Attributes.DisplayNameAttribute)customAttribute).DisplayName;
                                break;
                            }
                            if (customAttribute.GetType() == typeof(System.ComponentModel.DisplayNameAttribute))
                            {
                                displayName = ((System.ComponentModel.DisplayNameAttribute)customAttribute).DisplayName;
                                break;
                            }
                        }
                    }
                }
            }
            if (propertyInfo is null || (excelIgnoreAttribute && !includeExcelIgnored))
                return null;
            return new Property(propertyInfo.Name, instance is not null ? propertyInfo.GetValue(instance) : null, displayName);
        }

        public static Property GetProperty<T>(this Expression<Func<T, object>> expression) where T : class, new()
        {
            var memberExpression = expression.Body is UnaryExpression unaryExpression ? (MemberExpression)unaryExpression.Operand : (MemberExpression)expression.Body;
            return GetProperty<T>(memberExpression.Member.Name);
        }

        public static List<Property> GetProperties<T>(bool includeExcelIgnored = false) where T : class, new()
        {
            List<Property> properties = null;
            Property property;
            var propertyInfoList = GetPropertyInfo<T>();
            if (propertyInfoList is not null && propertyInfoList.Any())
            {
                properties = new List<Property>();
                foreach (var propertyInfoItem in propertyInfoList)
                {
                    property = GetProperty<T>(propertyInfoItem.Name, includeExcelIgnored);
                    if (property is not null)
                        properties.Add(property);

                }
            }
            return properties;
        }

        public static T Trim<T>(this T instance) where T : class, new()
        {
            if (instance is null)
                return null;
            var properties = GetPropertyInfo(instance).Where(property => property.PropertyType == typeof(string)).ToList();
            object value;
            if (properties is not null)
            {
                foreach (var property in properties)
                {
                    value = property.GetValue(instance);
                    if (value is not null)
                        property.SetValue(instance, ((string)value).Trim());
                }
            }
            return instance;
        }
    }
}
