using System.Linq.Expressions;
using System.Reflection;

namespace BitWiseBots.FluentBuilders.Internal
{
    /// <summary>
    /// A node that represents a standard property that has children.
    /// </summary>
    internal sealed class MemberBranchNode : BranchNode
    {
        private readonly MemberExpression _property;

        /// <summary>
        /// Constructs a new <see cref="MemberBranchNode"/>.
        /// </summary>
        /// <param name="property">The expression that accesses the property this node represents.</param>
        /// <param name="root"></param>
        public MemberBranchNode(MemberExpression property, RootNode root) 
            : base(((PropertyInfo)property.Member).PropertyType, root)
        {
            _property = property;
        }

        /// <inheritdoc />
        protected override bool IsBuilderNode => !IsMutable;

        /// <inheritdoc />
        public override void ApplyTo(object objectToBuild)
        {
            if (UsedByConstructor) return;

            var memberInfo = (PropertyInfo)_property.Member;

            var value = GetOrCreateValue(memberInfo.GetValue(objectToBuild));
            ApplyToChildren(value);

            if (IsBuilderNode)
            {
                var genMethod = BuildBuilderMethod.MakeGenericMethod(NodeType);
                var builderValue = genMethod.Invoke(this, new []{ value });
                memberInfo.SetValue(objectToBuild, builderValue);
            }
            else
            {                
                memberInfo.SetValue(objectToBuild, value);
            }
        }
    }
}