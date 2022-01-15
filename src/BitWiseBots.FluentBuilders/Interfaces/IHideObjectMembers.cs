using System;
using System.ComponentModel;
// ReSharper disable UnusedMemberInSuper.Global

namespace BitWiseBots.FluentBuilders.Interfaces
{
    /// <summary>
    /// This interface is used to remove the System.Object members from intellisense to improve intellisense readability.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IHideObjectMembers
    {
        /// <summary>
        /// Hides <see cref="object.GetType()"/> from intellisense.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();

        /// <summary>
        /// Hides <see cref="object.GetHashCode()"/> from intellisense.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        /// <summary>
        /// Hides <see cref="object.ToString()"/> from intellisense.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();

        /// <summary>
        /// Hides <see cref="object.Equals(object)"/> from intellisense.
        /// </summary>
        /// <param name="obj"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);
    }
}