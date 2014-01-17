using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CollectionJsonExtended.Core.Extensions
{
    internal class InstanceTypeCollection
    {
        readonly IDictionary<Type, IEnumerable<Type>> _collectionByNonInstanceType;

        public InstanceTypeCollection()
        {
            _collectionByNonInstanceType = new Dictionary<Type, IEnumerable<Type>>();
        }

        public IEnumerable<Type> Find(Type nonInstanceType)
        {
            if (!nonInstanceType.IsAbstract && !nonInstanceType.IsInterface)
                throw new ArgumentException(nonInstanceType.Name +
                                            " is an instance type, should be abstract or interface");
            IEnumerable<Type> result;
            if (_collectionByNonInstanceType.TryGetValue(nonInstanceType, out result))
                return result;

            result = nonInstanceType.Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(nonInstanceType))
                .OrderBy(t => t.Name)
                .ToList();
            _collectionByNonInstanceType.Add(nonInstanceType, result);
            return result;
        }
    }
    
    //TODO: only works if abstract or interface is within same assembly as instance types, also nested abstracts or interfaces do not work
    //TODO: if this is not enough we might use CollectionJsonConcreteTypeAttribute, to determine instance type
    internal static class TypeExtensions
    {
        internal static IEnumerable<Type> GetInstanceTypes(this Type nonInstanceType)
        {
            return SingletonFactory<InstanceTypeCollection>.Instance.Find(nonInstanceType);
        }

        internal static bool TryGetInstanceType(this Type nonInstanceType, string instanceTypeName, out Type value)
        {
            value = nonInstanceType.GetInstanceTypes()
                .SingleOrDefault(t => string.Equals(t.Name, instanceTypeName,
                    StringComparison.InvariantCultureIgnoreCase));
            return value != null;
        }
    }
}