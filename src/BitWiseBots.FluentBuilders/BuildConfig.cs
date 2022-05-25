using System;
using System.Collections.Generic;
using BitWiseBots.FluentBuilders.Interfaces;
using BitWiseBots.FluentBuilders.Internal;
using JetBrains.Annotations;

namespace BitWiseBots.FluentBuilders
{
    /// <summary>
    /// Provides the base functionality for adding constructors, post builds, and type defaults to be used by <see cref="Builders"/> when creating new <see cref="Builder{T}"/>.
    /// </summary>
    public abstract class BuilderConfig : IHideObjectMembers
    {
        /// <summary>
        /// Store all the constructor functions in an <c>internal</c> collection to be collated by <see cref="Builders"/>
        /// </summary>
        internal readonly Dictionary<string, Delegate> Constructors = new();

        /// <summary>
        /// Store all the post build actions in an <c>internal</c> collection to be collated by <see cref="Builders"/>
        /// </summary>
        internal readonly Dictionary<string, Delegate> PostBuilds = new();

        /// <summary>
        /// Store all the type defaults in an <c>internal</c> collection to be collated by <see cref="Builders"/>
        /// </summary>
        internal readonly Dictionary<string, Delegate> TypeDefaults = new();

        /// <summary>
        /// Adds a constructor function for a type to the config store.
        /// </summary>
        /// <typeparam name="T">The type of object to be constructed.</typeparam>
        /// <param name="constructorFunc">
        /// An expression that produces a new instance of <typeparamref name="T"/>.
        /// The expression is passed a <see cref="IConstructorBuilder{T}"/> that can be used to reference values set by <see cref="M:IBuilder{T}.With"/>.
        /// </param>
        [PublicAPI]
        protected void AddConstructor<T>(Func<IConstructorBuilder<T>, T> constructorFunc)
        {
            var storeKey = typeof(T).GetStoreKey();
            if (Constructors.ContainsKey(storeKey))
            {
                throw new BuildConfigurationException($"A constructor func has already been added for type '{storeKey}'");
            }

            Constructors[storeKey] = constructorFunc;
        }

        /// <summary>
        /// Adds a post build action for a type to the config store.
        /// </summary>
        /// <typeparam name="T">The type of object that was built.</typeparam>
        /// <param name="postBuild">
        /// An expression that performs additional work on the built <typeparamref name="T"/> instance after the builder is finished.
        /// </param>
        [PublicAPI]
        protected void AddPostBuild<T>(Action<T> postBuild)
        {
            var storeKey = typeof(T).GetStoreKey();
            if (PostBuilds.ContainsKey(storeKey))
            {
                throw new BuildConfigurationException($"A post build action has already been added for type '{storeKey}'");
            }

            PostBuilds[storeKey] = postBuild;
        }

        /// <summary>
        /// Adds a type default function for a type to the config store.
        /// </summary>
        /// <typeparam name="T">The type to add a default value function for.</typeparam>
        /// <param name="typeDefaultFunc">
        /// An expression that produces an instance of type <typeparamref name="T"/> to be used when a property of type <typeparamref name="T"/> doesn't have a value specified.
        /// </param>
        [PublicAPI]
        protected void AddTypeDefault<T>(Func<T> typeDefaultFunc)
        {
            var storeKey = typeof(T).GetStoreKey();
            if (TypeDefaults.ContainsKey(storeKey))
            {
                throw new BuildConfigurationException($"A type default function has already been added for type '{storeKey}'");
            }

            TypeDefaults[storeKey] = typeDefaultFunc;
        }
    }
}
