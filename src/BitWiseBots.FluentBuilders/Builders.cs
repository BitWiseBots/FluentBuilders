using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BitWiseBots.FluentBuilders.Interfaces;
using BitWiseBots.FluentBuilders.Internal;
using JetBrains.Annotations;

namespace BitWiseBots.FluentBuilders
{
    /// <summary>
    /// Creates new <see cref="Builder{T}"/> instances and manages the registrations for constructing and configuring the constructed type.
    /// </summary>
    public static class Builders
    {
        private static readonly Lazy<BuilderRegistrationStore> StoreInitializer = new();

        internal static BuilderRegistrationStore BuilderRegistrationStore => StoreInitializer.Value;

        /// <summary>
        /// Instantiates an instance of <typeparamref name="TBuilderRegistration"/> and calls <see cref="AddBuilderRegistration(BuilderRegistration)"/>
        /// </summary>
        /// <typeparam name="TBuilderRegistration">A type inheriting from <see cref="BuilderRegistration"/>.</typeparam>
        [PublicAPI]
        public static void AddBuilderRegistration<TBuilderRegistration>() where TBuilderRegistration : BuilderRegistration, new()
        {
            AddBuilderRegistration(new TBuilderRegistration());
        }

        /// <summary>
        /// Instantiates an instance of <paramref name="builderRegistrationType"/> and calls <see cref="AddBuilderRegistration(BuilderRegistration)"/>
        /// </summary>
        /// <param name="builderRegistrationType">A type inheriting from <see cref="BuilderRegistration"/>.</param>
        /// <exception cref="InvalidCastException">When the provided type does not inherit <see cref="BuilderRegistration"/>.</exception>
        [PublicAPI]
        public static void AddBuilderRegistration(Type builderRegistrationType)
        {
            AddBuilderRegistration((BuilderRegistration)Activator.CreateInstance(builderRegistrationType));
        }

        /// <summary>
        /// Retrieves the <see cref="BuilderRegistration.BuilderRegistrations"/> for the provided <see cref="BuilderRegistration"/> and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        [PublicAPI]
        public static void AddBuilderRegistration(BuilderRegistration builderRegistration)
        {
            AppendBuilderRegistrations(builderRegistration);
        }

        /// <summary>
        /// Scans the provided assemblies for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/>  and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="assembliesToScan">
        /// The assemblies to be scanned, as few as possible assemblies should be provided. IE. don't use <c>AppDomain.CurrentDomain.GetAssemblies()</c> or similar.
        /// </param>
        [PublicAPI]
        public static void AddBuilderRegistrations(IEnumerable<Assembly> assembliesToScan)
        {
            AddBuilderRegistrationsFromAssemblies(assembliesToScan);
        }

        /// <summary>
        /// Scans the provided assemblies for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/>  and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="assembliesToScan">
        /// The assemblies to be scanned, as few as possible assemblies should be provided. IE. don't use <c>AppDomain.CurrentDomain.GetAssemblies()</c> or similar.
        /// </param>
        [PublicAPI]
        public static void AddBuilderRegistrations(params Assembly[] assembliesToScan)
        {
            AddBuilderRegistrationsFromAssemblies(assembliesToScan);
        }

        /// <summary>
        /// Loads and scans the assemblies with the provided names for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/>  and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="assemblyNamesToScan">
        /// The assemblies names to be loaded and scanned.
        /// </param>
        [PublicAPI]
        public static void AddBuilderRegistrations(IEnumerable<string> assemblyNamesToScan)
        {
            AddBuilderRegistrationsFromAssemblies(assemblyNamesToScan.Select(Assembly.Load));
        }

        /// <summary>
        /// Loads and scans the assemblies with the provided names for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/>  and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="assemblyNamesToScan">
        /// The assemblies names to be loaded and scanned.
        /// </param>
        [PublicAPI]
        public static void AddBuilderRegistrations(params string[] assemblyNamesToScan)
        {
            AddBuilderRegistrationsFromAssemblies(assemblyNamesToScan.Select(Assembly.Load));
        }

        /// <summary>
        /// Scans the assemblies of the provided types for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/>  and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="typesFromAssembliesContainingRegistrations">
        /// The types whose assemblies should be scanned.
        /// </param>
        [PublicAPI]
        public static void AddBuilderRegistrations(IEnumerable<Type> typesFromAssembliesContainingRegistrations)
        {
            AddBuilderRegistrationsFromAssemblies(typesFromAssembliesContainingRegistrations.Select(t => t.GetTypeInfo().Assembly));
        }

        /// <summary>
        /// Scans the assemblies of the provided types for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/>  and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="typesFromAssembliesContainingRegistrations">
        /// The types whose assemblies should be scanned.
        /// </param>
        [PublicAPI]
        public static void AddBuilderRegistrations(params Type[] typesFromAssembliesContainingRegistrations)
        {
            AddBuilderRegistrationsFromAssemblies(typesFromAssembliesContainingRegistrations.Select(t => t.GetTypeInfo().Assembly));
        }

        /// <summary>
        /// Creates a <see cref="Builder{T}"/> for the provided type, uses a Constructor Expression if one was registered with <see cref="Builders"/>.
        /// </summary>
        /// <typeparam name="T">The type to be built.</typeparam>
        [PublicAPI]
        public static Builder<T> Create<T>( Func<IConstructorBuilder<T>,T> customConstructorFunc = null, Action<T> customPostBuildAction = null)
        {
            return new Builder<T>(BuilderRegistrationStore, customConstructorFunc, customPostBuildAction);
        }

        /// <summary>
        /// Scans the provided assemblies for types inheriting from <see cref="BuilderRegistration"/> and calls <see cref="AddBuilderRegistration(Type)"/>.
        /// </summary>
        /// <param name="assembliesToScan">The assemblies to be scanned.</param>
        private static void AddBuilderRegistrationsFromAssemblies(IEnumerable<Assembly> assembliesToScan)
        {
            var allTypes = assembliesToScan.Where(a => !a.IsDynamic).SelectMany(a => a.DefinedTypes);

            var registrations = allTypes.Where(t => typeof(BuilderRegistration).GetTypeInfo().IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .Select(t => t.AsType());

            foreach (var registration in registrations)
            {
                AddBuilderRegistration(registration);
            }
        }

        /// <summary>
        /// Adds the provides registrations to the <see cref="BuilderRegistrationStore"/>.
        /// </summary>
        private static void AppendBuilderRegistrations(BuilderRegistration builderRegistration)
        {
            foreach (var (key, value) in builderRegistration.BuilderRegistrations)
            {
                BuilderRegistrationStore.AddConstructorRegistration(key, value);
            }

            foreach (var (key, value) in builderRegistration.BuilderPostBuildRegistrations)
            {
                BuilderRegistrationStore.AddPostBuildRegistration(key, value);
            }

            foreach (var (key, value) in builderRegistration.TypeDefaultRegistrations)
            {
                BuilderRegistrationStore.AddTypeDefaultRegistration(key, value);
            }
        }
    }
}