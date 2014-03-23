using System;

namespace CollectionJsonExtended.Core.Attributes
{
    //TODO: probably depr. as we find types via assembly... (this has to be checked)
    
    /// <summary>
    /// Instructs the CollectionJson serialization and deserialization to use the given concrete type for the abstract or interface property.
    /// A list of valid concrete types can be provided by multiple usage of this attribute. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class CollectionJsonConcreteTypeAttribute : Attribute
    {
        private readonly Type _concreteType;
        private string _prompt;

        public CollectionJsonConcreteTypeAttribute(Type concreteType)
        {
            _concreteType = concreteType;
        }
    

        public Type Type {
            get { return _concreteType; }
        }

        //TODO implement usage for prompt of concrete Types
        public string Prompt
        {
            get { return _prompt ?? _concreteType.Name; }
            set { _prompt = value; }
        }
    }
}