using System;
using System.Reflection;
using FlitBit.IoC;
using FlitBit.IoC.Meta;

namespace Dapper
{
    /// <summary>
    /// Implements custom property mapping by user provided criteria (usually presence of some custom attribute with column to member mapping)
    /// </summary>
    [ContainerRegister(typeof(ITypeMap), RegistrationBehaviors.Default, ScopeBehavior = ScopeBehavior.Singleton)]
    public class CustomPropertyTypeMap : ITypeMap
    {
        private readonly Type _type;
        private readonly Func<Type, string, PropertyInfo> _propertySelector;

        /// <summary>
        /// Creates custom property mapping
        /// </summary>
        /// <param name="type">Target entity type</param>
        /// <param name="propertySelector">Property selector based on target type and DataReader column name</param>
        public CustomPropertyTypeMap(Type type, Func<Type, string, PropertyInfo> propertySelector)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (propertySelector == null)
                throw new ArgumentNullException("propertySelector");

            _type = type;
            _propertySelector = propertySelector;
        }

        /// <summary>
        /// Always returns default constructor
        /// </summary>
        /// <param name="names">DataReader column names</param>
        /// <param name="types">DataReader column types</param>
        /// <returns>Default constructor</returns>
        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            return _type.GetConstructor(new Type[0]);
        }

        /// <summary>
        /// Not impelmeneted as far as default constructor used for all cases
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns property based on selector strategy
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Poperty member map</returns>
        public IMemberMap GetMember(string columnName)
        {
            var prop = _propertySelector(_type, columnName);
            return prop != null ? Create.NewWithParams<IMemberMap>(LifespanTracking.Automatic, Param.FromValue(columnName), Param.FromValue(prop)) : null;
        }
    }
}