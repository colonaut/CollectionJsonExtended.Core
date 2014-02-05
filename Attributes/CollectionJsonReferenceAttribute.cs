using System;

namespace CollectionJsonExtended.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class CollectionJsonReferenceAttribute : Attribute
    {
        readonly Type _referenceType;
        public CollectionJsonReferenceAttribute(Type referenceType)
        {
            _referenceType = referenceType;
        }

        public Type ReferenceType
        {
            get { return _referenceType; }
        }
    }
}