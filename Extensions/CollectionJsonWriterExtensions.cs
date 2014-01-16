namespace CollectionJsonExtended.Core.Extensions
{
    public static class CollectionJsonWriterExtensions
    {
        public static string ParseVirtualPath<TEntity>(this ItemRepresentation<TEntity> representation, TEntity entity)
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

        public static string ParseVirtualPath<TEntity>(this CollectionRepresentation<TEntity> representation)
            where TEntity : class, new()
        {
            UrlInfoBase urlInfo;
            if (!SingletonFactory<UrlInfoCollection>.Instance
                .TryFindSingle(typeof(TEntity), Is.Base, out urlInfo))
                return null;
            return urlInfo.VirtualPath;
        }

    }

}
