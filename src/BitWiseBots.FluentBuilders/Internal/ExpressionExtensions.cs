using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BitWiseBots.FluentBuilders.Internal
{
    internal static class ExpressionExtensions
    {
        /// <summary>
        /// Gets a <see cref="MemberExpression"/> from the given <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The expression to dig into.</param>
        /// <returns>
        /// If <paramref name="expression"/> is a <see cref="MemberExpression"/> then <paramref name="expression"/>,
        /// If <paramref name="expression"/> is a <see cref="LambdaExpression"/> then <see cref="LambdaExpression.Body"/>,
        ///     this will also unwrap <see cref="UnaryExpression"/>s,
        /// Otherwise <c>null</c>.
        /// </returns>
        internal static MemberExpression AsMemberExpression(this Expression expression)
        {
            var member = expression as MemberExpression;
            var unary = expression as UnaryExpression;
            return member
                ?? unary?.Operand as MemberExpression;
        }

        /// <summary>
        /// Gets a <see cref="IndexExpression"/> from the given <see cref="Expression"/> by taking a <see cref="MethodCallExpression"/> to the Indexer's get and building a new <see cref="IndexExpression"/> to the underlying property.
        /// </summary>
        /// <param name="expression">A <see cref="LambdaExpression"/> to unwrap or a <see cref="MethodCallExpression"/> to use directly.</param>
        /// <returns>A new <see cref="IndexExpression"/> using the same indexer arguments provided in the original <see cref="MethodCallExpression"/>.</returns>
        internal static IndexExpression AsIndexExpression(this Expression expression)
        {
            // Unwrap the expression if its a Lambda
            var lambda = expression as LambdaExpression;
            var methodCallExpression = lambda?.Body as MethodCallExpression ?? expression as MethodCallExpression;

            // Short circuit if the expression isn't a valid node, or doesn't have a parent expression.
            if (methodCallExpression?.Method.Name != "get_Item" || methodCallExpression.Object == null) return null;

            // Find the Indexer property info with the same signature as the one called in the MethodCallExpression
            var indexerProperty = (from p in methodCallExpression.Object.Type.GetDefaultMembers().OfType<PropertyInfo>()
                                   let q = p.GetIndexParameters()
                                   where q.Length > 0
                                         && q.Length == methodCallExpression.Arguments.Count
                                         && q.Select(t => t.ParameterType).All(t => methodCallExpression.Arguments.Any(mt => mt.Type == t))
                                   select p).SingleOrDefault();

            // If no Indexer Property was found then we can't find a matching signature.
            return indexerProperty != null
                ? Expression.MakeIndex(methodCallExpression.Object, indexerProperty, methodCallExpression.Arguments)
                : null;
        }

        /// <summary>
        /// Gets the list of arguments provided to an Indexed property within a <see cref="IndexExpression"/>.
        /// </summary>
        /// <param name="indexExpression">The expression to inspect.</param>
        /// <returns>An <see cref="T:object[]"/> containing the indexer arguments.</returns>
        internal static object[] GetIndexerArguments(this IndexExpression indexExpression)
        {
            //Extract the indexer values from their expression.
            var expressionArgs = indexExpression.Arguments
                .Select(e => Expression.Lambda(e).Compile().DynamicInvoke());

            return expressionArgs.ToArray();
        }
    }
}