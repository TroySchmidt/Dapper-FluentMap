using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.FluentMap.Mapping
{
    public enum RelationshipType
    {
        HasOne,
        WithMany
    }

    public interface IRelationshipMap
    {
        PropertyInfo PropertyInfo { get; }

        Type Entity { get; }

        Type ReferencedEntityType { get; }

        RelationshipType RelationshipType { get; }

        LambdaExpression Relationship { get; }
    }

    public abstract class RelationshipMapBase<TEntity, TRelatedEntity, TRelationshipMap>
        where TRelationshipMap : class, IRelationshipMap
    {
        protected RelationshipMapBase(PropertyInfo info, RelationshipType relationshipType)
        {
            PropertyInfo = info;
            RelationshipType = relationshipType;
            var test = PropertyInfo.GetType().GetInterfaces()
            .Where(t => t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(t => t.GetGenericArguments()[0]);
        }

        public PropertyInfo PropertyInfo { get; }

        public Type Entity => PropertyInfo.DeclaringType;

        public Type ReferencedEntityType => (RelationshipType == RelationshipType.HasOne)
            ? PropertyInfo.PropertyType
            : PropertyInfo.PropertyType.GetGenericArguments()[0];

        public LambdaExpression Relationship { get; private set; }

        public RelationshipType RelationshipType { get; }

        public TRelationshipMap Where(Expression<Func<TEntity, TRelatedEntity, bool>> relationship)
        {
            // This is where we need the magic to convert the join.
            // a (parent), b(child) => a.Property == b.Property (and more for complex keys)
            Relationship = relationship;
            return this as TRelationshipMap;
        }

        #region EditorBrowsableStates
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
        #endregion
    }

    public class RelationshipMap<TEntity, TRelatedEntity> : RelationshipMapBase<TEntity, TRelatedEntity, RelationshipMap<TEntity, TRelatedEntity>>, IRelationshipMap
    {
        public RelationshipMap(PropertyInfo info, RelationshipType relationshipType) : base(info, relationshipType) { }
    }
}
