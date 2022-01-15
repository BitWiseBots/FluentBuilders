using System;
using System.Collections.Generic;
using BitWiseBots.FluentBuilders.Interfaces;
using BitWiseBots.FluentBuilders.Internal;
using JetBrains.Annotations;

namespace BitWiseBots.FluentBuilders
{
    /// <summary>
    /// Provides the base functionality for registering constructors to be used by <see cref="Builders"/> when creating new <see cref="IBuilder{T}"/>.
    /// </summary>
    public abstract class BuilderRegistration : IHideObjectMembers
    {
        /// <summary>
        /// Store all the registrations in an <c>internal</c> collection to be collated by <see cref="Builders"/>
        /// </summary>
        internal readonly Dictionary<string, Delegate> BuilderRegistrations = new();

        /// <summary>
        /// Store all the post build registrations in an <c>internal</c> collection to be collated by <see cref="Builders"/>
        /// </summary>
        internal readonly Dictionary<string, Delegate> BuilderPostBuildRegistrations = new();

        /// <summary>
        /// Store all the type default registrations in an <c>internal</c> collection to be collated by <see cref="Builders"/>
        /// </summary>
        internal readonly Dictionary<string, Delegate> TypeDefaultRegistrations = new();

        /// <summary>
        /// Adds a constructor function for a type to the list of registrations.
        /// </summary>
        /// <typeparam name="T">The type of object to be constructed.</typeparam>
        /// <param name="constructorFunc">
        /// An expression that produces a new instance of <typeparamref name="T"/>.
        /// The expression is passed a <see cref="IConstructorBuilder{T}"/> that can be used to reference values set by <see cref="M:IBuilder{T}.With"/>.
        /// </param>
        [PublicAPI]
        protected void RegisterConstructor<T>(Func<IConstructorBuilder<T>, T> constructorFunc)
        {
            var registrationKey = typeof(T).GetRegistrationKey();
            if (BuilderRegistrations.ContainsKey(registrationKey))
            {
                throw new BuildConfigurationException($"A constructor func has already been registered for type '{registrationKey}'");
            }

            BuilderRegistrations[registrationKey] = constructorFunc;
        }

        /// <summary>
        /// Adds a post build action for a type to the list of registrations.
        /// </summary>
        /// <typeparam name="T">The type of object that was built.</typeparam>
        /// <param name="postBuildAction">
        /// An expression that performs additional work on the built <typeparamref name="T"/> instance after the builder is finished.
        /// </param>
        [PublicAPI]
        protected void RegisterPostBuildAction<T>(Action<T> postBuildAction)
        {
            var registrationKey = typeof(T).GetRegistrationKey();
            if (BuilderPostBuildRegistrations.ContainsKey(registrationKey))
            {
                throw new BuildConfigurationException($"A post build action has already been registered for type '{registrationKey}'");
            }

            BuilderPostBuildRegistrations[registrationKey] = postBuildAction;
        }

        /// <summary>
        /// Adds a type default function for a type to the list of registrations.
        /// </summary>
        /// <typeparam name="T">The type to add a default value function for.</typeparam>
        /// <param name="typeDefaultFunc">
        /// An expression that produces an instance of type <typeparamref name="T"/> to be used when a property of type <typeparamref name="T"/> doesn't have a value specified.
        /// </param>
        [PublicAPI]
        protected void RegisterTypeDefaultFunc<T>(Func<T> typeDefaultFunc)
        {
            var registrationKey = typeof(T).GetRegistrationKey();
            if (TypeDefaultRegistrations.ContainsKey(registrationKey))
            {
                throw new BuildConfigurationException($"A type default function has already been registered for type '{registrationKey}'");
            }

            TypeDefaultRegistrations[registrationKey] = typeDefaultFunc;
        }
    }
}
