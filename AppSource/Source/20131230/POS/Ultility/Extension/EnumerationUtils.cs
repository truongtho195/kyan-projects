using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Enumeration comparative
/// </summary>
static class EnumerationUtils
{

    //checks if the value contains the provided type
    public static bool Has<T>(this System.Enum type, T value)
    {
        try
        {
            return ((int)(object)type & (int)(object)value) == (int)(object)value;
        }
        catch
        {
            return false;
        }
    }

    public static bool Has<T>(this int type, T value)
    {
        try
        {
            return (type & (int)(object)value) == (int)(object)value;
        }
        catch
        {
            return false;
        }
    }
    //checks if the value contains the provided type
    public static bool In<T>(this System.Enum type, T value)
    {
        try
        {
            return ((int)(object)type & (int)(object)value) == (int)(object)type;
        }
        catch
        {
            return false;
        }
    }

    public static bool In<T>(this int type, T value)
    {
        try
        {
            return (type & (int)(object)value) == type;
        }
        catch
        {
            return false;
        }
    }
    //checks if the value is only the provided type
    public static bool Is<T>(this System.Enum type, T value)
    {
        try
        {
            return (int)(object)type == (int)(object)value;
        }
        catch
        {
            return false;
        }
    }

    public static bool Is<T>(this int type, T value)
    {
        try
        {
            return type == (int)(object)value;
        }
        catch
        {
            return false;
        }
    }

    public static bool Is<T>(this short type, T value)
    {
        try
        {
            return (int)type == (int)(object)value;
        }
        catch
        {
            return false;
        }
    }

    //appends a value
    public static T Add<T>(this System.Enum type, T value)
    {
        try
        {
            return (T)(object)(((int)(object)type | (int)(object)value));
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                string.Format(
                    "Could not append value from enumerated type '{0}'.",
                    typeof(T).Name
                    ), ex);
        }
    }

    public static int Add<T>(this int type, T value)
    {
        try
        {
            return type | (int)(object)value;
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                string.Format(
                    "Could not append value from enumerated type '{0}'.",
                    typeof(T).Name
                    ), ex);
        }
    }

    //completely removes the value
    public static T Remove<T>(this System.Enum type, T value)
    {
        try
        {
            return (T)(object)(((int)(object)type & ~(int)(object)value));
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                string.Format(
                    "Could not remove value from enumerated type '{0}'.",
                    typeof(T).Name
                    ), ex);
        }
    }

    public static int Remove<T>(this int type, T value)
    {
        try
        {
            return type & ~(int)(object)value;
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                string.Format(
                    "Could not remove value from enumerated type '{0}'.",
                    typeof(T).Name
                    ), ex);
        }
    }

}