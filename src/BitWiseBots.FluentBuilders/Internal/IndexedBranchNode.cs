﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BitWiseBots.FluentBuilders.Internal
{
    /// <summary>
    /// A node that represents an indexed property that has children.
    /// </summary>
    internal sealed class IndexedBranchNode : BranchNode
    {
        private readonly IndexExpression _property;
                
        /// <summary>
        /// Constructs a new <see cref="IndexedBranchNode"/>.
        /// </summary>
        /// <param name="property">The expressions that accesses the property this node represents.</param>
        public IndexedBranchNode(IndexExpression property, RootNode root) 
            : base(property.Indexer!.PropertyType, root)
        {
            _property = property;
        }

        /// <inheritdoc />
        protected override bool IsBuilderNode => !IsMutable;

        /// <inheritdoc />
        public override void ApplyTo(object objectToBuild)
        {
            if (UsedByConstructor) return;

            var memberInfo = _property.Indexer;
            var indexerArguments = _property.GetIndexerArguments();

            object currentValue = null;
            try
            {
                currentValue = memberInfo.GetValue(objectToBuild, indexerArguments);
            }
            catch
            {
                //No value set
            }

            var value = GetOrCreateValue(currentValue);
            ApplyToChildren(value);
            
            if (IsBuilderNode)
            {
                var objectType = objectToBuild.GetType();
                var genMethod = BuildBuilderMethod.MakeGenericMethod(objectType);
                var builderValue = genMethod.Invoke(this, new []{ value });
                memberInfo!.SetValue(objectToBuild, builderValue, indexerArguments);
            }
            else
            {                
                memberInfo!.SetValue(objectToBuild, value, indexerArguments);
            }            
        }
    }
}