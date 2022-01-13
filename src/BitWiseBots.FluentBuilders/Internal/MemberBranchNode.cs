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
        public MemberBranchNode(MemberExpression property, RootNode root) 
            : base(((PropertyInfo)property.Member).PropertyType, root)
        {
            _property = property;
        }

        protected override bool IsBuilderNode => !IsMutable;

        /// <inheritdoc />
        public override void ApplyTo(object objectToBuild)
        {
            var value = GetOrCreateValue();
            ApplyToChildren(value);

            if (UsedByConstructor) return; 

            var memberInfo = (PropertyInfo)_property.Member;

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