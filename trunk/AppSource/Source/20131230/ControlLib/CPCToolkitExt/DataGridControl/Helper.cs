using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;

namespace CPCToolkitExt
{
    public static class Helper
    {
        public static object CloneObjectWithReflection(object p)
        {
            // Get all the fields of the type, also the privates.
            FieldInfo[] fis = p.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            // Create a new person object
            object newobject = new object();
            // Loop through all the fields and copy the information from the parameter class
            // to the newPerson class.
            foreach (FieldInfo fi in fis)
            {
                fi.SetValue(newobject, fi.GetValue(p));
            }
            // Return the cloned object.
            return newobject;
        }

        public static object ConverObject(object object_1, object object_2)
        {
            // Get all the fields of the type, also the privates.
            // Loop through all the fields and copy the information from the parameter class
            // to the newPerson class.
            foreach (PropertyInfo oPropertyInfo in
                object_1.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(ø => ø.CanRead && ø.CanWrite)
                .Where(ø => ø.GetSetMethod(true).IsPublic))
            {
                oPropertyInfo.SetValue(object_2, oPropertyInfo.GetValue(object_1, null), null);
            }
            // Return the cloned object.
            Debug.WriteLine(" RollBack value");
            return object_2;
        }
        public static object Clone(object obj)
        {
            try
            {
                // an instance of target type.
                object _object = (object)Activator.CreateInstance(obj.GetType());
                //To get type of value.
                Type type = obj.GetType();
                //To Copy value from input value.
                foreach (PropertyInfo oPropertyInfo in
                    type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(ø => ø.CanRead && ø.CanWrite)
                    .Where(ø => ø.GetSetMethod(true).IsPublic))
                    oPropertyInfo.SetValue(_object, type.GetProperty(oPropertyInfo.Name).GetValue(obj, null), null);
                //To retrun clone value.
                return _object;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return null;
        }
    }
}
