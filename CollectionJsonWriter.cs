using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using CollectionJsonExtended.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CollectionJsonExtended.Core
{
    //TODO implement settings for collection json writer and implement entity handling (only dataObject handling currentlly implemented)

    public interface IRepresentation
    {
        
    }

    public interface IRepresentation<TEntity> : IRepresentation where TEntity : class, new()
    {

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
        static protected readonly CollectionJsonSerializerSettings DefaultSerializerSettings;
        static CollectionJsonWriter() //this will be envoked at first usage and create the DefaultSettings
        {
            DefaultSerializerSettings = new CollectionJsonSerializerSettings
            {
                DataPropertyCasing = DataPropertyCasing.CamelCase,
                ConversionMethod = ConversionMethod.Entity
            };
        }

        /*Ctor*/
        public static string Serialize(IRepresentation representation)
        {
            return SerializeObject(representation);
        }
        public static string Serialize(IRepresentation representation, CollectionJsonSerializerSettings settings)
        {
            return SerializeObject(representation, settings);
        }

        public static string Serialize(IEnumerable<IRepresentation> representations)
        {
            return SerializeObject(representations);
        }
        public static string Serialize(IEnumerable<IRepresentation> representations, CollectionJsonSerializerSettings settings)
        {
            return SerializeObject(representations, settings);
        }

        static protected string SerializeObject(object representation, CollectionJsonSerializerSettings settings = null)
        {
            settings = settings ?? new CollectionJsonSerializerSettings{ConversionMethod = ConversionMethod.Data};

            var jsonSerializerSettings = new JsonSerializerSettings();
            if (settings.DataPropertyCasing == DataPropertyCasing.CamelCase)
                jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); //BUT: camel casing is done globally in mvc project    

            return JsonConvert.SerializeObject(representation, settings.Formatting, jsonSerializerSettings);
        }
    }

    public sealed class CollectionJsonWriter<TEntity> : CollectionJsonWriter, IRepresentation<TEntity> where TEntity : class, new()
    {
        readonly CollectionJsonSerializerSettings _settings = DefaultSerializerSettings;
        
        /* Ctor */
        public CollectionJsonWriter(HttpStatusCode httpStatusCode,
            string message = null)
        {
            if ((int)httpStatusCode < 400)
                throw new NotImplementedException("Currently only status code above/equal 400 supported (errors)");

            Error = new ErrorRepresentation(httpStatusCode, message);
        }

        public CollectionJsonWriter(TEntity entity,
            //IEnumerable<UrlInfo> urlInfoCollection,
            CollectionJsonSerializerSettings settings = null,
            As writer = As.Collection)
        {
            if (settings != null)
                _settings = settings;
            Collection = new CollectionRepresentation<TEntity>(entity, _settings);
        }

        public CollectionJsonWriter(IEnumerable<TEntity> entities,
            //IEnumerable<UrlInfo> urlInfoCollection,
            CollectionJsonSerializerSettings settings = null)
        {
            if (settings != null)
                _settings = settings;
            Collection = new CollectionRepresentation<TEntity>(entities, _settings);
        }
        

        /* Properties */
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CollectionRepresentation<TEntity> Collection { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ErrorRepresentation Error { get; set; }


        /* Methods */
        public string Serialize()
        {
            return SerializeObject(this, _settings);
        }

    }

    /*****************
     * Representations
     *****************/
    public sealed class CollectionRepresentation<TEntity> : IRepresentation<TEntity> where TEntity : class, new()
    {
        /* Private fields */
        string _version = "1.0";
        readonly IEnumerable<UrlInfoBase> _urlInfoCollection;

        /* Ctor */
        CollectionRepresentation()
        {
            _urlInfoCollection = SingletonFactory<UrlInfoCollection>.Instance
                .Find(typeof (TEntity));
        }
        
        public CollectionRepresentation(CollectionJsonSerializerSettings settings) : this() //collection representing a template //TODO: settings transportation
        {
            Template = new WriteTemplateRepresentation<TEntity>(settings);
            Links = new List<LinkRepresentation>();
        }

        public CollectionRepresentation(TEntity entity,
            CollectionJsonSerializerSettings settings) : this()
        {
            Items = new List<ItemRepresentation<TEntity>>
                    {
                        new ItemRepresentation<TEntity>(entity, _urlInfoCollection, settings)
                    };
        }

        public CollectionRepresentation(IEnumerable<TEntity> entities,
            CollectionJsonSerializerSettings settings) : this()
        {
            
            Items = new List<ItemRepresentation<TEntity>>(entities.Select(entity =>
                new ItemRepresentation<TEntity>(entity, _urlInfoCollection, settings)));
            
            Template = new WriteTemplateRepresentation<TEntity>(settings);
            
            Links = new List<LinkRepresentation>();
            
            Queries = _urlInfoCollection.Where(ui => ui.Kind == Is.Query)
                .Select(ui => new QueryRepresentation(ui))
                .ToList();
        }


        /* Properties */
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        public string Href
        {
            get
            {
                var baseUrlInfo = _urlInfoCollection.SingleOrDefault(ui => ui.Kind == Is.Base);
                if (baseUrlInfo != null)
                    return baseUrlInfo.VirtualPath;
                throw new Exception("must be single..."); //TODO exception
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<LinkRepresentation> Links { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<ItemRepresentation<TEntity>> Items { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<QueryRepresentation> Queries { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WriteTemplateRepresentation<TEntity> Template { get; set; }
   }


    public sealed class WriteTemplateRepresentation<TEntity> : IRepresentation<TEntity> where TEntity : class, new()
    {
       
        public WriteTemplateRepresentation(CollectionJsonSerializerSettings settings)
        {
            ConversionMethod = settings.ConversionMethod;
        }

        //TODO: we might not have to care about the settings here. we might remove the passing of the settings to here, once this descision is settled
        //a template should always represent the complex data structure, as this only can transport prompts and what so ever.
        //if ettings are on entity, the clint must convert the template to an read template, which can then be read on entity base
        
        [JsonConverter(typeof(StringEnumConverter))] //we might not need that...
        public ConversionMethod ConversionMethod;

        [JsonConverter(typeof(DataRepresentationConverter))]
        public object Data {
            get { return typeof(TEntity); }
        }

        //public IEnumerable<DataObject> Data { get { return this.MapFromEntityType(_propertyFormatter); } } 
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
