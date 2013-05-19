
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.Common;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.ComponentModel;

public static partial class LinqToSqlExtension
{
    public static IQueryable<TSource> Between<TSource, TKey>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TKey>> keySelector,
        TKey low, TKey high) where TKey : IComparable<TKey>
    {
        Expression key = Expression.Invoke(keySelector,
             keySelector.Parameters.ToArray());
        Expression lowerBound = Expression.LessThanOrEqual
            (Expression.Constant(low), key);
        Expression upperBound = Expression.LessThanOrEqual
            (key, Expression.Constant(high));
        Expression and = Expression.AndAlso(lowerBound, upperBound);
        Expression<Func<TSource, bool>> lambda =
            Expression.Lambda<Func<TSource, bool>>(and, keySelector.Parameters);
        return source.Where(lambda);
    }

    public static IQueryable<TSource> WhereIf<TSource>(
        this IQueryable<TSource> source, bool condition,
        Expression<Func<TSource, bool>> predicate)
    {
        if (condition)
            return source.Where(predicate);
        else
            return source;
    }

    public static IEnumerable<TSource> WhereIf<TSource>(
        this IEnumerable<TSource> source, bool condition,
        Func<TSource, bool> predicate)
    {
        if (condition)
            return source.Where(predicate);
        else
            return source;
    }
}