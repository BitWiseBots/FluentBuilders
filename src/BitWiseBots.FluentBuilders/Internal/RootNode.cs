using System;

namespace BitWiseBots.FluentBuilders.Internal
{
    /// <summary>
    /// A node that represents the top of a given tree.
    /// </summary>
    internal sealed class RootNode : BranchNode
    {
        public RootNode(object sourceBuilder, ConfigStore configStore, Type nodeType ) : base(nodeType, null)
        {
            SourceBuilder = sourceBuilder;
            ConfigStore = configStore;
        }

        /// <summary>
        /// A reference to the <see cref="Builder{T}"/> that this node belongs to.
        /// </summary>
        public object SourceBuilder { get; }

        /// <summary>
        /// A reference to the current <see cref="ConfigStore"/> to allow access to the various configs when building.
        /// </summary>
        public ConfigStore ConfigStore { get; }

        /// <inheritdoc />
        public override RootNode Root => this;

        /// <inheritdoc />
        protected override bool IsBuilderNode => false;
    }
}