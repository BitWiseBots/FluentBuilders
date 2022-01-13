using System;
using System.Collections.Generic;
using BitWiseBots.FluentBuilders.Interfaces;

namespace BitWiseBots.FluentBuilders.Internal
{
    /// <summary>
    /// Holds the various registrations that have been made within the current process.
    /// </summary>
    internal class BuilderRegistrationStore
    {
        public BuilderRegistrationStore()
        {
            ConstructorRegistrations = new Dictionary<string, Delegate>();
            PostBuildRegistrations = new Dictionary<string, Delegate>();
            TypeDefaultRegistrations = new Dictionary<string, Delegate>();
        }

        internal Dictionary<string, Delegate> ConstructorRegistrations { get; }
        internal Dictionary<string, Delegate> PostBuildRegistrations { get; }
        internal Dictionary<string, Delegate> TypeDefaultRegistrations { get; }

        /// <summary>
        /// Attempts to get a constructor function for the immutable type being built
        /// </summary>
        /// <returns>Either a <see cref="Func{TConstructorBuilder,T}"/> if one was registered, or <c>null</c> if not.</returns>
        public Func<IConstructorBuilder<T>,T> GetConstructorFunc<T>()
        {
            var key = typeof(T).GetRegistrationKey();
            return ConstructorRegistrations.ContainsKey(key) ? (Func<IConstructorBuilder<T>, T>) ConstructorRegistrations[key] : null;
        }

        /// <summary>
        /// Attempts to get a post build action for the immutable type being built
        /// </summary>
        /// <returns>Either a <see cref="Action{T}"/> if one was registered, or <c>null</c> if not.</returns>
        public Action<T> GetPostBuildAction<T>()
        {
            var key = typeof(T).GetRegistrationKey();
            return PostBuildRegistrations.ContainsKey(key) ? (Action<T>)PostBuildRegistrations[key] : null;
        }

        /// <summary>
        /// Attempts to get default value func for the Type being requested
        /// </summary>
        /// <returns>Either a <see cref="Func{T}"/> if one was registered, or <c>null</c> if not.</returns>
        public Delegate GetTypeDefaultFunc(Type valueType)
        {
            var key = valueType.GetRegistrationKey();
            return TypeDefaultRegistrations.ContainsKey(key) ? TypeDefaultRegistrations[key] : null;
        }

        /// <summary>
        /// Adds a new registration of a constructor function for the provided registration key;
        /// </summary>
        /// <param name="registrationKey">The type key the constructor function is for.</param>
        /// <param name="constructorDelegate">A <see cref="Func{TResult}"/> that returns an instance of the type specified by the key.</param>
        public void AddConstructorRegistration(string registrationKey, Delegate constructorDelegate)
        {
            if (ConstructorRegistrations.ContainsKey(registrationKey))
            {
                throw new BuildConfigurationException($"A constructor func has already been registered for type '{registrationKey}'");
            }

            ConstructorRegistrations.Add(registrationKey, constructorDelegate);
        }

        /// <summary>
        /// Adds a new registration of a post build action for the provided registration key;
        /// </summary>
        /// <param name="registrationKey">The type key the post build action is for.</param>
        /// <param name="constructorDelegate">A <see cref="Action{TResult}"/> that takes an instance of the type specified by the key.</param>
        public void AddPostBuildRegistration(string registrationKey, Delegate constructorDelegate)
        {
            if (PostBuildRegistrations.ContainsKey(registrationKey))
            {
                throw new BuildConfigurationException($"A post build action has already been registered for type '{registrationKey}'");
            }

            PostBuildRegistrations.Add(registrationKey, constructorDelegate);
        }

        /// <summary>
        /// Adds a new registration of a type default value function for the provided registration key;
        /// </summary>
        /// <param name="registrationKey">The type key the post build action is for.</param>
        /// <param name="typeDefaultDelegate">A <see cref="Func{TResult}"/> returns a value of the type specified by the key.</param>
        public void AddTypeDefaultRegistration(string registrationKey, Delegate typeDefaultDelegate)
        {
            if (TypeDefaultRegistrations.ContainsKey(registrationKey))
            {
                throw new BuildConfigurationException($"A post build action has already been registered for type '{registrationKey}'");
            }

            TypeDefaultRegistrations.Add(registrationKey, typeDefaultDelegate);
        }

        /// <summary>
        /// Clears all current registrations.
        /// </summary>
        public void ClearRegistrations()
        {
            ConstructorRegistrations.Clear();
            PostBuildRegistrations.Clear();
            TypeDefaultRegistrations.Clear();
        }
    }
}
