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
            Builders.ConfigStore.Clear();
        }

        [Fact]
        public void Create_ShouldReturnNewBuilder_WhenNoConfigsExists()
        {
            var result = Builders.Create<TestableClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public void Create_ShouldReturnNewBuilder_WhenConstructorConfigExists()
        {
            Builders.AddConfig(new TestableBuilderConfig(true, false, false));

            var builder = Builders.Create<TestableClass>();
            var result = builder.Build();

            Assert.Equal("someString", result.Property);
        }

        [Fact]
        public void Create_ShouldReturnNewBuilder_WhenPostBuildConfigExists()
        {
            Builders.AddConfig(new TestableBuilderConfig(false, true, false));

            var builder = Builders.Create<TestableClass>();
            var result = builder.Build();

            Assert.Equal("someString", result.Property);
        }

        [Fact]
        public void Create_ShouldReturnNewBuilder_WhenTypeDefaultConfigExistsAndWithForTypeProvided()
        {
            Builders.AddConfig(new TestableBuilderConfig(false, false, true));

            var builder = Builders.Create<TestableClass>();
            var result = builder.With(b => b.GuidProperty).Build();

            Assert.NotEqual(Guid.Empty, result.GuidProperty);
        }

        [Fact]
        public void AddConfigT_ShouldAddConfigs()
        {
            Builders.AddConfig<TestableBuilderConfig>();

            Assert.Single(Builders.ConfigStore.Constructors);
            Assert.Single(Builders.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigType_ShouldAddBuilderConfigs()
        {
            Builders.AddConfig(typeof(TestableBuilderConfig));

            Assert.Single(Builders.ConfigStore.Constructors);
            Assert.Single(Builders.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfig_ShouldAddConfigs()
        {
            Builders.AddConfig(new TestableBuilderConfig());

            Assert.Single(Builders.ConfigStore.Constructors);
            Assert.Single(Builders.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigsIEnumerable_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddConfigs(new List<Assembly> { Assembly.GetAssembly(typeof(TestableBuilderConfig)) });

            Assert.Single(Builders.ConfigStore.Constructors);
            Assert.Single(Builders.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigsParams_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddConfigs(Assembly.GetAssembly(typeof(TestableBuilderConfig)));

            Assert.Single(Builders.ConfigStore.Constructors);
            Assert.Single(Builders.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigsTypeIEnumerable_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddConfigs(new List<Type> { typeof(TestableBuilderConfig) });

            Assert.Single(Builders.ConfigStore.Constructors);
            Assert.Single(Builders.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigsTypeParams_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddConfigs(typeof(TestableBuilderConfig));

            Assert.Single(Builders.ConfigStore.Constructors);
            Assert.Single(Builders.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigs_ShouldUseAssemblyQualifiedNameOfType()
        {
            Builders.AddConfigs(typeof(TestableBuilderConfig));

            Assert.Single(Builders.ConfigStore.Constructors.Keys, typeof(TestableClass).AssemblyQualifiedName);
        }

        [Fact]
        public void AddConfigsStringIEnumerable_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddConfigs(new List<string> { "BitWiseBots.FluentBuilders.Tests.Unit" });

            Assert.Single(Builders.ConfigStore.Constructors);
            Assert.Single(Builders.ConfigStore.PostBuilds);
        }

        [Fact]
        public void AddConfigsStringParams_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddConfigs("BitWiseBots.FluentBuilders.Tests.Unit");

            Assert.Single(Builders.ConfigStore.Constructors);
            Assert.Single(Builders.ConfigStore.PostBuilds);
        }

        [Fact]
        public void GetConstructor_ShouldReturnFunc_WhenConfigExists()
        {
            Builders.AddConfig(new TestableBuilderConfig(true, false, false));

            var result = Builders.ConfigStore.GetConstructor<TestableClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public void GetConstructor_ShouldReturnNull_WhenConfigDoesNotExist()
        {
            var result = Builders.ConfigStore.GetConstructor<TestableClass>();

            Assert.Null(result);
        }

        [Fact]
        public void GetPostBuild_ShouldReturnAction_WhenConfigExists()
        {
            Builders.AddConfig(new TestableBuilderConfig(false, true, false));

            var result = Builders.ConfigStore.GetPostBuild<TestableClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public void GetPostBuild_ShouldReturnNull_WhenConfigDoesNotExist()
        {
            var result = Builders.ConfigStore.GetPostBuild<TestableClass>();

            Assert.Null(result);
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenSingleConfigAddsMultipleConstructorsForSameType()
        {
            Assert.Throws<BuildConfigurationException>(() => Builders.AddConfig(new BadTestableBuilderConfig(true, false, false)));
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenSingleConfigAddsMultiplePostBuildActionsForSameType()
        {
            Assert.Throws<BuildConfigurationException>(() => Builders.AddConfig(new BadTestableBuilderConfig(false, true, false)));
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenSingleConfigAddsMultipleTypeDefaultsForSameType()
        {
            Assert.Throws<BuildConfigurationException>(() => Builders.AddConfig(new BadTestableBuilderConfig(false, false, true)));
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenMultipleConfigsAddsSameConstructors()
        {
            Builders.AddConfig(new TestableBuilderConfig(true, false, false));
            Assert.Throws<BuildConfigurationException>(() => Builders.AddConfig(new TestableBuilderConfig(true, false, false)));
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenMultipleConfigsAddsSamePostBuildActions()
        {
            Builders.AddConfig(new TestableBuilderConfig(false, true, false));
            Assert.Throws<BuildConfigurationException>(() => Builders.AddConfig(new TestableBuilderConfig(false, true, false)));
        }

        [Fact]
        public void AddConfig_ShouldThrowError_WhenMultipleConfigsAddsSameTypeDefaults()
        {
            Builders.AddConfig(new TestableBuilderConfig(false, false, true));
            Assert.Throws<BuildConfigurationException>(() => Builders.AddConfig(new TestableBuilderConfig(false, false, true)));
        }

        private class TestableBuilderConfig : BuildConfig
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

        private class BadTestableBuilderConfig : BuildConfig
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