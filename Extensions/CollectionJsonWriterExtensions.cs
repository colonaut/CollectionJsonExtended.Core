using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CollectionJsonExtended.Core
{
    public static class CollectionJsonWriterExtensions
    {

        /*public extension methods*/
        public static string Serialize(this IRepresentation representation)
        {
            return SerializeObject(representation,
                new CollectionJsonSerializerSettings {ConversionMethod = ConversionMethod.Data});
        }

        public static string Serialize<TEntity>(this IRepresentation<TEntity> representation)
            where TEntity : class, new()
        {
            return SerializeObject(representation,
                representation.Settings);
        }


        /*private methods*/
        static string SerializeObject(object representation, CollectionJsonSerializerSettings settings)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            if (settings.DataPropertyCasing == DataPropertyCasing.CamelCase)
                jsonSerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver(); //BUT: camel casing is done globally in mvc project    

            return JsonConvert.SerializeObject(representation, settings.Formatting, jsonSerializerSettings);
        }
    }

}
