using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

public class Serializer
{

    #region Serializer Object    

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string SerializeObj<T>(T obj)
    {
        XmlSerializer serializer = new XmlSerializer(obj.GetType());
        using (StringWriter writer = new StringWriter())
        {
            serializer.Serialize(writer, obj);

            return writer.ToString();
        }
    }

    public static T DeserializeObj<T>(string xml) where T : class
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using (StringReader reader = new StringReader(xml))
        {
            return (T)serializer.Deserialize(reader);
        }
    } 
    #endregion

    #region Serializer to Files
    public static void Serialize<T>(T obj, string filelocation)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
        using (TextWriter textWriter = new StreamWriter(filelocation))
        {
            xmlSerializer.Serialize(textWriter, obj);
        }
    }

    public static T Deserialize<T>(string filelocation)
    {
        try
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (TextReader textReader = new StreamReader(filelocation))
            {
                return (T)xmlSerializer.Deserialize(textReader);
            }
        }
        catch
        {
            if (File.Exists(filelocation))
            {
                File.Delete(filelocation);
            }
        }
        return default(T);
    } 
    #endregion

}
