using System;
using System.Collections.Generic;
using System.Linq;
using BitWiseBots.FluentBuilders.Interfaces;
using BitWiseBots.FluentBuilders.Internal;
using Xunit;

// The following rules are disabled as a result of items only used via reflection.
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable MemberCanBeProtected.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace BitWiseBots.FluentBuilders.Tests.Unit.Internal
{
	public class BuilderFixture
    {
        private readonly ConfigStore _configStore = new ConfigStore();
        
        internal void AddConstructor<T>(Func<IConstructorBuilder<T>,T> func)
        {
            _configStore.AddConstructor(typeof(T).GetStoreKey(), func);
        }

        internal void AddPostBuild<T>(Action<T> action)
        {
            _configStore.AddPostBuild(typeof(T).GetStoreKey(), action);
        }

        internal void AddTypeDefault<T>(Func<T> func)
        {
            _configStore.AddTypeDefault(typeof(T).GetStoreKey(), func);
        }

        internal Builder<T> Create<T>(Func<IConstructorBuilder<T>, T> customConstructorFunc = null, Action<T> customPostBuild = null)
        {
            return new Builder<T>(_configStore, customConstructorFunc, customPostBuild);
        }

#region Mutable With Tests

        [Fact]
        public void WithValue_ShouldAllowValueToBeSet_WhenBuildIsCalled()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.SingleProperty, "someValue");

            var result = builder.Build();

            Assert.Equal("someValue", result.SingleProperty);
        }

        [Fact]
        public void WithValue_ShouldAllowPrivateValueToBeSet_WhenBuildIsCalled()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.PrivateSingleProperty, "someValue");

            var result = builder.Build();

            Assert.Equal("someValue", result.PrivateSingleProperty);
        }

        [Fact]
        public void WithValue_ShouldThrowNotSupportedException_WhenMethodCallSpecified()
        {
            var builder = Create<TestableMutableObject>();

            var ex = Assert.Throws<NotSupportedException>(() => builder.With(o => o.Method(), builder));

            Assert.Equal("The provided expression contains an expression node type that is not supported: \n\tNode Type:\tCall\n\tBody:\t\to.Method().\nEnsure that the expression only contains property accessors.", ex.Message);
        }

        [Fact]
        public void WithValue_ShouldAllowValueToBeSet_WhenIndexedProperty()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o["someString"], Create<TestableMutableObject>().With(o => o.SingleProperty, "someValue"));

            var result = builder.Build();

            Assert.Equal("someValue",result["someString"].SingleProperty);
        }

	    [Fact]
	    public void WithValue_ShouldAllowValueToBeSet_WhenIndexedPropertyAndKeyIsVariable()
	    {
		    var builder = Create<TestableMutableObject>();
		    var key = "someString";

		    builder.With(o => o[key],  Create<TestableMutableObject>().With(o => o.SingleProperty, "someValue"));

		    var result = builder.Build();

		    Assert.Equal("someValue", result[key].SingleProperty);
	    }

        [Fact]
        public void WithMultipleValue_ShouldAllowValuesToBeSet_WhenIndexedProperty()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o["someString"],  Create<TestableMutableObject>().With(o => o.SingleProperty, "someValue"));
            builder.With(o => o["someOtherString"],  Create<TestableMutableObject>().With(o => o.SingleProperty, "someOtherValue"));

            var result = builder.Build();

            Assert.Equal("someValue", result["someString"].SingleProperty);
            Assert.Equal("someOtherValue", result["someOtherString"].SingleProperty);
        }

        [Fact]
        public void WithMultipleValue_ShouldAllowValuesToBeSet_WhenBuildIsCalled()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.MultipleProperty, "someValue1", "someValue2");

            var result = builder.Build();

            Assert.Equal(2, result.MultipleProperty.Count);
            Assert.Contains("someValue1", result.MultipleProperty);
            Assert.Contains("someValue2",result.MultipleProperty);
        }

        [Fact]
        public void WithMultipleValue_ShouldAllowPrivateValuesToBeSet_WhenBuildIsCalled()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.PrivateMultipleProperty, "someValue1", "someValue2");

            var result = builder.Build();

            Assert.Equal(2, result.PrivateMultipleProperty.Count);
            Assert.Contains("someValue1", result.PrivateMultipleProperty);
            Assert.Contains("someValue2", result.PrivateMultipleProperty);
        }

        [Fact]
        public void WithBuilderValue_ShouldAllowValueToBeSet_WhenBuildIsCalled()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.NestedProperty, Create<TestableMutableObject>().With(np => np.SingleProperty, "someValue"));

            var result = builder.Build();

            Assert.NotNull(result.NestedProperty);
            Assert.Equal("someValue", result.NestedProperty.SingleProperty);
        }

        [Fact]
        public void WithBuilderValue_ShouldAllowPrivateValueToBeSet_WhenBuildIsCalled()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.PrivateNestedProperty, Create<TestableMutableObject>().With(np => np.SingleProperty, "someValue"));

            var result = builder.Build();

            Assert.NotNull(result.PrivateNestedProperty);
            Assert.Equal("someValue", result.PrivateNestedProperty.SingleProperty);
        }

        [Fact]
        public void WithNestedValue_ShouldAllowIntermediatePropertiesToBeInstantiated_WhenBuildIsCalled()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.NestedProperty.NestedProperty.SingleProperty, "someValue");

            var result = builder.Build();

            Assert.NotNull(result.NestedProperty);
            Assert.NotNull(result.NestedProperty.NestedProperty);
            Assert.Equal("someValue", result.NestedProperty.NestedProperty.SingleProperty);
        }

        [Fact]
        public void WithNestedValue_ShouldNotReInstantiateIntermediateProperties_WhenBuildIsCalledAndMultipleIntermediateWithCallsProvided()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.NestedProperty.NestedProperty.SingleProperty, "someValue")
                .With(o => o.NestedProperty.SingleProperty, "someOtherValue");

            var result = builder.Build();

            Assert.NotNull(result.NestedProperty);
            Assert.NotNull(result.NestedProperty.NestedProperty);
            Assert.Equal("someValue", result.NestedProperty.NestedProperty.SingleProperty);
            Assert.Equal("someOtherValue", result.NestedProperty.SingleProperty);
        }

        [Fact]
        public void WithNestedValue_ShouldAllowValueToBeSet_WhenIntermediateIndexedProperty()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o["someKey1","someKey2"].SingleProperty, "someValue");

            var result = builder.Build();

            Assert.Equal("someValue", result["someKey1","someKey2"].SingleProperty);
        }
        
        [Fact]
        public void WithIndexedProperty_ShouldAllowValueToBeSet_WhenKeyComplexObject()
        {
			var builder = Create<TestableMutableObject>();

			var key = new TestableMutableObject{SingleProperty = "someKey"};

			builder.With(o => o[key], "some value");

			var result = builder.Build();

			Assert.Equal("some value", result[key]);
        }

        [Fact]
        public void WithIndexedProperty_ShouldAllowValueToBeSet_WhenKeyProvidedByDelegate()
        {
	        var builder = Create<TestableMutableObject>();

	        Func<string> key = () => "someKey";

	        builder.With(o => o[key()],  Create<TestableMutableObject>().With(o => o.SingleProperty, "someValue"));

	        var result = builder.Build();

	        Assert.Equal("someValue", result[key()].SingleProperty);
        }

        [Fact]
        public void WithMultipleNestedValue_ShouldAllowValuesToBeSet_WhenBuildIsCalled()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.MultipleNestedProperty,
                Create<TestableMutableObject>().With(o => o.SingleProperty, "someValue1"),
                Create<TestableMutableObject>().With(o => o.SingleProperty, "someValue2"));

            var result = builder.Build();

            Assert.Equal(2, result.MultipleNestedProperty.Count);
            Assert.Equal("someValue1", result.MultipleNestedProperty.ElementAt(0).SingleProperty);
            Assert.Equal("someValue2", result.MultipleNestedProperty.ElementAt(1).SingleProperty);
        }

        [Fact]
        public void WithNestedValue_ShouldThrowNotSupportedException_WhenNestedMethodCallProperty()
        {
            var builder = Create<TestableMutableObject>();

            var ex = Assert.Throws<NotSupportedException>(() => builder.With(o => o.NestedProperty.Method().SingleProperty, "someValue"));

            Assert.Equal("The provided expression contains an expression node type that is not supported: \n\tNode Type:\tCall\n\tBody:\t\to.NestedProperty.Method().\nEnsure that the expression only contains property accessors.", ex.Message);
        }

        [Fact]
        public void WithMultipleSharedPaths_ShouldSetPropertiesCorrectly()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.NestedProperty.SingleProperty, "someValue");
            builder.With(o => o.NestedProperty.MultipleProperty, "someOtherValue");

            var result = builder.Build();
            Assert.Equal("someValue", result.NestedProperty.SingleProperty);
            Assert.Equal("someOtherValue", result.NestedProperty.MultipleProperty.Single());
        }

        [Fact]
        public void WithMultipleDuplicatePaths_ShouldSetPropertiesCorrectly()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.NestedProperty.SingleProperty, "someValue");
            builder.With(o => o.NestedProperty.SingleProperty, "someOtherValue");

            var result = builder.Build();
            Assert.Equal("someValue", result.NestedProperty.SingleProperty);
        }

        [Fact]
        public void WithFromExpression_ShouldSetPropertiesCorrectly()
        {
            var builder = Create<TestableMutableObject>();

            builder.With(o => o.SingleProperty, "someValue");
            builder.With(o => o.NestedProperty.SingleProperty, (b)=> b.From(o => o.SingleProperty));

            var result = builder.Build();
            Assert.Equal("someValue", result.SingleProperty);
            Assert.Equal("someValue", result.NestedProperty.SingleProperty);
        }

        [Fact]
        public void WithValueFunc_ShouldCallFuncForEachCallToBuild()
        {
            var i = 0;
            var builder = Create<TestableMutableObject>();
            builder.With(b => b.IntProperty, () => i++);

            var result1 = builder.Build();
            var result2 = builder.Build();

            Assert.NotEqual(result1.IntProperty, result2.IntProperty);
        }

        [Fact]
        public void WithValueFunc_ShouldCallFuncForEachCallToBuild_WhenNestedWithProvided()
        {
            var i = 0;
            var builder = Create<TestableMutableObject>();
            builder.With(b => b.NestedProperty.IntProperty, () => i++);

            var result1 = builder.Build();
            var result2 = builder.Build();

            Assert.NotEqual(result1.NestedProperty.IntProperty, result2.NestedProperty.IntProperty);
        }

        [Fact]
        public void WithMultipleValueFunc_ShouldCallFuncOnce_WhenNoItemCountProvided()
        {
            var i = 0;
            var builder = Create<TestableMutableObject>();
            builder.With(b => b.MultipleProperty, () => i++.ToString());

            var result = builder.Build();

            Assert.Collection(result.MultipleProperty, s => Assert.Equal("0", s));
        }

        [Fact]
        public void WithMultipleValueFunc_ShouldCallFuncGivenTimes_WhenItemCountProvided()
        {
            var i = 0;
            var builder = Create<TestableMutableObject>();
            builder.With(b => b.MultipleProperty, () => i++.ToString(), 3);

            var result = builder.Build();

            Assert.Collection(result.MultipleProperty, 
                s => Assert.Equal("0", s), 
                s => Assert.Equal("1", s), 
                s => Assert.Equal("2", s));
        }

        [Fact]
        public void WithMultipleValueFunc_ShouldCallFuncGivenTimesEachTimeBuildIsCalled_WhenItemCountProvided()
        {
            var i = 0;
            var builder = Create<TestableMutableObject>();
            builder.With(b => b.MultipleProperty, () => i++.ToString(), 3);

            var result1 = builder.Build();
            var result2 = builder.Build();

            Assert.Collection(result1.MultipleProperty, 
                s => Assert.Equal("0", s), 
                s => Assert.Equal("1", s), 
                s => Assert.Equal("2", s));

            Assert.Collection(result2.MultipleProperty, 
                s => Assert.Equal("3", s), 
                s => Assert.Equal("4", s), 
                s => Assert.Equal("5", s));
        }

        [Fact]
        public void WithAbstractProperty_ShouldSetNestedProperty_WhenAbstractValueGivenFirst()
        {
            var builder = Create<TestableMutableObject>();
            
            builder.With(b => b.AbstractProperty, new DerivedObject());
            builder.With(b => b.AbstractProperty.StringProperty, "AbstractString");
            
            var result = builder.Build();

            Assert.IsType<DerivedObject>(result.AbstractProperty);
            Assert.Equal("AbstractString", result.AbstractProperty.StringProperty);
        }

        [Fact]
        public void WithAbstractProperty_ShouldSetNestedProperty_WhenNestedValueGivenFirst()
        {
            var builder = Create<TestableMutableObject>();
            
            builder.With(b => b.AbstractProperty.StringProperty, "AbstractString");
            builder.With(b => b.AbstractProperty, new DerivedObject());
            
            var result = builder.Build();

            Assert.IsType<DerivedObject>(result.AbstractProperty);
            Assert.Equal("AbstractString", result.AbstractProperty.StringProperty);
        }

        [Fact]
        public void WithAbstractProperty_ShouldBeAbleToSetDerivedProperties_WhenAbstractIsTypeCast()
        {
            var builder = Create<TestableMutableObject>();
            
            builder.With(b => ((DerivedObject)b.AbstractProperty).DerivedStringProperty, "DerivedString");
            builder.With(b => b.AbstractProperty, new DerivedObject());
            
            var result = builder.Build();

            Assert.IsType<DerivedObject>(result.AbstractProperty);
            Assert.Equal("DerivedString", ((DerivedObject)result.AbstractProperty).DerivedStringProperty);
        }

        [Fact]
        public void Build_ShouldUseStoredConstructor_WhenTypeIsMutable()
        {
            AddConstructor<TestableMutableObject>(b => new TestableMutableObject{SingleProperty = "someValue" });
            var builder = Create<TestableMutableObject>();

            var result = builder.Build();

            Assert.Equal("someValue", result.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseCustomConstructor_WhenTypeIsMutable()
        {
            AddConstructor<TestableMutableObject>(b => new TestableMutableObject { SingleProperty = "someValue" });
            var builder = Create<TestableMutableObject>(b => new TestableMutableObject{SingleProperty = "someOtherValue"});

            var result = builder.Build();

            Assert.Equal("someOtherValue", result.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseStoredTypeDefault_WhenTypeIsMutable()
        {
            AddTypeDefault(() => 5);
            var builder = Create<TestableMutableObject>();

            var result = builder.With(b => b.IntProperty).Build();

            Assert.Equal(5, result.IntProperty);
        }

        [Fact]
        public void Build_ShouldUseStoredTypeDefault_WhenTypeIsMutableAndDefaultIsComplexType()
        {
            AddTypeDefault(() => new TestableMutableObject{SingleProperty = "someValue"});
            var builder = Create<TestableMutableObject>();

            var result = builder.With(b => b.NestedProperty).Build();

            Assert.Equal("someValue", result.NestedProperty.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseStoredPostBuildAction_WhenTypeIsMutable()
        {
            var wasCalled = false;
            AddPostBuild<TestableMutableObject>(o => wasCalled = true);
            var builder = Create<TestableMutableObject>();

            builder.Build();

            Assert.True(wasCalled);
        }

        [Fact]
        public void Build_ShouldUseCustomPostBuild_WhenTypeIsMutable()
        {
            var storedWasCalled = false;
            var customWasCalled = false;
            AddPostBuild<TestableMutableObject>(o => storedWasCalled = true);
            var builder = Create<TestableMutableObject>(customPostBuild: o => customWasCalled = true);

            builder.Build();

            Assert.True(customWasCalled);
            Assert.False(storedWasCalled);
        }
        #endregion

        #region Immutable With Tests

        [Fact]
        public void Build_ShouldThrowBuildConfigurationException_WhenImmutableObjectHasNoConstructorStored()
        {
            var ex = Assert.Throws<BuildConfigurationException>(() => Create<TestableImmutableObject>().Build());

            Assert.Equal("No Parameter-less Constructor present on type BitWiseBots.FluentBuilders.Tests.Unit.Internal.BuilderFixture+TestableImmutableObject.\nEnsure a construction function is added in an implementation of BuilderConfig.\nAnd that you have called one of the BuilderFactory.AddConfig methods.", ex.Message);
        }

        [Fact]
        public void Build_ShouldUseExpressionInConstructor_WhenWithIsProvided()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            builder.With(o => o.SingleProperty, "someString");

            var result = builder.Build();

            Assert.Equal("someString", result.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseExpressionInConstructor_WhenNestedImmutableSpecified()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            builder.With(o => o.ImmutableNestedProperty, Create<TestableImmutableObject>());

            var result = builder.Build();

            Assert.NotNull(result.ImmutableNestedProperty);
        }

        [Fact]
        public void Build_ShouldUseExpressionInConstructor_WhenNestedMutableIsSpecified()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            builder.With(o => o.NestedProperty, Create<TestableMutableObject>().With(o => o.SingleProperty, "someValue"));

            var result = builder.Build();

            Assert.Equal("someValue", result.NestedProperty.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseExpressionInConstructor_WhenDoubleNestedImmutableSpecified()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            builder.With(o => o.ImmutableNestedProperty.ImmutableNestedProperty, Create<TestableImmutableObject>().Build());
            builder.With(o => o.IntProperty, () => 25);
            var result = builder.Build();

            Assert.NotNull(result.ImmutableNestedProperty.ImmutableNestedProperty);
        }

        [Fact]
        public void Build_ShouldUseExpressionInConstructor_WhenImmutablePropertyNestedInMutableProperty()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            builder.With(o => o.NestedProperty.ImmutableNestedProperty, Create<TestableImmutableObject>());

            var result = builder.Build();

            Assert.NotNull(result.NestedProperty.ImmutableNestedProperty);
        }

        [Fact]
        public void Build_ShouldUseExpressionInConstructor_WhenMutablePropertyNestedInImmutableProperty()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            builder.With(o => o.ImmutableNestedProperty.NestedProperty.SingleProperty, "someValue");

            var result = builder.Build();

            Assert.Equal("someValue", result.ImmutableNestedProperty.NestedProperty.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseExpressionInConstructor_WhenNestedIndexedPropertySpecified()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            builder.With(o => o["someKey"].ImmutableNestedProperty.SingleProperty, "someString");

            var result = builder.Build();

            Assert.Equal("someString", result["someKey"].ImmutableNestedProperty.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseExpressionInConstructor_WhenNestedImmutableIndexedPropertySpecified()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            builder.With(o => o[1].ImmutableNestedProperty.SingleProperty, "someString");

            var result = builder.Build();

            Assert.Equal("someString", result[1].ImmutableNestedProperty.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseExpressionInConstructor_WhenDefaultSpecified()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty, "defaultValue"), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            var result = builder.Build();

            Assert.Equal("defaultValue", result.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseExpressionInConstructor_WhenBuilderDefaultSpecified()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty, Create<TestableMutableObject>().With(o2 => o2.SingleProperty,"someValue"))));

            var builder = Create<TestableImmutableObject>();

            var result = builder.Build();

            Assert.Equal("someValue", result.NestedProperty.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseDefaultInConstructor_WhenWithIsNotProvided()
        {
            AddConstructor<TestableImmutableObject>(b => 
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            var result = builder.Build();

            Assert.Null(result.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseExpressionInConstructorWithDefault_WhenWithIsProvided()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            builder.With(o => o.SingleProperty, "someString");

            var result = builder.Build();

            Assert.Equal("someString", result.SingleProperty);
        }

        [Fact]
        public void Build_ShouldUseDefaultInConstructorWithDefault_WhenWithIsNotProvided()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty, "defaultValue"), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            var result = builder.Build();

            Assert.Equal("defaultValue", result.SingleProperty);
        }

        [Fact]
        public void Build_ShouldNotUseValueAgain_WhenIndexedPropertySetByConstructor()
        {
            var key = new TestableMutableObject();
            AddConstructor<TestableImmutableObject>(b => new TestableImmutableObject(key, b.From(o => o[key])));

            var builder = Create<TestableImmutableObject>();

            builder.With(o => o[key], "someValue");

            var result = builder.Build();
            Assert.Equal("someValue", result[key]);
        }

        [Fact]
        public void Build_ShouldNotUseValueAgain_WhenIntermediateIndexedPropertySetByConstructor()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o["someKey"].SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var builder = Create<TestableImmutableObject>();

            builder.With(o => o["someKey"].SingleProperty, "someValue");

            var result = builder.Build();
            Assert.Equal("someValue", result.SingleProperty);
        }

        [Fact]
        public void Build_ShouldCallPostBuildAction_WhenTypeIsImmutable()
        {
            AddConstructor<TestableImmutableObject>(b =>
                new TestableImmutableObject(b.From(o => o.SingleProperty), b.From(o => o.MultipleProperty), b.From(o => o.ImmutableNestedProperty), b.From(o => o.NestedProperty)));

            var wasCalled = false;
            AddPostBuild<TestableImmutableObject>(o => wasCalled = true);

            var builder = Create<TestableImmutableObject>();

            builder.Build();

            Assert.True(wasCalled);
        }

        [Fact]
        public void Build_ShouldUseGivenValue_WhenValueIsPrimitiveAndNoTypeDefaultGiven()
        {
            AddConstructor<TestableImmutableObject>(b => new TestableImmutableObject(b.From(o => o.IntProperty)));

            var builder = Create<TestableImmutableObject>()
                .With(o => o.IntProperty, 0);

            var result = builder.Build();

            Assert.Equal(0, result.IntProperty);
        }

        #endregion
        #region TestSupportTypes
		private class TestableMutableObject : IEquatable<TestableMutableObject>
		{
			/// <inheritdoc />
			public bool Equals(TestableMutableObject other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;
				return Equals(_dictionary, other._dictionary)
					   && Equals(_nestedDictionary, other._nestedDictionary)
					   && Equals(_complexDictionary, other._complexDictionary)
					   && string.Equals(SingleProperty, other.SingleProperty)
					   && Equals(MultipleProperty, other.MultipleProperty)
					   && Equals(NestedProperty, other.NestedProperty)
					   && Equals(MultipleNestedProperty, other.MultipleNestedProperty)
					   && Equals(ImmutableNestedProperty, other.ImmutableNestedProperty)
					   && string.Equals(PrivateSingleProperty, other.PrivateSingleProperty)
					   && Equals(PrivateMultipleProperty, other.PrivateMultipleProperty)
					   && Equals(PrivateNestedProperty, other.PrivateNestedProperty);
			}

			/// <inheritdoc />
			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;
				return Equals((TestableMutableObject)obj);
			}

			/// <inheritdoc />
			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = _dictionary != null ? _dictionary.GetHashCode() : 0;
					hashCode = (hashCode * 397) ^ (_nestedDictionary != null ? _nestedDictionary.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (_complexDictionary != null ? _complexDictionary.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (SingleProperty != null ? SingleProperty.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (MultipleProperty != null ? MultipleProperty.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (NestedProperty != null ? NestedProperty.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (MultipleNestedProperty != null ? MultipleNestedProperty.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (ImmutableNestedProperty != null ? ImmutableNestedProperty.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (PrivateSingleProperty != null ? PrivateSingleProperty.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (PrivateMultipleProperty != null ? PrivateMultipleProperty.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (PrivateNestedProperty != null ? PrivateNestedProperty.GetHashCode() : 0);
					return hashCode;
				}
			}

			// Public Setters
            public int IntProperty { get; set; }
			public string SingleProperty { get; set; }
			public ICollection<string> MultipleProperty { get; set; }
			public TestableMutableObject NestedProperty { get; set; }
            public AbstractObject AbstractProperty { get; set; }

			// Additional scenarios
			public ICollection<TestableMutableObject> MultipleNestedProperty { get; set; }
			public TestableImmutableObject ImmutableNestedProperty { get; set; }

			// Private Setters
			public string PrivateSingleProperty { get; private set; }
			public ICollection<string> PrivateMultipleProperty { get; private set; }
			public TestableMutableObject PrivateNestedProperty { get; private set; }


			// Indexed Properties
			private readonly Dictionary<string, TestableMutableObject> _dictionary = new Dictionary<string, TestableMutableObject>();
			public TestableMutableObject this[string s]
			{
				get => _dictionary[s];
                set => _dictionary[s] = value;
			}

			private readonly Dictionary<Tuple<string, string>, TestableMutableObject> _nestedDictionary = new Dictionary<Tuple<string, string>, TestableMutableObject>();
			public TestableMutableObject this[string s, string s2]
			{
				get => _nestedDictionary[new Tuple<string, string>(s, s2)];
				set => _nestedDictionary[new Tuple<string, string>(s, s2)] = value;
			}

			private readonly Dictionary<TestableMutableObject, string> _complexDictionary = new Dictionary<TestableMutableObject, string>();

			public string this[TestableMutableObject key]
			{
				get => _complexDictionary[key];
				set => _complexDictionary[key] = value;
			}

			// UnSupported Members
			public TestableMutableObject Method()
			{
				return this;
			}
		}

		private class TestableImmutableObject : TestableMutableObject
		{
            public TestableImmutableObject(int intProperty)
            {
                IntProperty = intProperty;
            }

			public TestableImmutableObject(string singleProperty, ICollection<string> multipleProperty, TestableImmutableObject immutableNestedProperty, TestableMutableObject mutableNestProperty)
			{
				SingleProperty = singleProperty;
				MultipleProperty = multipleProperty;
                ImmutableNestedProperty = immutableNestedProperty;
                NestedProperty = mutableNestProperty;
            }

            private readonly Dictionary<int, TestableImmutableObject> _dictionary = new Dictionary<int, TestableImmutableObject>();
            public TestableImmutableObject this[int s]
            {
                get => _dictionary[s];
                set => _dictionary[s] = value;
            }

            public TestableImmutableObject(TestableMutableObject key, string value)
            {
                this[key] = value;
            }
        }

        private abstract class AbstractObject
        {
            public string StringProperty { get; set; }
        }

        private class DerivedObject : AbstractObject
        {
            public string DerivedStringProperty { get; set; }
        }
        #endregion
	}
}