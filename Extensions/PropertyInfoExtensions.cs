using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CollectionJsonExtended.Core.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CollectionJsonExtended.Core.Extensions
{
    internal static class PropertyInfoExtensions
    {
        
        internal static bool TrySetValue(this PropertyInfo propertyInfo, object obj, object value)
        {
            try
            {
                propertyInfo.SetValue(obj, value);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        internal static object GetDefaultValue(this PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;
            var isNullableType = Nullable.GetUnderlyingType(propertyType) != null;
            return isNullableType ? null : Activator.CreateInstance(propertyType);
        }

        internal static string GetCollectionJsonPrompt(this PropertyInfo propertyInfo)
        {
            var prompt = propertyInfo.GetCustomAttributes<CollectionJsonPropertyAttribute>(false)
                        .Select(a => a.Prompt)
                        .SingleOrDefault();
            return prompt ?? propertyInfo.Name.ToDisplayName();
        }
        
        internal static bool IsJsonStringEnumConverter(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes<JsonConverterAttribute>(false)
                            .Any(a => a.ConverterType == typeof (StringEnumConverter));
        }

        
    }
}