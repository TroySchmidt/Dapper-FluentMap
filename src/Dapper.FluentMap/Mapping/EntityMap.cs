using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.FluentMap.Utils;

namespace Dapper.FluentMap.Mapping
{
    /// <summary>
    /// Represents a non-typed mapping of an entity.
    /// </summary>
    public interface IEntityMap
    {
        /// <summary>
        /// Gets the collection of mapped properties.
        /// </summary>
        IList<IPropertyMap> PropertyMaps { get; }
    }

    /// <summary>
    /// Represents a typed mapping of an entity.
    /// This serves as a marker interface for generic type inference.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to configure the mapping for.</typeparam>
    public interface IEntityMap<TEntity> : IEntityMap
    {
    }

    /// <summary>
    /// Serves as the base class for all entity mapping implementations.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TPropertyMap">The type of the property mapping.</typeparam>
    public abstract class EntityMapBase<TEntity, TPropertyMap> : IEntityMap<TEntity>
        where TPropertyMap : IPropertyMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMapBase{TEntity, TPropertyMap}"/> class.
        /// </summary>
        protected EntityMapBase()
        {
            PropertyMaps = new List<IPropertyMap>();
            RelationshipMaps = new List<IRelationshipMap>();
        }

        /// <summary>
        /// Gets the collection of mapped properties.
        /// </summary>
        public IList<IPropertyMap> PropertyMaps { get; }

        public IList<IRelationshipMap> RelationshipMaps { get; }

        /// <summary>
        /// Returns an instance of <typeparamref name="TPropertyMap"/> which can perform custom mapping
        /// for the specified property on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="expression">Expression to the property on <typeparamref name="TEntity"/>.</param>
        /// <returns>The created <see cref="T:Dapper.FluentMap.Mapping.PropertyMap"/> instance. This enables a fluent API.</returns>
        /// <exception cref="T:System.Exception">when a duplicate mapping is provided.</exception>
        protected TPropertyMap Map(Expression<Func<TEntity, object>> expression)
        {
            var info = (PropertyInfo)ReflectionHelper.GetMemberInfo(expression);
            var propertyMap = GetPropertyMap(info);
            ThrowIfDuplicateMapping(propertyMap);
            PropertyMaps.Add(propertyMap);
            return propertyMap;
        }

        /// <summary>
        /// When overridden in a derived class, gets the property mapping for the specified property.
        /// </summary>
        /// <param name="info">The <see cref="PropertyInfo"/> for the property.</param>
        /// <returns>An instance of <typeparamref name="TPropertyMap"/>.</returns>
        protected abstract TPropertyMap GetPropertyMap(PropertyInfo info);

        protected RelationshipMap<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(Expression<Func<TEntity, object>> expression)
        {
            // Need to get property that is a type that isn't primitive that could be singular or collection.
            // Then start to map the columns and build the where clause if it isn't provided.

            var info = (PropertyInfo)ReflectionHelper.GetMemberInfo(expression);
            var type = info.PropertyType;
            var relationshipMap = GetRelationshipMap<TRelatedEntity>(info, RelationshipType.HasOne);
            RelationshipMaps.Add(relationshipMap);
            return relationshipMap;
        }

        protected RelationshipMap<TEntity, TRelatedEntity> WithMany<TRelatedEntity>(Expression<Func<TEntity, object>> expression)
        {
            var info = (PropertyInfo)ReflectionHelper.GetMemberInfo(expression);
            var type = info.PropertyType;
            var relationshipMap = GetRelationshipMap<TRelatedEntity>(info, RelationshipType.WithMany);
            RelationshipMaps.Add(relationshipMap);
            return relationshipMap;
        }

        protected abstract RelationshipMap<TEntity, TRelatedEntity> GetRelationshipMap<TRelatedEntity>(PropertyInfo info, RelationshipType relationshipType);

        private void ThrowIfDuplicateMapping(IPropertyMap map)
        {
            if (PropertyMaps.Any(p => p.PropertyInfo.Name == map.PropertyInfo.Name))
            {
                throw new Exception($"Duplicate mapping detected. Property '{map.PropertyInfo.Name}' is already mapped to column '{map.ColumnName}'.");
            }
        }

        private void ThrowIfDuplicateMapping(IRelationshipMap map)
        {
            if (RelationshipMaps.Any(p => p.PropertyInfo.Name == map.PropertyInfo.Name))
            {
                throw new Exception($"Duplicate mapping detected. Property '{map.PropertyInfo.Name}' is already mapped to column '{map.ReferencedEntityType}'.");
            }
        }
    }

    /// <summary>
    /// Represents a typed mapping of an entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to configure the mapping for.</typeparam>
    public abstract class EntityMap<TEntity> : EntityMapBase<TEntity, PropertyMap>
        where TEntity : class
    {
        /// <inheritdoc />
        protected override PropertyMap GetPropertyMap(PropertyInfo info)
        {
            return new PropertyMap(info);
        }

        protected override RelationshipMap<TEntity, TRelatedEntity> GetRelationshipMap<TRelatedEntity>(PropertyInfo info, RelationshipType relationshipType)
        {
            return new RelationshipMap<TEntity, TRelatedEntity>(info, relationshipType);
        }
    }
}
