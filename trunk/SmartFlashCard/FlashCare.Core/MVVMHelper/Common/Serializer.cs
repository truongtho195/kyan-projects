using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

public class Serializer<T>
{

    #region Serializer Object

    public static string SerializeObj(T obj)
    {
        XmlSerializer serializer = new XmlSerializer(obj.GetType());
        using (StringWriter writer = new StringWriter())
        {
            serializer.Serialize(writer, obj);
            return writer.ToString();
        }
    }

    public static T DeserializeObj(string xml)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using (StringReader reader = new StringReader(xml))
        {
            return (T)serializer.Deserialize(reader);
        }
    }
    #endregion

    #region Serializer to Files
    public static void Serialize(T obj, string filelocation, bool Append=false)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
        using (TextWriter textWriter = new StreamWriter(filelocation, Append, ASCIIEncoding.UTF8))
        {

            xmlSerializer.Serialize(textWriter, obj);
        }
    }

    


    public static T Deserialize(string filelocation)
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
