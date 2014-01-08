using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

/// <summary>
/// Extension methods for ObservableCollection
/// </summary>
static class CollectionUtils
{
    /// <summary>
    /// Sort an ObservableCollection using bubble sort
    /// </summary>
    /// <typeparam name="T">Type of the collections elements</typeparam>
    /// <param name="pTheCollection">Collection that will be sorted</param>
    /// <param name="pComparer">Comparer used for comparing two elements of the collection</param>
    public static void SortBubble<T>(this ObservableCollection<T> pTheCollection, IComparer<T> pComparer)
    {
        T currentT;
        int j;
        for (int i = 1; i < pTheCollection.Count; i++)
        {
            currentT = pTheCollection[i];
            j = i;
            while (j > 0 && pComparer.Compare(pTheCollection[j - 1], currentT) == 1)
            {
                pTheCollection[j] = pTheCollection[j - 1];
                j--;
            }

            pTheCollection[j] = currentT;
        }
    }

    /// <summary>
    /// Sort an ObservableCollection using quick sort
    /// </summary>
    /// <typeparam name="T">Type of the collections elements</typeparam>
    /// <param name="pTheCollection">Collection that will be sorted</param>
    /// <param name="pComparer">Comparer used for comparing two elements of the collection</param>
    public static void SortQuick<T>(this ObservableCollection<T> pTheCollection, IComparer<T> pComparer)
    {
        InternalQuickSort(pTheCollection, 0, pTheCollection.Count - 1, pComparer);
    }

    /// <summary>
    /// Internal implementation for sorting one interval. This is the recursive version of quick sort
    /// </summary>
    /// <typeparam name="T">Type of the collections elements</typeparam>
    /// <param name="pTheCollection">Collection that will be sorted</param>
    /// <param name="pLower">lower bound of the sorted interval</param>
    /// <param name="pUpper">upper bound of the sorted interval</param>
    /// <param name="pComparer">Comparer used for comparing two elements of the collection</param>
    private static void InternalQuickSort<T>(ObservableCollection<T> pTheCollection, int pLower, int pUpper, IComparer<T> pComparer)
    {
        int i = pLower;
        int j = pUpper;
        int pivot = (pLower + pUpper) / 2;
        T center = pTheCollection[pivot];
        T tmpT;
        while (i <= j)
        {
            while (pComparer.Compare(pTheCollection[i], center) == -1)
            {
                i++;
            }

            while (pComparer.Compare(pTheCollection[j], center) == 1)
            {
                j--;
            }

            if (i <= j)
            {
                tmpT = pTheCollection[i];
                pTheCollection[i++] = pTheCollection[j];
                pTheCollection[j--] = tmpT;
            }
        }

        if (pLower < j)
        {
            InternalQuickSort(pTheCollection, pLower, j, pComparer);
        }

        if (i < pUpper)
        {
            InternalQuickSort(pTheCollection, i, pUpper, pComparer);
        }
    }

    /// <summary>
    /// Convert IEnumerable to DataTable 
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static DataTable ToDataTable(this IEnumerable collection, string tableName)
    {
        DataTable dt = new DataTable(tableName);
        foreach (object obj in collection)
        {
            // Fill headers
            Type type = obj.GetType();
            PropertyInfo[] propertyInfos = type.GetProperties();
            if (dt.Columns.Count == 0)
            {
                foreach (PropertyInfo pi in propertyInfos)
                {
                    Type pt = pi.PropertyType;
                    if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>))
                        pt = Nullable.GetUnderlyingType(pt);
                    dt.Columns.Add(pi.Name, pt);
                }
            }

            // Fill data
            DataRow dr = dt.NewRow();
            foreach (PropertyInfo pi in propertyInfos)
            {
                object value = pi.GetValue(obj, null);
                if (value is object[])
                {
                    // Inorge indexer
                    continue;
                }
                dr[pi.Name] = value ?? DBNull.Value;
            }
            dt.Rows.Add(dr);
        }

        return dt;
    }

    /// <summary>
    /// Check property of item in collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static bool Has<T>(this ObservableCollection<T> collection, params string[] parameters)
    {
        bool result = false;
        if (collection != null)
            for (int i = 0; i < parameters.Length; i++)
                result |= collection.Count(x => (bool)x.GetType().GetProperty(parameters[i]).GetValue(x, null)) > 0;
        return result;
    }

    /// <summary>
    /// Check property of item in collection with expression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static bool Has<T>(this ObservableCollection<T> collection, Func<T, bool> expression)
    {
        bool result = false;
        if (collection != null)
            result = collection.Count(expression) > 0;
        return result;
    }
}