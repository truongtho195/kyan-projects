﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

public class CacheObject
{
    static readonly int cachePeriod = 30; // one day
    static readonly ObjectCache Cache = MemoryCache.Default;

    /// <summary>
    /// Retrieve cached item
    /// </summary>
    /// <typeparam name="T">Type of cached item</typeparam>
    /// <param name="key">Name of cached item</param>
    /// <returns>Cached item as type</returns>
    public static T Get<T>(string key) where T : class
    {
        try
        {
            return (T)Cache[key];
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Insert value into the cache using
    /// appropriate name/value pairs
    /// </summary>
    /// <typeparam name="T">Type of cached item</typeparam>
    /// <param name="objectToCache">Item to be cached</param>
    /// <param name="key">Name of item</param>
    public static void Add<T>(string key,T objectToCache) where T : class
    {
        Cache.Add(key, objectToCache, DateTime.Now.AddDays(cachePeriod));
    }

    /// <summary>
    /// Insert value into the cache using
    /// appropriate name/value pairs
    /// </summary>
    /// <param name="objectToCache">Item to be cached</param>
    /// <param name="key">Name of item</param>
    public static void Add(string key,object objectToCache )
    {
        Cache.Add(key, objectToCache, DateTime.Now.AddDays(cachePeriod));
    }

    /// <summary>
    /// Remove item from cache
    /// </summary>
    /// <param name="key">Name of cached item</param>
    public static void Clear(string key)
    {
        Cache.Remove(key);
    }

    /// <summary>
    /// Check for item in cache
    /// </summary>
    /// <param name="key">Name of cached item</param>
    /// <returns></returns>
    public static bool Exists(string key)
    {
        return Cache.Get(key) != null;
    }

    /// <summary>
    /// Gets all cached items as a list by their key.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAll()
    {
        return Cache.Select(keyValuePair => keyValuePair.Key).ToList();
    }
}