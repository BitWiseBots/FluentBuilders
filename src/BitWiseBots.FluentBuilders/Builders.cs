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
    /// Creates new <see cref="Builder{T}"/> instances and manages the configs for constructing and configuring the constructed type.
    /// </summary>
    public static class Builders
    {
        private static readonly Lazy<ConfigStore> StoreInitializer = new();

        internal static ConfigStore ConfigStore => StoreInitializer.Value;

        /// <summary>
        /// Instantiates an instance of <typeparamref name="TBuilderConfig"/> and calls <see cref="AddConfig(BuilderConfig)"/>
        /// </summary>
        /// <typeparam name="TBuilderConfig">A type inheriting from <see cref="BuilderConfig"/>.</typeparam>
        [PublicAPI]
        public static void AddConfig<TBuilderConfig>() where TBuilderConfig : BuilderConfig, new()
        {
            AddConfig(new TBuilderConfig());
        }

        /// <summary>
        /// Instantiates an instance of <paramref name="configType"/> and calls <see cref="AddConfig(BuilderConfig)"/>
        /// </summary>
        /// <param name="configType">A type inheriting from <see cref="BuilderConfig"/>.</param>
        /// <exception cref="InvalidCastException">When the provided type does not inherit <see cref="BuilderConfig"/>.</exception>
        [PublicAPI]
        public static void AddConfig(Type configType)
        {
            AddConfig((BuilderConfig)Activator.CreateInstance(configType));
        }

        /// <summary>
        /// Stores the the provided <see cref="BuilderConfig"/> in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        [PublicAPI]
        public static void AddConfig(BuilderConfig builderConfig)
        {
            AppendConfigs(builderConfig);
        }

        /// <summary>
        /// Scans the provided assemblies for implementations of <see cref="BuilderConfig"/> and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="assembliesToScan">
        /// The assemblies to be scanned, as few as possible assemblies should be provided. IE. don't use <c>AppDomain.CurrentDomain.GetAssemblies()</c> or similar.
        /// </param>
        [PublicAPI]
        public static void AddConfigs(IEnumerable<Assembly> assembliesToScan)
        {
            AddConfigsFromAssemblies(assembliesToScan);
        }

        /// <summary>
        /// Scans the provided assemblies for implementations of <see cref="BuilderConfig"/>  and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="assembliesToScan">
        /// The assemblies to be scanned, as few as possible assemblies should be provided. IE. don't use <c>AppDomain.CurrentDomain.GetAssemblies()</c> or similar.
        /// </param>
        [PublicAPI]
        public static void AddConfigs(params Assembly[] assembliesToScan)
        {
            AddConfigsFromAssemblies(assembliesToScan);
        }

        /// <summary>
        /// Loads and scans the assemblies with the provided names for implementations of <see cref="BuilderConfig"/> and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="assemblyNamesToScan">
        /// The assemblies names to be loaded and scanned.
        /// </param>
        [PublicAPI]
        public static void AddConfigs(IEnumerable<string> assemblyNamesToScan)
        {
            AddConfigsFromAssemblies(assemblyNamesToScan.Select(Assembly.Load));
        }

        /// <summary>
        /// Loads and scans the assemblies with the provided names for implementations of <see cref="BuilderConfig"/> and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="assemblyNamesToScan">
        /// The assemblies names to be loaded and scanned.
        /// </param>
        [PublicAPI]
        public static void AddConfigs(params string[] assemblyNamesToScan)
        {
            AddConfigsFromAssemblies(assemblyNamesToScan.Select(Assembly.Load));
        }

        /// <summary>
        /// Scans the assemblies of the provided types for implementations of <see cref="BuilderConfig"/> and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="typesFromAssembliesContainingConfigs">
        /// The types whose assemblies should be scanned.
        /// </param>
        [PublicAPI]
        public static void AddConfigs(IEnumerable<Type> typesFromAssembliesContainingConfigs)
        {
            AddConfigsFromAssemblies(typesFromAssembliesContainingConfigs.Select(t => t.GetTypeInfo().Assembly));
        }

        /// <summary>
        /// Scans the assemblies of the provided types for implementations of <see cref="BuilderConfig"/> and stores them in a static collection to be provided to new <see cref="Builder{T}"/> instances.
        /// </summary>
        /// <param name="typesFromAssembliesContainingConfigs">
        /// The types whose assemblies should be scanned.
        /// </param>
        [PublicAPI]
        public static void AddConfigs(params Type[] typesFromAssembliesContainingConfigs)
        {
            AddConfigsFromAssemblies(typesFromAssembliesContainingConfigs.Select(t => t.GetTypeInfo().Assembly));
        }

        /// <summary>
        /// Creates a <see cref="Builder{T}"/> for the provided type, uses a Constructor Expression if one was added with <see cref="Builders"/>.
        /// </summary>
        /// <typeparam name="T">The type to be built.</typeparam>
        [PublicAPI]
        public static Builder<T> Create<T>( Func<IConstructorBuilder<T>,T> customConstructor = null, Action<T> customPostBuild = null)
        {
            return new Builder<T>(ConfigStore, customConstructor, customPostBuild);
        }

        /// <summary>
        /// Scans the provided assemblies for types inheriting from <see cref="BuilderConfig"/> and calls <see cref="AddConfig"/>.
        /// </summary>
        /// <param name="assembliesToScan">The assemblies to be scanned.</param>
        private static void AddConfigsFromAssemblies(IEnumerable<Assembly> assembliesToScan)
        {
            var allTypes = assembliesToScan.Where(a => !a.IsDynamic).SelectMany(a => a.DefinedTypes);

            var configs = allTypes.Where(t => typeof(BuilderConfig).GetTypeInfo().IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .Select(t => t.AsType());

            foreach (var config in configs)
            {
                AddConfig(config);
            }
        }

        /// <summary>
        /// Adds the provided configs to the <see cref="ConfigStore"/>.
        /// </summary>
        private static void AppendConfigs(BuilderConfig builderConfig)
        {
            foreach (var (key, value) in builderConfig.Constructors)
            {
                ConfigStore.AddConstructor(key, value);
            }

            foreach (var (key, value) in builderConfig.PostBuilds)
            {
                ConfigStore.AddPostBuild(key, value);
            }

            foreach (var (key, value) in builderConfig.TypeDefaults)
            {
                ConfigStore.AddTypeDefault(key, value);
            }
        }
    }
}