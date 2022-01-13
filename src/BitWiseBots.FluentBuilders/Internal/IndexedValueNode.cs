using System;
using System.Linq.Expressions;

namespace BitWiseBots.FluentBuilders.Internal
{
    /// <summary>
    /// A node that represents an indexed property that only has a value.
    /// </summary>
    internal sealed class IndexedValueNode : ValueNode
    {
        /// <summary>
        /// Constructs a new <see cref="IndexedValueNode"/>.
        /// </summary>
        /// <param name="expression">The expression that accesses the property this node represents.</param>
        /// <param name="value">The value to be set on the property this node represents</param>
        public IndexedValueNode(IndexExpression expression, object value, Type valueType, bool allowDefaults, RootNode root) : base (expression, value, valueType, allowDefaults, root)
        {
        }
        
        /// <inheritdoc />
        public override void ApplyTo(object objectToBuild)
        {
            if (UsedByConstructor) return;

            var memberInfo = TypedExpression.Indexer;

            var indexerArguments = TypedExpression.GetIndexerArguments();

            memberInfo.SetValue(objectToBuild, Value, indexerArguments);
        }

        /// <summary>
        /// Casts the provided expression to a <see cref="IndexExpression"/> so we can work with it directly.
        /// </summary>
        private IndexExpression TypedExpression => (IndexExpression)Expression;
    }
}