namespace BitWiseBots.FluentBuilders.Internal
{
    /// <summary>
    /// Provides the base functional interface for all Node types.
    /// </summary>
    internal abstract class TreeNode
    {
        protected TreeNode(RootNode root)
        {
            Root = root;
        }

        public virtual RootNode Root { get; }
        /// <summary>
        /// Whether or not this node was used during the construction of the top level object,
        /// this prevents it from being used twice on immutable objects.
        /// </summary>
        public bool UsedByConstructor { get; set; }

        /// <summary>
        /// Applies the node's configuration to the provided object.
        /// </summary>
        /// <param name="objectToBuild">The object to be modified.</param>
        public abstract void ApplyTo(object objectToBuild);
        
        
    }
}