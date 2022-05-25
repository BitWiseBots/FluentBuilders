using System;
using System.Collections.Generic;
using BitWiseBots.FluentBuilders.Interfaces;

namespace BitWiseBots.FluentBuilders.Internal
{
    /// <summary>
    /// Holds the various configs that have been added within the current process.
    /// </summary>
    internal class ConfigStore
    {
        public ConfigStore()
        {
            Constructors = new Dictionary<string, Delegate>();
            PostBuilds = new Dictionary<string, Delegate>();
            TypeDefaults = new Dictionary<string, Delegate>();
        }

        internal Dictionary<string, Delegate> Constructors { get; }
        internal Dictionary<string, Delegate> PostBuilds { get; }
        internal Dictionary<string, Delegate> TypeDefaults { get; }

        /// <summary>
        /// Attempts to get a constructor function for the immutable type being built
        /// </summary>
        /// <returns>Either a <see cref="Func{TConstructorBuilder,T}"/> if one was added, or <c>null</c> if not.</returns>
        public Func<IConstructorBuilder<T>,T> GetConstructor<T>()
        {
            var key = typeof(T).GetStoreKey();
            return Constructors.ContainsKey(key) ? (Func<IConstructorBuilder<T>, T>) Constructors[key] : null;
        }

        /// <summary>
        /// Attempts to get a post build action for the immutable type being built
        /// </summary>
        /// <returns>Either a <see cref="Action{T}"/> if one was added, or <c>null</c> if not.</returns>
        public Action<T> GetPostBuild<T>()
        {
            var key = typeof(T).GetStoreKey();
            return PostBuilds.ContainsKey(key) ? (Action<T>)PostBuilds[key] : null;
        }

        /// <summary>
        /// Attempts to get default value func for the Type being requested
        /// </summary>
        /// <returns>Either a <see cref="Func{T}"/> if one was added, or <c>null</c> if not.</returns>
        public Delegate GetTypeDefault(Type valueType)
        {
            var key = valueType.GetStoreKey();
            return TypeDefaults.ContainsKey(key) ? TypeDefaults[key] : null;
        }

        /// <summary>
        /// Adds a new constructor function for the provided store key;
        /// </summary>
        /// <param name="storeKey">The type key the constructor function is for.</param>
        /// <param name="constructorDelegate">A <see cref="Func{TResult}"/> that returns an instance of the type specified by the key.</param>
        public void AddConstructor(string storeKey, Delegate constructorDelegate)
        {
            if (Constructors.ContainsKey(storeKey))
            {
                throw new BuildConfigurationException($"A constructor func has already been added for type '{storeKey}'");
            }

            Constructors.Add(storeKey, constructorDelegate);
        }

        /// <summary>
        /// Adds a new post build action for the provided store key;
        /// </summary>
        /// <param name="storeKey">The type key the post build action is for.</param>
        /// <param name="constructorDelegate">A <see cref="Action{TResult}"/> that takes an instance of the type specified by the key.</param>
        public void AddPostBuild(string storeKey, Delegate constructorDelegate)
        {
            if (PostBuilds.ContainsKey(storeKey))
            {
                throw new BuildConfigurationException($"A post build action has already been added for type '{storeKey}'");
            }

            PostBuilds.Add(storeKey, constructorDelegate);
        }

        /// <summary>
        /// Adds a new  type default value function for the provided store key;
        /// </summary>
        /// <param name="storeKey">The type key the post build action is for.</param>
        /// <param name="typeDefaultDelegate">A <see cref="Func{TResult}"/> returns a value of the type specified by the key.</param>
        public void AddTypeDefault(string storeKey, Delegate typeDefaultDelegate)
        {
            if (TypeDefaults.ContainsKey(storeKey))
            {
                throw new BuildConfigurationException($"A post build action has already been added for type '{storeKey}'");
            }

            TypeDefaults.Add(storeKey, typeDefaultDelegate);
        }

        /// <summary>
        /// Clears all current configs.
        /// </summary>
        public void Clear()
        {
            Constructors.Clear();
            PostBuilds.Clear();
            TypeDefaults.Clear();
        }
    }
}
