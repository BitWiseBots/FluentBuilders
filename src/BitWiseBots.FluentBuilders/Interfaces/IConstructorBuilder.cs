using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace BitWiseBots.FluentBuilders.Interfaces
{
    /// <summary>
    /// A secondary interface that allows for extracting values set using the <see cref="Builder{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of object being built.</typeparam>
    public interface IConstructorBuilder<T> : IHideObjectMembers
    {
        /// <summary>
        /// Use this method within an implementation of <see cref="BuilderConfig"/> to use values set using any of the <see cref="M:With"/> overloads within the constructor expression.
        /// </summary>
        /// <typeparam name="T2">The type of the property being accessed.</typeparam>
        /// <param name="expression">An expression that specifies which property is being set within the constructor.</param>
        /// <param name="defaultValue">A value that will be used if a call to <see cref="M:With"/> was not made for the same <paramref name="expression"/>.</param>
        /// <returns>The value that was set for a call to <see cref="M:With"/> using the same <paramref name="expression"/>, or <paramref name="defaultValue"/>.</returns>
        [PublicAPI]
        T2 From<T2>(Expression<Func<T, T2>> expression, T2 defaultValue = default);
    }
}