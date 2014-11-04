using System;

namespace CollectionJsonExtended.Core
{
    public interface IDenormalizedReference
    {
        int Id { get; set; }
        string Name { get; set; }
    }


    public class DenormalizedReference<T> where T : IDenormalizedReference
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public Uri Link { get; set; }

        public static implicit operator DenormalizedReference<T>(T entity)
        {
            return new DenormalizedReference<T>
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }
    }
        
}