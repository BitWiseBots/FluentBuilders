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
            Builders.BuilderRegistrationStore.ClearRegistrations();
        }

        [Fact]
        public void Create_ShouldReturnNewBuilder_WhenNoRegistrationExists()
        {
            var result = Builders.Create<TestableClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public void Create_ShouldReturnNewBuilder_WhenConstructorRegistrationExists()
        {
            Builders.AddBuilderRegistration(new TestableBuilderRegistration(true, false));

            var builder = Builders.Create<TestableClass>();
            var result = builder.Build();

            Assert.Equal("someString", result.Property);
        }

        [Fact]
        public void Create_ShouldReturnNewBuilder_WhenPostBuildRegistrationExists()
        {
            Builders.AddBuilderRegistration(new TestableBuilderRegistration(false, true));

            var builder = Builders.Create<TestableClass>();
            var result = builder.Build();

            Assert.Equal("someString", result.Property);
        }

        [Fact]
        public void AddBuilderRegistrationT_ShouldAddBuilderRegistrations()
        {
            Builders.AddBuilderRegistration<TestableBuilderRegistration>();

            Assert.Single(Builders.BuilderRegistrationStore.ConstructorRegistrations);
            Assert.Single(Builders.BuilderRegistrationStore.PostBuildRegistrations);
        }

        [Fact]
        public void AddBuilderRegistrationType_ShouldAddBuilderRegistrations()
        {
            Builders.AddBuilderRegistration(typeof(TestableBuilderRegistration));

            Assert.Single(Builders.BuilderRegistrationStore.ConstructorRegistrations);
            Assert.Single(Builders.BuilderRegistrationStore.PostBuildRegistrations);
        }

        [Fact]
        public void AddBuilderRegistration_ShouldAddBuilderRegistration()
        {
            Builders.AddBuilderRegistration(new TestableBuilderRegistration());

            Assert.Single(Builders.BuilderRegistrationStore.ConstructorRegistrations);
            Assert.Single(Builders.BuilderRegistrationStore.PostBuildRegistrations);
        }

        [Fact]
        public void AddBuilderRegistrationsAssemblyIEnumerable_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddBuilderRegistrations(new List<Assembly> { Assembly.GetAssembly(typeof(TestableBuilderRegistration)) });

            Assert.Single(Builders.BuilderRegistrationStore.ConstructorRegistrations);
            Assert.Single(Builders.BuilderRegistrationStore.PostBuildRegistrations);
        }

        [Fact]
        public void AddBuilderRegistrationsAssemblyParams_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddBuilderRegistrations(Assembly.GetAssembly(typeof(TestableBuilderRegistration)));

            Assert.Single(Builders.BuilderRegistrationStore.ConstructorRegistrations);
            Assert.Single(Builders.BuilderRegistrationStore.PostBuildRegistrations);
        }

        [Fact]
        public void AddBuilderRegistrationsTypeIEnumerable_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddBuilderRegistrations(new List<Type> { typeof(TestableBuilderRegistration) });

            Assert.Single(Builders.BuilderRegistrationStore.ConstructorRegistrations);
            Assert.Single(Builders.BuilderRegistrationStore.PostBuildRegistrations);
        }

        [Fact]
        public void AddBuilderRegistrationsTypeParams_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddBuilderRegistrations(typeof(TestableBuilderRegistration));

            Assert.Single(Builders.BuilderRegistrationStore.ConstructorRegistrations);
            Assert.Single(Builders.BuilderRegistrationStore.PostBuildRegistrations);
        }

        [Fact]
        public void AddBuilderRegistrations_ShouldUseAssemblyQualifiedNameOfType()
        {
            Builders.AddBuilderRegistrations(typeof(TestableBuilderRegistration));

            Assert.Single(Builders.BuilderRegistrationStore.ConstructorRegistrations.Keys, typeof(TestableClass).AssemblyQualifiedName);
        }

        [Fact]
        public void AddBuilderRegistrationsStringIEnumerable_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddBuilderRegistrations(new List<string> { "BitWiseBots.FluentBuilders.Tests.Unit" });

            Assert.Single(Builders.BuilderRegistrationStore.ConstructorRegistrations);
            Assert.Single(Builders.BuilderRegistrationStore.PostBuildRegistrations);
        }

        [Fact]
        public void AddBuilderRegistrationsStringParams_ShouldFindImplementationInAssemblyAndExecute()
        {
            Builders.AddBuilderRegistrations("BitWiseBots.FluentBuilders.Tests.Unit");

            Assert.Single(Builders.BuilderRegistrationStore.ConstructorRegistrations);
            Assert.Single(Builders.BuilderRegistrationStore.PostBuildRegistrations);
        }

        [Fact]
        public void GetConstructorFunc_ShouldReturnFunc_WhenRegistrationExists()
        {
            Builders.AddBuilderRegistration(new TestableBuilderRegistration(true, false));

            var result = Builders.BuilderRegistrationStore.GetConstructorFunc<TestableClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public void GetConstructorFunc_ShouldReturnNull_WhenRegistrationDoesNotExist()
        {
            var result = Builders.BuilderRegistrationStore.GetConstructorFunc<TestableClass>();

            Assert.Null(result);
        }

        [Fact]
        public void GetPostBuildAction_ShouldReturnAction_WhenRegistrationExists()
        {
            Builders.AddBuilderRegistration(new TestableBuilderRegistration(false, true));

            var result = Builders.BuilderRegistrationStore.GetPostBuildAction<TestableClass>();

            Assert.NotNull(result);
        }

        [Fact]
        public void GetPostBuildAction_ShouldReturnNull_WhenRegistrationDoesNotExist()
        {
            var result = Builders.BuilderRegistrationStore.GetPostBuildAction<TestableClass>();

            Assert.Null(result);
        }

        [Fact]
        public void AddBuilderRegistration_ShouldThrowError_WhenSingleRegistrationAddsMultipleConstructorRegistrationsForSameType()
        {
            Assert.Throws<BuildConfigurationException>(() => Builders.AddBuilderRegistration(new BadTestableBuilderRegistration(true, false)));
        }

        [Fact]
        public void AddBuilderRegistration_ShouldThrowError_WhenSingleRegistrationAddsMultiplePostBuildActionRegistrationsForSameType()
        {
            Assert.Throws<BuildConfigurationException>(() => Builders.AddBuilderRegistration(new BadTestableBuilderRegistration(false, true)));
        }

        [Fact]
        public void AddBuilderRegistration_ShouldThrowError_WhenMultipleRegistrationAddsSameConstructorRegistrations()
        {
            Builders.AddBuilderRegistration(new TestableBuilderRegistration(true, false));
            Assert.Throws<BuildConfigurationException>(() => Builders.AddBuilderRegistration(new TestableBuilderRegistration(true, false)));
        }

        [Fact]
        public void AddBuilderRegistration_ShouldThrowError_WhenMultipleRegistrationAddsSamePostBuildActionRegistrations()
        {
            Builders.AddBuilderRegistration(new TestableBuilderRegistration(false, true));
            Assert.Throws<BuildConfigurationException>(() => Builders.AddBuilderRegistration(new TestableBuilderRegistration(false, true)));
        }

        private class TestableBuilderRegistration : BuilderRegistration
        {
            public TestableBuilderRegistration() : this(true, true)
            {
            }

            public TestableBuilderRegistration(bool shouldRegisterConstructor, bool shouldRegisterPostBuild)
            {
                if (shouldRegisterConstructor)
                {
                    RegisterConstructor<TestableClass>(b => new TestableClass{Property = "someString"});
                }

                if (shouldRegisterPostBuild)
                {
                    RegisterPostBuildAction<TestableClass>(c => c.Property = "someString");
                }
            }
        }

        private class BadTestableBuilderRegistration : BuilderRegistration
        {
            // This constructor gets used via reflection
            // ReSharper disable once UnusedMember.Local
            public BadTestableBuilderRegistration() : this(false, false)
            {
            }

            public BadTestableBuilderRegistration(bool shouldRegisterConstructor, bool shouldRegisterPostBuild)
            {
                if (shouldRegisterConstructor)
                {
                    RegisterConstructor<TestableClass>(b => new TestableClass{Property = "someString"});
                    RegisterConstructor<TestableClass>(b => new TestableClass{Property = "someString"});
                }

                if (shouldRegisterPostBuild)
                {
                    RegisterPostBuildAction<TestableClass>(c => c.Property = "someString");
                    RegisterPostBuildAction<TestableClass>(c => c.Property = "someString");
                }
            }
        }

        private class TestableClass
        {
            public string Property { get; set; }
        }
    }
}