using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Core.Factory;
using FlitBit.Dapper;
using FlitBit.IoC;
using FlitBit.IoC.Meta;

namespace Dapper
{
    /// <summary>
    /// Implement this interface to change default mapping of reader columns to type memebers
    /// </summary>
    public interface ITypeMap
    {
        /// <summary>
        /// Finds best constructor
        /// </summary>
        /// <param name="names">DataReader column names</param>
        /// <param name="types">DataReader column types</param>
        /// <returns>Matching constructor or default one</returns>
        ConstructorInfo FindConstructor(string[] names, Type[] types);

        /// <summary>
        /// Gets mapping for constructor parameter
        /// </summary>
        /// <param name="constructor">Constructor to resolve</param>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Mapping implementation</returns>
        IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName);

        /// <summary>
        /// Gets member mapping for column
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Mapping implementation</returns>
        IMemberMap GetMember(string columnName);
    }



    /// <summary>
    /// Represents default type mapping strategy used by Dapper
    /// </summary>
    [ContainerRegister(typeof(ITypeMap), RegistrationBehaviors.Default)]
    public class DefaultTypeMap : ITypeMap
    {
        private readonly List<FieldInfo> _fields;
        private readonly List<PropertyInfo> _properties;
        private readonly Type _type;
        private readonly IFactory _factory;


        /// <summary>
        /// Creates default type map
        /// </summary>
        /// <param name="type">Entity type</param>
        public DefaultTypeMap(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            _fields = GetSettableFields(type);
            _properties = GetSettableProps(type);
            _type = type;

            _factory = FactoryProvider.Factory;
        }

        internal static MethodInfo GetPropertySetter(PropertyInfo propertyInfo, Type type)
        {
            return propertyInfo.DeclaringType == type ?
                       propertyInfo.GetSetMethod(true) :
                       propertyInfo.DeclaringType.GetProperty(propertyInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true);
        }

        internal static List<PropertyInfo> GetSettableProps(Type t)
        {
            return t
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => GetPropertySetter(p, t) != null)
                .ToList();
        }

        internal static List<FieldInfo> GetSettableFields(Type t)
        {
            return t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
        }

        /// <summary>
        /// Finds best constructor
        /// </summary>
        /// <param name="names">DataReader column names</param>
        /// <param name="types">DataReader column types</param>
        /// <returns>Matching constructor or default one</returns>
        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            var constructors = GetConstructorInfoFromType(_type);
            foreach (ConstructorInfo ctor in constructors.OrderBy(c => c.IsPublic ? 0 : (c.IsPrivate ? 2 : 1)).ThenBy(c => c.GetParameters().Length))
            {
                ParameterInfo[] ctorParameters = ctor.GetParameters();
                if (ctorParameters.Length == 0)
                    return ctor;

                if (ctorParameters.Length != types.Length)
                    continue;

                int i = 0;
                for (; i < ctorParameters.Length; i++)
                {
                    if (!String.Equals(ctorParameters[i].Name, names[i], StringComparison.OrdinalIgnoreCase))
                        break;
                    if (types[i] == typeof(byte[]) && ctorParameters[i].ParameterType.FullName == SqlMapper.LinqBinary)
                        continue;
                    var unboxedType = Nullable.GetUnderlyingType(ctorParameters[i].ParameterType) ?? ctorParameters[i].ParameterType;
                    if (unboxedType != types[i]
                        && !(unboxedType.IsEnum && Enum.GetUnderlyingType(unboxedType) == types[i])
                        && !(unboxedType == typeof(char) && types[i] == typeof(string)))
                        break;
                }

                if (i == ctorParameters.Length)
                    return ctor;
            }

            return null;
        }

        IEnumerable<ConstructorInfo> GetConstructorInfoFromType(Type type)
        {
            return _factory.GetImplementationType(type).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Gets mapping for constructor parameter
        /// </summary>
        /// <param name="constructor">Constructor to resolve</param>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Mapping implementation</returns>
        public IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            var parameters = constructor.GetParameters();
            return Create.NewWithParams<IMemberMap>(LifespanTracking.Automatic, Param.FromValue(columnName), Param.FromValue(parameters.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase))));
        }

        /// <summary>
        /// Gets member mapping for column
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Mapping implementation</returns>
        public IMemberMap GetMember(string columnName)
        {
            var property = GetPropertyInfoByColumnName(columnName);
            if (property != null)
                return new SimpleMemberMap(columnName, property);

            var field = GetFieldInfoByColumnName(columnName);
            if (field != null)
                return new SimpleMemberMap(columnName, field);

            return null;
        }

        PropertyInfo GetPropertyInfoByColumnName(string columnName)
        {
            var property = _properties.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.Ordinal))
                          ?? _properties.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

            // Is Column C# cased?
            if (property == null)
            {
                var name = columnName.ToCamelCase();
                property = _properties.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.Ordinal))
                          ?? _properties.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            }

            return property;
        }

        FieldInfo GetFieldInfoByColumnName(string columnName)
        {
            var field = _fields.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.Ordinal))
                       ?? _fields.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

            // Is Column C# cased?
            if (field == null)
            {
                var name = columnName.ToCamelCase();
                field = _fields.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.Ordinal))
                          ?? _fields.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            }

            return field;
        }
    }
}