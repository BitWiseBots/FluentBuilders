using System;
using System.Linq.Expressions;
using System.Reflection;

namespace BitWiseBots.FluentBuilders.Internal
{
    /// <summary>
    /// A node that only has a value and no children.
    /// </summary>
    internal abstract class ValueNode : TreeNode
    {
        private static readonly MethodInfo GetDefaultMethod = typeof(ValueNode).GetMethod(nameof(GetDefaultGeneric), BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly object _value;
        private readonly Type _valueType;

        private readonly bool _isDelegate;
        private readonly bool _delegateTakesBuilder;

        private readonly bool _allowDefaults;

        /// <summary>
        /// Constructs a new <see cref="ValueNode"/>.
        /// </summary>
        /// <param name="expression">The expression that accesses the property this node represents.</param>
        /// <param name="value">The value to be set on the property this node represents.</param>
        /// <param name="valueType">The type of property this node represents.</param>
        /// <param name="allowDefaults">Whether or not to use registered type defaults when <paramref name="value"/> is <c>default</c>.</param>
        /// <param name="root">The <see cref="RootNode"/> for the tree this node belongs to.</param>
        protected ValueNode(Expression expression, object value, Type valueType, bool allowDefaults, RootNode root) : base(root)
        {
            Expression = expression;
            _value = value;
            _valueType = valueType;
            _allowDefaults = allowDefaults;

            if (value is not Delegate d) return;

            _isDelegate = true;
            _delegateTakesBuilder = d.Method.GetParameters().Length > 0;
        }

        /// <summary>
        /// The value to be set on the property this node represents.
        /// </summary>
        public object Value
        {
            get
            {
                var value = _isDelegate
                    ? _delegateTakesBuilder
                        ? ((Delegate) _value).DynamicInvoke(Root.SourceBuilder)
                        : ((Delegate) _value).DynamicInvoke()
                    : _value;

                if (Equals(value, GetDefault(_valueType)) && _allowDefaults)
                {
                    value = Root.BuilderRegistrationStore.GetTypeDefaultFunc(_valueType)?.DynamicInvoke();
                }

                return value;
            }
        }

        /// <summary>
        /// The expression that accesses the property represented by this node.
        /// </summary>
        protected Expression Expression { get; }

        /// <summary>
        /// Gets the value of <c>default(<paramref name="t"/>)</c>.
        /// </summary>
        private object GetDefault(Type t)
        {
            return GetDefaultMethod.MakeGenericMethod(t).Invoke(this, null);
        }

        /// <summary>
        /// A generic pass through to <c>default</c> to be used via reflection.
        /// </summary>
        private T GetDefaultGeneric<T>()
        {
            return default;
        }
    }
}