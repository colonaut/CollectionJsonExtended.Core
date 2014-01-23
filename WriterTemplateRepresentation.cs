﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


//TODO: could be one tempate? compare with read template...

namespace CollectionJsonExtended.Core
{
    public sealed class WriterTemplateRepresentation<TEntity> : RepresentationBase, IRepresentation<TEntity>
        where TEntity : class, new()
    {
        public WriterTemplateRepresentation(CollectionJsonSerializerSettings settings)
            : base(settings)
        {
            ConversionMethod = settings.ConversionMethod;
        }

        //TODO: we might not have to care about the settings here. we might remove the passing of the settings to here, once this descision is settled
        //a template should always represent the complex data structure, as this only can transport prompts and what so ever.
        //if settings are on entity, the client must convert the template to an read template, which can then be read on entity base
        //but that is not neccessary! we would just send the template.. so we probably will have only one template, not a read and write one;
        //We need the settings for private property serialization and primary key property serialization here!

        [JsonConverter(typeof(StringEnumConverter))] //we might not need that...
        public ConversionMethod ConversionMethod;

        [JsonConverter(typeof(DataRepresentationConverter))]
        public object Data {
            get { return typeof(TEntity); }
        }
    }
}