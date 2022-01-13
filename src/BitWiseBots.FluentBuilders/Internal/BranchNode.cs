using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace BitWiseBots.FluentBuilders.Internal
{
    internal sealed class RootNode : BranchNode
    {
        public RootNode(object sourceBuilder, BuilderRegistrationStore registrationStore, Type nodeType ) : base(nodeType, null)
        {
            SourceBuilder = sourceBuilder;
            BuilderRegistrationStore = registrationStore;
        }

        public object SourceBuilder { get; }
        public BuilderRegistrationStore BuilderRegistrationStore { get; }

        public override RootNode Root => this;
        protected override bool IsBuilderNode => false;
    }

    /// <summary>
    /// A node that has children nodes.
    /// </summary>
    internal abstract class BranchNode : TreeNode
    {
        // Pre fill the method Info to reduce the cost of reflecting.
        protected static readonly MethodInfo BuildBuilderMethod = typeof(BranchNode).GetMethod(nameof(BuildBuilder), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo ApplyToBuilderMethod = typeof(BranchNode).GetMethod(nameof(ApplyToBuilder), BindingFlags.NonPublic | BindingFlags.Instance);        

        // Children nodes
        private readonly Dictionary<string, BranchNode> _nodeBranches = new Dictionary<string, BranchNode>();
        private readonly Dictionary<string, ValueNode> _nodeValues = new Dictionary<string, ValueNode>();

        /// <summary>
        /// Constructs a new <see cref="BranchNode"/>.
        /// </summary>
        /// <param name="nodeType">The type of the property the node represents.</param>
        protected BranchNode(Type nodeType, RootNode root) : base(root)
        {
            IsMutable = HasParameterlessConstructor();
            NodeType = nodeType;

            bool HasParameterlessConstructor()
            {
                return nodeType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null;
            }
        }

        protected abstract bool IsBuilderNode { get; }

        protected bool IsMutable { get; }
        protected Type NodeType { get; }

        /// <inheritdoc />
        public override void ApplyTo(object objectToBuild)
        {
            ApplyToChildren(objectToBuild);
        }

        /// <summary>
        ///  Handles calling <see cref="ApplyToChildren(object)"/> on all children nodes.
        /// </summary>
        /// <param name="objectToBuild">The current object being built.</param>
        public void ApplyToChildren(object objectToBuild)
        {
            // Type is not mutable so we need to start a new builder and pass of children to it.
            if (IsBuilderNode)
            {
                var genMethod = ApplyToBuilderMethod.MakeGenericMethod(NodeType);
                genMethod.Invoke(this, new [] { objectToBuild });
            }
            else
            {
                foreach (var nodeBranch in _nodeBranches)
                {
                    nodeBranch.Value.ApplyTo(objectToBuild);
                }

                foreach (var nodeValue in _nodeValues)
                {
                    nodeValue.Value.ApplyTo(objectToBuild);
                }
            }
        }

        /// <summary>
        /// Runs the <see cref="ApplyTo"/> logic for when nodes are used by a registered constructor function.
        /// </summary>
        /// <returns>The constructed valueFunc for this node with all child nodes already processed.</returns>
        public object ApplyToConstructor()
        {
            var value = GetOrCreateValue();
            if (IsBuilderNode)
            {
                var genApplyMethod = ApplyToBuilderMethod.MakeGenericMethod(NodeType);
                genApplyMethod.Invoke(this, new [] { value });
            }
            else
            {
                ApplyToChildren(value);
            }

            if (IsMutable) return value;

            var genMethod = BuildBuilderMethod.MakeGenericMethod(NodeType);
            return genMethod.Invoke(this, new []{ value });
        }

        /// <summary>
        /// Searches immediate children for a given node.
        /// </summary>
        /// <param name="key">The key of the node to find.</param>
        /// <returns>Either a <see cref="BranchNode"/> or <see cref="ValueNode"/> if a child was found, and <c>null</c> if none was found.</returns>
        public TreeNode Find(string key)
        {
            if(_nodeBranches.ContainsKey(key)) return _nodeBranches[key];
            if(_nodeValues.ContainsKey(key)) return _nodeValues[key];
            return null;
        }

        /// <summary>
        /// Gets an existing child <see cref="BranchNode"/> or adds a new <see cref="BranchNode"/> as a child to this node.
        /// </summary>
        /// <param name="key">The key for the node being added.</param>
        /// <param name="expr">The expression that accesses the property represented by the node.</param>
        /// <returns>Either the newly created <see cref="BranchNode"/> if it didn't yet exist, or the existing <see cref="BranchNode"/> if it did.</returns>
        public BranchNode AddOrGetBranchNode(string key, Expression expr)
        {
            if (_nodeBranches.ContainsKey(key))
            {
                return _nodeBranches[key];
            }

            switch (expr)
            {
                case MemberExpression memberExpression:
                    _nodeBranches.Add(key, new MemberBranchNode(memberExpression, Root));
                    break;
                case IndexExpression indexExpression:
                    _nodeBranches.Add(key, new IndexedBranchNode(indexExpression, Root));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return _nodeBranches[key];
        }

        /// <summary>
        /// Gets an existing child <see cref="ValueNode"/> or adds a new <see cref="ValueNode"/> as a child to this node.
        /// </summary>
        /// <param name="key">The key for the node being added.</param>
        /// <param name="expr">The expression that accesses the property represented by the node.</param>
        /// <param name="value">The valueFunc to be set for the property represented by the node.</param>
        /// <returns>Either the newly created <see cref="ValueNode"/> if it didn't yet exist, or the existing <see cref="ValueNode"/> if it did.</returns>
        public ValueNode AddOrGetValueNode<T>(string key, Expression expr, object value, bool allowDefaults)
        {
            if (_nodeValues.ContainsKey(key))
            {
                return _nodeValues[key];
            }

            switch (expr)
            {
                case MemberExpression memberExpression:
                    _nodeValues.Add(key, new MemberValueNode(memberExpression, value, typeof(T), allowDefaults, Root));
                    break;
                case IndexExpression indexExpression:
                    _nodeValues.Add(key, new IndexedValueNode(indexExpression, value, typeof(T), allowDefaults, Root));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return _nodeValues[key];
        }

        /// <summary>
        /// Internal method for adding a<see cref="BranchNode"/> while handling immutable properties.
        /// </summary>
        /// <param name="key">The key of the node to add.</param>
        /// <param name="branchNode">The pre-existing <see cref="BranchNode"/> to add.</param>
        /// <remarks>
        /// It should not be possible for this method to try to add a key that is already present.
        /// </remarks>
        internal void AddBranchNode(string key, BranchNode branchNode)
        {
            _nodeBranches.Add(key, branchNode);
        }

        /// <summary>
        /// Internal method for adding a<see cref="ValueNode"/> while handling immutable properties.
        /// </summary>
        /// <param name="key">The key of the node to add.</param>
        /// <param name="valueNode">The pre-existing <see cref="ValueNode"/> to add.</param>
        /// <remarks>
        /// It should not be possible for this method to try to add a key that is already present.
        /// </remarks>
        internal void AddValueNode(string key, ValueNode valueNode)
        {
            _nodeValues.Add(key, valueNode);
        }

        /// <summary>
        /// Get the valueFunc to be used for the current node.
        /// </summary>
        /// <returns>Either a simple valueFunc if the node represents a mutable object, or a <see cref="Builder{T}"/> of the type for an immutable object.</returns>
        protected object GetOrCreateValue()
        {
            if (IsMutable)
            {
                return Activator.CreateInstance(NodeType, true);
            }
            else
            {
                var builderType = typeof(Builder<>);
                var genBuilderType = builderType.MakeGenericType(NodeType);
                return Activator.CreateInstance(genBuilderType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[]{Root.BuilderRegistrationStore, null, null}, null);
            }
        }

        /// <summary>
        /// Alternate version of <see cref="ApplyTo(object)"/> that is specific to handling immutable nodes.
        /// </summary>
        /// <typeparam name="T">The type of the property the node represents</typeparam>
        /// <param name="builder">The <see cref="Builder{T}"/> to construct that valueFunc for the node.</param>
        /// <remarks>
        /// This method is used via reflection to close the generic type
        /// </remarks>
        private void ApplyToBuilder<T>(Builder<T> builder)
        {
            foreach (var nodeBranch in _nodeBranches)
            {
                builder.BuilderRootNode.AddBranchNode(nodeBranch.Key, nodeBranch.Value);
            }

            foreach (var nodeValue in _nodeValues)
            {
                builder.BuilderRootNode.AddValueNode(nodeValue.Key, nodeValue.Value);
            }
        }

        /// <summary>
        /// Helper method to call <see cref="Builder{T}.Build"/> on the provided <see cref="Builder{T}"/>
        /// </summary>
        /// <typeparam name="T">The type of object the builder will produce.</typeparam>
        /// <param name="builder">The <see cref="Builder{T}"/> to call <see cref="Builder{T}.Build"/> on.</param>
        /// <returns>The built valueFunc.</returns>
        /// <remarks>
        /// This method is used via reflection to close the generic type
        /// </remarks>
        private T BuildBuilder<T>(Builder<T> builder)
        {
            return builder.Build();
        }
    }
}