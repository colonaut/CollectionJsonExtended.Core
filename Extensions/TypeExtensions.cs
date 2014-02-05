using System;
using System.Collections.Generic;
using System.Linq;

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

        internal static string GetSimpleTypeName(this Type type)
        {
            var typeName = type.FullName;
            bool isArray = false, isNullable = false;

            if (type.IsArray) //|| type.GetInterfaces().Contains(typeof (IEnumerable)))
            {
                isArray = true;
                typeName = typeName.Remove(typeName.IndexOf("[]", StringComparison.Ordinal), 2);
            }
            
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                isNullable = true;
                typeName = nullableType.FullName;
            }

            string parsedTypeName = null;
            #region switchLookupSystemtypes
            switch (typeName)
            {
                case "System.Boolean":
                    parsedTypeName = "bool";
                    break;
                case "System.Byte":
                    parsedTypeName = "byte";
                    break;
                case "System.Char":
                    parsedTypeName = "char";
                    break;
                case "System.DateTime":
                    parsedTypeName = "datetime";
                    break;
                case "System.DateTimeOffset":
                    parsedTypeName = "datetimeoffset";
                    break;
                case "System.Decimal":
                    parsedTypeName = "decimal";
                    break;
                case "System.Double":
                    parsedTypeName = "double";
                    break;
                case "System.Single":
                    parsedTypeName = "float";
                    break;
                case "System.Int16":
                    parsedTypeName = "short";
                    break;
                case "System.Int32":
                    parsedTypeName = "int";
                    break;
                case "System.Int64":
                    parsedTypeName = "long";
                    break;
                case "System.Object":
                    parsedTypeName = "object";
                    break;
                case "System.SByte":
                    parsedTypeName = "sbyte";
                    break;
                case "System.String":
                    parsedTypeName = "string";
                    break;
                case "System.TimeSpan":
                    parsedTypeName = "timespan";
                    break;
                case "System.UInt16":
                    parsedTypeName = "ushort";
                    break;
                case "System.UInt32":
                    parsedTypeName = "uint";
                    break;
                case "System.UInt64":
                    parsedTypeName = "ulong";
                    break;
            }
            #endregion

            if (parsedTypeName == null)
                return type.Name;

            if (isArray)
                parsedTypeName = parsedTypeName + "[]";

            if (isNullable)
                parsedTypeName = parsedTypeName + "?";

            return parsedTypeName;
        }

        static Type GetType(string simpleTypeName)
        {
            simpleTypeName = simpleTypeName.Trim().ToLower();

            bool isArray = false, isNullable = false;

            if (simpleTypeName.IndexOf("[]", System.StringComparison.Ordinal) != -1)
            {
                isArray = true;
                simpleTypeName = simpleTypeName.Remove(simpleTypeName.IndexOf("[]", System.StringComparison.Ordinal), 2);
            }

            if (simpleTypeName.IndexOf("?", System.StringComparison.Ordinal) != -1)
            {
                isNullable = true;
                simpleTypeName = simpleTypeName.Remove(simpleTypeName.IndexOf("?", System.StringComparison.Ordinal), 1);
            }

            string parsedTypeName = null;
            #region switchLookupSystemtypes
            switch (simpleTypeName)
            {
                case "bool":
                case "boolean":
                    parsedTypeName = "System.Boolean";
                    break;
                case "byte":
                    parsedTypeName = "System.Byte";
                    break;
                case "char":
                    parsedTypeName = "System.Char";
                    break;
                case "datetime":
                    parsedTypeName = "System.DateTime";
                    break;
                case "datetimeoffset":
                    parsedTypeName = "System.DateTimeOffset";
                    break;
                case "decimal":
                    parsedTypeName = "System.Decimal";
                    break;
                case "double":
                    parsedTypeName = "System.Double";
                    break;
                case "float":
                    parsedTypeName = "System.Single";
                    break;
                case "int16":
                case "short":
                    parsedTypeName = "System.Int16";
                    break;
                case "int32":
                case "int":
                    parsedTypeName = "System.Int32";
                    break;
                case "int64":
                case "long":
                    parsedTypeName = "System.Int64";
                    break;
                case "object":
                    parsedTypeName = "System.Object";
                    break;
                case "sbyte":
                    parsedTypeName = "System.SByte";
                    break;
                case "string":
                    parsedTypeName = "System.String";
                    break;
                case "timespan":
                    parsedTypeName = "System.TimeSpan";
                    break;
                case "uint16":
                case "ushort":
                    parsedTypeName = "System.UInt16";
                    break;
                case "uint32":
                case "uint":
                    parsedTypeName = "System.UInt32";
                    break;
                case "uint64":
                case "ulong":
                    parsedTypeName = "System.UInt64";
                    break;
            }
            #endregion

            if (parsedTypeName != null)
            {
                if (isArray)
                {
                    parsedTypeName = parsedTypeName + "[]";
                }

                if (isNullable)
                {
                    parsedTypeName = String.Concat("System.Nullable`1[", parsedTypeName, "]");
                }

                return Type.GetType(parsedTypeName, true, false);
            }

            return Type.GetType(simpleTypeName, true, true);
        }

    }
}