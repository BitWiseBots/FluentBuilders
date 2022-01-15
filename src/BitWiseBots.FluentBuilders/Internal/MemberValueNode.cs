using System;
using System.Linq.Expressions;
using System.Reflection;

namespace BitWiseBots.FluentBuilders.Internal
{
    /// <summary>
    /// A node that represents an standard property that only has a value.
    /// </summary>
    internal sealed class MemberValueNode : ValueNode
    {
        /// <summary>
        /// Constructs a new <see cref="IndexedValueNode"/>.
        /// </summary>
        /// <param name="expression">The expression that accesses the property this node represents.</param>
        /// <param name="value">The value to be set on the property this node represents</param>
        /// <param name="valueType">The type of property this node represents.</param>
        /// <param name="allowDefaults">Whether or not to use registered type defaults when <paramref name="value"/> is <c>default</c>.</param>
        /// <param name="root">The <see cref="RootNode"/> for the tree this node belongs to.</param>
        public MemberValueNode(MemberExpression expression, object value, Type valueType, bool allowDefaults, RootNode root) 
            : base(expression, value, valueType, allowDefaults, root)
        {
        }
        
        /// <inheritdoc />
        public override void ApplyTo(object objectToBuild)
        {
            if (UsedByConstructor) return;

            var memberInfo = (PropertyInfo)TypedExpression.Member;

            memberInfo.SetValue(objectToBuild, Value);
        }

        /// <summary>
        /// Casts the provided expression to a <see cref="MemberExpression"/> so we can work with it directly.
        /// </summary>
        private MemberExpression TypedExpression => (MemberExpression) Expression;
    }
}