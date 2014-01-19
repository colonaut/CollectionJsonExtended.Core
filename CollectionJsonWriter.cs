using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CollectionJsonExtended.Core
{
    //TODO implement settings for collection json writer and implement entity handling (only dataObject handling currentlly implemented)

    public interface IRepresentation
    {
        
    }

    public interface IRepresentation<TEntity> where TEntity : class, new()
    {
        [JsonIgnore]
        CollectionJsonSerializerSettings Settings { get; }
    }

    public enum As
    {
        Collection,
        Template,
        Error
    }

    public enum With
    {
        All,
        Template,
        Queries
    }


    //The abstract (initializes static stuff)
    public abstract class CollectionJsonWriter
    {
        //why is all the following defined in an absrtract? http://stackoverflow.com/a/9665168
        static readonly CollectionJsonSerializerSettings DefaultSerializerSettings;
        
        static CollectionJsonWriter() //this will be envoked at first usage and create the DefaultSettings
        {
            DefaultSerializerSettings = new CollectionJsonSerializerSettings
            {
                DataPropertyCasing = DataPropertyCasing.CamelCase,
                ConversionMethod = ConversionMethod.Entity
            };
        }

        protected CollectionJsonWriter()
        {
            Settings = DefaultSerializerSettings;
        }
        
        protected CollectionJsonWriter(CollectionJsonSerializerSettings settings)
        {
            Settings = settings ?? DefaultSerializerSettings;
        }


        [JsonIgnore]
        public CollectionJsonSerializerSettings Settings { get; private set; }
    }

    public sealed class CollectionJsonWriter<TEntity> : CollectionJsonWriter, IRepresentation<TEntity>
        where TEntity : class, new()
    {
        /* Ctor */
        public CollectionJsonWriter(HttpStatusCode httpStatusCode,
            string message = null)
        {
            if ((int)httpStatusCode < 400)
                throw new NotImplementedException("Currently only status code above/equal 400 supported (errors)");

            Error = new ErrorRepresentation(httpStatusCode, message);
        }

        public CollectionJsonWriter(TEntity entity,
            CollectionJsonSerializerSettings settings = null): base(settings)
        {
            Collection = new CollectionRepresentation<TEntity>(entity, Settings);
        }

        public CollectionJsonWriter(IEnumerable<TEntity> entities,
            CollectionJsonSerializerSettings settings = null) : base(settings)
        {
            Collection = new CollectionRepresentation<TEntity>(entities, Settings);
        }
        

        /* Properties */
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CollectionRepresentation<TEntity> Collection { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ErrorRepresentation Error { get; private set; }
    }


    /*****************
     * Representations
     *****************/


    public sealed class WriteTemplateRepresentation<TEntity> : IRepresentation<TEntity>
        where TEntity : class, new()
    {
        public WriteTemplateRepresentation(CollectionJsonSerializerSettings settings)
        {
            Settings = settings;
            ConversionMethod = settings.ConversionMethod;
        }

        //TODO: we might not have to care about the settings here. we might remove the passing of the settings to here, once this descision is settled
        //a template should always represent the complex data structure, as this only can transport prompts and what so ever.
        //if settings are on entity, the client must convert the template to an read template, which can then be read on entity base
        //but that is not neccessary! we would just send the template.. so we probably will have only one template, not a read and write one;
        
        [JsonConverter(typeof(StringEnumConverter))] //we might not need that...
        public ConversionMethod ConversionMethod;

        [JsonConverter(typeof(DataRepresentationConverter))]
        public object Data {
            get { return typeof(TEntity); }
        }

        public CollectionJsonSerializerSettings Settings { get; private set; }
    }


    public sealed class LinkRepresentation : IRepresentation
    {
        
        
        public string Rel { get; set; }
        
        public Uri Href { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Prompt { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public RenderType Render { get; set; }


        //TODO RenderType vs RenderType in client.. and this should be string?
        //DEPR, done in client??? or use this... adapt specs
        public enum RenderType
        {
            href,
            image
        }
    }

}
