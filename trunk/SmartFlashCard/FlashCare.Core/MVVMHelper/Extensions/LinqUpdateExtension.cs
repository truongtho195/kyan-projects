using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMHelper.Common
{
    public static class LinqUpdateExtension
    {
        public delegate void Func<TArg0>(TArg0 element);

        /// <summary>
        /// Executes an Update statement block on all elements in an IEnumerable<T> sequence.
        /// </summary>
        /// <typeparam name="TSource">The source element type.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="update">The update statement to execute for each element.</param>
        /// <returns>The numer of records affected.</returns>
        public static int Update<TSource>(this IEnumerable<TSource> source, Func<TSource> update)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (update == null) throw new ArgumentNullException("update");
            if (typeof(TSource).IsValueType)
                throw new NotSupportedException("value type elements are not supported by update.");

            int count = 0;
            foreach (TSource element in source)
            {
                update(element);
                count++;
            }
            return count;
        }

    }
}
