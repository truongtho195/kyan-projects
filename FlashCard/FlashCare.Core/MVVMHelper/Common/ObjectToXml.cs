using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace POS.Libraries
{
    public class ObjectToXml<T>
    {
        #region Convert object to XML method
        /// <summary>
        /// Convert object to XML
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        public static void ToXml(T obj, string fileName)
        {
            // Serialize
            Stream stream = File.Open(fileName, FileMode.OpenOrCreate);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, obj);

            stream.Flush();
            stream.Close();
            stream.Dispose();
            stream = null;
        } 
        #endregion

        #region Convert XML To Object
        /// <summary>
        /// Convert XML File to Object with type is T
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T ToObject(string fileName)
        {
            //deserialize
            Stream stream = File.Open(fileName, FileMode.Open);
            BinaryFormatter bFormatter = new BinaryFormatter();
            T obj = (T)bFormatter.Deserialize(stream);

            stream.Flush();
            stream.Close();
            stream.Dispose();
            stream = null;

            return obj;
        } 
        #endregion
    }
}
