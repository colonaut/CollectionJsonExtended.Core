using System.Collections.Generic;
using System.Linq;

namespace CollectionJsonExtended.Core.Extensions
{
    public static class CollectionJsonWriterExtensions
    {
        public static string GetParsedVirtualPath<TEntity>(this ItemRepresentation<TEntity> representation, TEntity entity)
            where TEntity : class, new()
        {
            //TODO: how to deal with renderType (TryFindSingle for i.e. Is.Item crashes, when we have another Is.Item that is for another renderType...)
            //MayBe add another method?

            UrlInfoBase urlInfo;
            if (!SingletonFactory<UrlInfoCollection>.Instance
                .TryFindSingle(typeof (TEntity), Is.Item, out urlInfo))
                return null;
            
            var primaryKey = urlInfo.PrimaryKeyProperty.GetValue(entity).ToString();
            var virtualPath = urlInfo.VirtualPath.Replace(urlInfo.PrimaryKeyTemplate, primaryKey);
            return virtualPath;
        }



        //TODO: cache this!!!!


       
        //rework
        public static IEnumerable<QueryRepresentation> GetQueryRepresentations<TEntity>(this CollectionRepresentation<TEntity> representation)
            where TEntity : class, new()
        {
            return SingletonFactory<UrlInfoCollection>.Instance
                .Find(typeof (TEntity), Is.Query)
                .Select(urlInfo => new QueryRepresentation(urlInfo));
        }
    }

}
