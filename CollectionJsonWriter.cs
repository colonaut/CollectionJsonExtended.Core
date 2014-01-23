using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;

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

    //The abstract (initializes static stuff and settings property for instances)
    public abstract class RepresentationBase
    {
        //why is all the following defined in an absrtract? http://stackoverflow.com/a/9665168
        static readonly CollectionJsonSerializerSettings DefaultSerializerSettings;
        
        static RepresentationBase() //this will be envoked at first usage and create the DefaultSettings
        {
            DefaultSerializerSettings = new CollectionJsonSerializerSettings
            {
                DataPropertyCasing = DataPropertyCasing.CamelCase,
                ConversionMethod = ConversionMethod.Entity
            };
        }

        protected RepresentationBase()
        {
            Settings = DefaultSerializerSettings;
        }
        
        protected RepresentationBase(CollectionJsonSerializerSettings settings)
        {
            Settings = settings ?? DefaultSerializerSettings;
        }


        [JsonIgnore]
        public CollectionJsonSerializerSettings Settings { get; private set; }
    }

    public sealed class CollectionJsonWriter<TEntity> : RepresentationBase, IRepresentation<TEntity>
        where TEntity : class, new()
    {
        /* Ctor */

        public CollectionJsonWriter(CollectionJsonSerializerSettings settings = null)
            : base(settings)
        {
            Collection = new CollectionRepresentation<TEntity>(Settings);
        } 
        
        public CollectionJsonWriter(HttpStatusCode httpStatusCode,
            string message = null)
        {
            if ((int)httpStatusCode < 400)
                throw new NotImplementedException("Currently only status code above/equal 400 supported (errors)");

            Error = new ErrorRepresentation(httpStatusCode, message);
        }

        public CollectionJsonWriter(TEntity entity,
            CollectionJsonSerializerSettings settings = null)
            : base(settings)
        {
            Collection = new CollectionRepresentation<TEntity>(entity, Settings);
        }

        public CollectionJsonWriter(IEnumerable<TEntity> entities,
            CollectionJsonSerializerSettings settings = null)
            : base(settings)
        {
            Collection = new CollectionRepresentation<TEntity>(entities, Settings);
        }
        

        /* Properties */
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CollectionRepresentation<TEntity> Collection { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ErrorRepresentation Error { get; private set; }
    }

    
}
