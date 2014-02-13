using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Reflection;
using CPCToolkitExtLibraries;
using System.Diagnostics;

namespace CPCToolkitExt.AddressControl
{
    public class ControlHelper
    {
        #region DeepClone
        /// <summary>
        /// Copy value 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object DeepClone(object obj)
        {
            object objResult = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }
            return objResult;
        }
        public static object ConverObject(object object_1, object object_2)
        {
            // Get all the fields of the type, also the privates.
            FieldInfo[] fis = object_1.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            // Loop through all the fields and copy the information from the parameter class

            // to the newPerson class.
            foreach (FieldInfo fi in fis)
            {
                fi.SetValue(object_2, fi.GetValue(object_1));
            }
            // Return the cloned object.
            return object_2;
        }

        public static AddressControlModel DeepClone(object param, Type type)
        {
            using (var ms = new MemoryStream())
            {
                XmlSerializer xs = new XmlSerializer(type);
                xs.Serialize(ms, param);
                ms.Position = 0;
                return (AddressControlModel)xs.Deserialize(ms);
            }
        }
        public static AddressControlModel Clone(AddressControlModel obj)
        {
            try
            {
                // an instance of target type.
                AddressControlModel _object = (AddressControlModel)Activator.CreateInstance(obj.GetType());
                //To get type of value.
                Type type = obj.GetType();
                //To Copy value from input value.
                foreach (PropertyInfo oPropertyInfo in
                    type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(ø => ø.CanRead && ø.CanWrite)
                    .Where(ø => ø.GetSetMethod(true).IsPublic))
                {
                    oPropertyInfo.SetValue(_object, type.GetProperty(oPropertyInfo.Name).GetValue(obj, null), null);
                }
                //To retrun clone value.
                Debug.WriteLine("Clone value");
                return _object;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Clone value" + ex.ToString());
            }
            return null;
        }
        public static AddressControlCollection CollectionClone(object param, Type type)
        {
            using (var ms = new MemoryStream())
            {
                XmlSerializer xs = new XmlSerializer(type);
                xs.Serialize(ms, param);
                ms.Position = 0;

                return (AddressControlCollection)xs.Deserialize(ms);
            }
        }
        #endregion
    }
}
