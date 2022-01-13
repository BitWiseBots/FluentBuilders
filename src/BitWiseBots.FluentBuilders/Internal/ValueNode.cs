using System;
using System.Linq.Expressions;

namespace BitWiseBots.FluentBuilders.Internal
{
    /// <summary>
    /// A node only has a value.
    /// </summary>
    internal abstract class ValueNode : TreeNode
    {
        private readonly object _value;
        private readonly Type _valueType;

        private readonly bool _isDelegate;
        private readonly bool _delegateTakesBuilder;

        private readonly bool _allowDefaults;

        /// <summary>
        /// Constructs a new <see cref="ValueNode"/>.
        /// </summary>
        /// <param name="expression">The expression that accesses the property this node represents.</param>
        /// <param name="value">The value to be set on the property this node represents</param>
        protected ValueNode(Expression expression, object value, Type valueType, bool allowDefaults, RootNode root) : base(root)
        {
            Expression = expression;
            _value = value;
            _valueType = valueType;
            _allowDefaults = allowDefaults;

            if (!(value is Delegate d)) return;

            _isDelegate = true;
            _delegateTakesBuilder = d.Method.GetParameters().Length > 0;
        }

        /// <summary>
        /// The value to be set on the property this node represents.
        /// </summary>
        public object Value => _isDelegate 
            ? _delegateTakesBuilder 
                ? ((Delegate) _value).DynamicInvoke(Root.SourceBuilder) 
                : ((Delegate) _value).DynamicInvoke() 
            : _value
            ?? (_allowDefaults 
                ? Root.BuilderRegistrationStore.GetTypeDefaultFunc(_valueType)?.DynamicInvoke() 
                : null);

        /// <summary>
        /// The expression that accesses the property represented by this node.
        /// </summary>
        protected Expression Expression { get; }
    }
}