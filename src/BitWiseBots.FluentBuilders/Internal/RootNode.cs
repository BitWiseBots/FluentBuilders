using System;

namespace BitWiseBots.FluentBuilders.Internal
{
    /// <summary>
    /// A node that represents the top of a given tree.
    /// </summary>
    internal sealed class RootNode : BranchNode
    {
        public RootNode(object sourceBuilder, BuilderRegistrationStore registrationStore, Type nodeType ) : base(nodeType, null)
        {
            SourceBuilder = sourceBuilder;
            BuilderRegistrationStore = registrationStore;
        }

        /// <summary>
        /// A reference to the <see cref="Builder{T}"/> that this node belongs to.
        /// </summary>
        public object SourceBuilder { get; }

        /// <summary>
        /// A reference to the currently configured <see cref="BuilderRegistrationStore"/> to allow access to the various registrations when building.
        /// </summary>
        public BuilderRegistrationStore BuilderRegistrationStore { get; }

        /// <inheritdoc />
        public override RootNode Root => this;

        /// <inheritdoc />
        protected override bool IsBuilderNode => false;
    }
}