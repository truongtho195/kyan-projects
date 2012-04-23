using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.IO;
using System.Windows.Markup;
using System.Xml;

namespace MVVMHelper.Common
{
    public static class FlowDocumentConverter
    {
        public static string ConvertFlowDocumentToSUBStringFormat(object flowDocument)
        {
            //take the flow document and change all of its images into a base64 string
            //apply the XamlWriter to the newly transformed flowdocument
            using (StringWriter stringwriter = new StringWriter())
            {
                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stringwriter))
                {
                    XamlWriter.Save(flowDocument, writer);
                }
                return stringwriter.ToString();
            }
        }

        public static FlowDocument ConvertXMLToFlowDocument(string XmlContent)
        {
            try
            {
                var stringReader = new StringReader(XmlContent);
                var xmlTextReader = new XmlTextReader(stringReader);
                return (FlowDocument)XamlReader.Load(xmlTextReader);
            }
            catch (Exception ex)
            {
                throw ex;
            }



            //var flowDocument = new FlowDocument();
            //if (XmlContent != null)
            //{
            //    var stringReader = new StringReader(XmlContent);
            //    var xmlTextReader = new XmlTextReader(stringReader);
            //    var doc = new FlowDocument();
            //    Section sec = XamlReader.Load(xmlTextReader) as Section;
            //    while (sec.Blocks.Count > 0)
            //    {
            //        var block = sec.Blocks.FirstBlock;
            //        sec.Blocks.Remove(block);
            //        doc.Blocks.Add(block);
            //    }
            //    return flowDocument;
            //}
            //return null;
        }
    }
}
