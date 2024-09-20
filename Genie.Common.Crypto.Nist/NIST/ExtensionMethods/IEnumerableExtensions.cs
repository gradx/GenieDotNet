using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Common.ExtensionMethods
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// returns first item found, or null if not found.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="items">The IEnumerable to iterate</param>
        /// <param name="predicate">What to search for</param>
        /// <returns></returns>
        public static T? FirstOrNull<T>(this IEnumerable<T> items, Func<T, bool> predicate) where T : struct
        {
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
            if (items == null)
                throw new ArgumentNullException(nameof(items));
#pragma warning restore CA1510 // Use ArgumentNullException throw helper
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
#pragma warning restore CA1510 // Use ArgumentNullException throw helper

            foreach (var item in items)
            {
                if (predicate(item))
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Try to get an item from the <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="items">The IEnumerable to iterate</param>
        /// <param name="predicate"></param>
        /// <param name="result">The type to return when found</param>
        /// <returns></returns>
        public static bool TryFirst<T>(this IEnumerable<T> items, Func<T, bool> predicate, out T result)
        {
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
            if (items == null)
                throw new ArgumentNullException(nameof(items));
#pragma warning restore CA1510 // Use ArgumentNullException throw helper
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
#pragma warning restore CA1510 // Use ArgumentNullException throw helper

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable IDE0034 // Simplify 'default' expression
            result = default(T);
#pragma warning restore IDE0034 // Simplify 'default' expression
#pragma warning restore CS8601 // Possible null reference assignment.
            foreach (var item in items)
            {
                if (predicate(item))
                {
                    result = item;
                    return true;
                }
            }
            return false;
        }
    }
}
