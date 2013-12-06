using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        internal static IList<Type> GetCollectionJsonConcreteTypes(this PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;
            if (!propertyType.IsAbstract && !propertyType.IsInterface)
                return new List<Type> {propertyType};
            return propertyInfo.GetCustomAttributes<CollectionJsonConcreteTypeAttribute>(false)
                               .Select(a => a.Type)
                               .OrderBy(t => t.Name)
                               .ToList();
        }

        internal static Type GetCollectionJsonConcreteTypeByName(this PropertyInfo propertyInfo, string destinationTypeName)
        {
            return propertyInfo.GetCollectionJsonConcreteTypes()
                            .SingleOrDefault(t => t.Name.ToLowerInvariant() == destinationTypeName.ToLowerInvariant());
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