using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace BitWiseBots.FluentBuilders.Tests.Unit
{
    public class BuildersFixture
    {
        public BuildersFixture()
        {
            BuilderFactory.ConfigStore.Clear();
        }

        [Fact]
        public void Create_ShouldReturnNewBuilder_WhenNoConfigsExists()
        {
            var result = BuilderFactory.Create<TestableClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public void Create_ShouldReturnNewBuilder_WhenConstructorConfigExists()
        {
            BuilderFactory.AddConfig(new TestableBuilderConfig(true, false, false));

            var builder = BuilderFactory.Create<TestableClass>();
            var result = builder.Build();

            Assert.Equal("someString", result.Property);
        }

        [Fact]
        public void Create_ShouldReturnNewBuilder_WhenPostBuildConfigExists()
        {
            BuilderFactory.AddConfig(new TestableBuilderConfig(false, true, false));

            var builder = BuilderFactory.Create<TestableClass>();
            var result = builder.Build();

            Assert.Equal("someString", result.Property);
        }

        [Fact]
        public void Create_ShouldReturnNewBuilder_WhenTypeDefaultConfigExistsAndWithForTypeProvided()
        {
            BuilderFactory.AddConfig(new TestableBuilderConfig(false, false, true));

            var builder = BuilderFactory.Create<TestableClass>();
            var result = builder.With(b => b.GuidProperty).Build();

            Assert.NotEqual(Guid.Empty, result.GuidProperty);
        }

        [Fact]
        public void AddConfigT_ShouldAddConfigs()
        {
            BuilderFactory.AddConfig<TestableBuilderConfig>();

            Assert.Single(BuilderFactory.ConfigStore.Constructors);
            Assert.Single(BuilderFactory.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigType_ShouldAddBuilderConfigs()
        {
            BuilderFactory.AddConfig(typeof(TestableBuilderConfig));

            Assert.Single(BuilderFactory.ConfigStore.Constructors);
            Assert.Single(BuilderFactory.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfig_ShouldAddConfigs()
        {
            BuilderFactory.AddConfig(new TestableBuilderConfig());

            Assert.Single(BuilderFactory.ConfigStore.Constructors);
            Assert.Single(BuilderFactory.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigsIEnumerable_ShouldFindImplementationInAssemblyAndExecute()
        {
            BuilderFactory.AddConfigs(new List<Assembly> { Assembly.GetAssembly(typeof(TestableBuilderConfig)) });

            Assert.Single(BuilderFactory.ConfigStore.Constructors);
            Assert.Single(BuilderFactory.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigsParams_ShouldFindImplementationInAssemblyAndExecute()
        {
            BuilderFactory.AddConfigs(Assembly.GetAssembly(typeof(TestableBuilderConfig)));

            Assert.Single(BuilderFactory.ConfigStore.Constructors);
            Assert.Single(BuilderFactory.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigsTypeIEnumerable_ShouldFindImplementationInAssemblyAndExecute()
        {
            BuilderFactory.AddConfigs(new List<Type> { typeof(TestableBuilderConfig) });

            Assert.Single(BuilderFactory.ConfigStore.Constructors);
            Assert.Single(BuilderFactory.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigsTypeParams_ShouldFindImplementationInAssemblyAndExecute()
        {
            BuilderFactory.AddConfigs(typeof(TestableBuilderConfig));

            Assert.Single(BuilderFactory.ConfigStore.Constructors);
            Assert.Single(BuilderFactory.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigs_ShouldUseAssemblyQualifiedNameOfType()
        {
            BuilderFactory.AddConfigs(typeof(TestableBuilderConfig));

            Assert.Single(BuilderFactory.ConfigStore.Constructors.Keys, typeof(TestableClass).AssemblyQualifiedName);
        }

        [Fact]
        public void AddConfigsStringIEnumerable_ShouldFindImplementationInAssemblyAndExecute()
        {
            BuilderFactory.AddConfigs(new List<string> { "BitWiseBots.FluentBuilders.Tests.Unit" });

            Assert.Single(BuilderFactory.ConfigStore.Constructors);
            Assert.Single(BuilderFactory.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigsStringParams_ShouldFindImplementationInAssemblyAndExecute()
        {
            BuilderFactory.AddConfigs("BitWiseBots.FluentBuilders.Tests.Unit");

            Assert.Single(BuilderFactory.ConfigStore.Constructors);
            Assert.Single(BuilderFactory.ConfigStore.PostBuilds);
        }

        [Fact]
        public void GetConstructor_ShouldReturnFunc_WhenConfigExists()
        {
            BuilderFactory.AddConfig(new TestableBuilderConfig(true, false, false));

            var result = BuilderFactory.ConfigStore.GetConstructor<TestableClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public void GetConstructor_ShouldReturnNull_WhenConfigDoesNotExist()
        {
            var result = BuilderFactory.ConfigStore.GetConstructor<TestableClass>();

            Assert.Null(result);
        }

        [Fact]
        public void GetPostBuild_ShouldReturnAction_WhenConfigExists()
        {
            BuilderFactory.AddConfig(new TestableBuilderConfig(false, true, false));

            var result = BuilderFactory.ConfigStore.GetPostBuild<TestableClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public void GetPostBuild_ShouldReturnNull_WhenConfigDoesNotExist()
        {
            var result = BuilderFactory.ConfigStore.GetPostBuild<TestableClass>();

            Assert.Null(result);
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenSingleConfigAddsMultipleConstructorsForSameType()
        {
            Assert.Throws<BuildConfigurationException>(() => BuilderFactory.AddConfig(new BadTestableBuilderConfig(true, false, false)));
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenSingleConfigAddsMultiplePostBuildActionsForSameType()
        {
            Assert.Throws<BuildConfigurationException>(() => BuilderFactory.AddConfig(new BadTestableBuilderConfig(false, true, false)));
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenSingleConfigAddsMultipleTypeDefaultsForSameType()
        {
            Assert.Throws<BuildConfigurationException>(() => BuilderFactory.AddConfig(new BadTestableBuilderConfig(false, false, true)));
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenMultipleConfigsAddsSameConstructors()
        {
            BuilderFactory.AddConfig(new TestableBuilderConfig(true, false, false));
            Assert.Throws<BuildConfigurationException>(() => BuilderFactory.AddConfig(new TestableBuilderConfig(true, false, false)));
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenMultipleConfigsAddsSamePostBuildActions()
        {
            BuilderFactory.AddConfig(new TestableBuilderConfig(false, true, false));
            Assert.Throws<BuildConfigurationException>(() => BuilderFactory.AddConfig(new TestableBuilderConfig(false, true, false)));
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenMultipleConfigsAddsSameTypeDefaults()
        {
            BuilderFactory.AddConfig(new TestableBuilderConfig(false, false, true));
            Assert.Throws<BuildConfigurationException>(() => BuilderFactory.AddConfig(new TestableBuilderConfig(false, false, true)));
        }

        private class TestableBuilderConfig : BuilderConfig
        {
            public TestableBuilderConfig() : this(true, true, true)
            {
            }

            public TestableBuilderConfig(bool shouldAddConstructor, bool shouldAddPostBuild, bool shouldAddTypeDefaults)
            {
                if (shouldAddConstructor)
                {
                    AddConstructor<TestableClass>(b => new TestableClass{Property = "someString"});
                }

                if (shouldAddPostBuild)
                {
                    AddPostBuild<TestableClass>(c => c.Property = "someString");
                }

                if (shouldAddTypeDefaults)
                {
                    AddTypeDefault(Guid.NewGuid);
                }
            }
        }

        private class BadTestableBuilderConfig : BuilderConfig
        {
            // This constructor gets used via reflection
            // ReSharper disable once UnusedMember.Local
            public BadTestableBuilderConfig() : this(false, false, false)
            {
            }

            public BadTestableBuilderConfig(bool shouldAddConstructor, bool shouldAddPostBuild, bool shouldAddTypeDefaults)
            {
                if (shouldAddConstructor)
                {
                    AddConstructor<TestableClass>(b => new TestableClass{Property = "someString"});
                    AddConstructor<TestableClass>(b => new TestableClass{Property = "someString"});
                }

                if (shouldAddPostBuild)
                {
                    AddPostBuild<TestableClass>(c => c.Property = "someString");
                    AddPostBuild<TestableClass>(c => c.Property = "someString");
                }

                if (shouldAddTypeDefaults)
                {
                    AddTypeDefault(() => new TestableClass());
                    AddTypeDefault(() => new TestableClass());
                }
            }
        }

        private class TestableClass
        {
            public string Property { get; set; }

            public Guid GuidProperty { get; set; }
        }
    }
}