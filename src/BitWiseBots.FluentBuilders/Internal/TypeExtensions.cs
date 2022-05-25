using System;
using System.Reflection;

namespace BitWiseBots.FluentBuilders.Internal
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Returns the most specific name available on the provided type.
        /// </summary>
        /// <param name="type">The type to get a name for.</param>
        /// <returns><see cref="Type.AssemblyQualifiedName"/> if available, or <see cref="Type.FullName"/> if available, or <see cref="MemberInfo.Name"/> in all other scenarios.</returns>
        public static string GetStoreKey(this Type type)
        {
            return type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
        }
    }
}