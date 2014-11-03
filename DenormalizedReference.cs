using System;

namespace CollectionJsonExtended.Core
{
    public interface INamedDocument
    {
        int Id { get; set; }
        string Name { get; set; }
    }


    public class DenormalizedReference<T> where T : INamedDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static implicit operator DenormalizedReference<T>(T doc)
        {
            return new DenormalizedReference<T>
            {
                Id = doc.Id,
                Name = doc.Name
            };
        }
    }
        
}