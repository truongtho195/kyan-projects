using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.IO;
using System.Xml;
using System.Text;

namespace FlashCard.Converters
{
    [ValueConversion(typeof(string), typeof(FlowDocument))]
    public class FlowDocumentToXamlConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts from XAML markup to a WPF FlowDocument.
        /// </summary>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            /* See http://stackoverflow.com/questions/897505/getting-a-flowdocument-from-a-xaml-template-file */
            var flowDocument = new FlowDocument();
            if (value != null)
            {
                var stringReader = new StringReader(value.ToString());
                var xmlTextReader = new XmlTextReader(stringReader);
                var doc = new FlowDocument();
                Section sec = XamlReader.Load(xmlTextReader) as Section;
                while (sec.Blocks.Count > 0)
                {
                    var block = sec.Blocks.FirstBlock;
                    sec.Blocks.Remove(block);
                    doc.Blocks.Add(block);
                }
                flowDocument = doc;
                
            }
            return flowDocument; 

            //var flowDocument = new FlowDocument();
            //if (value != null)
            //{
            //    var xamlText = (string) value;
            //    flowDocument = (FlowDocument)XamlReader.Parse(xamlText); 
            //}

            //// Set return value
            //return flowDocument; 
        }

        /// <summary>
        /// Converts from a WPF FlowDocument to a XAML markup string.
        /// </summary>
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ///* This converter does not insert returns or indentation into the XAML. If you need to 
            // * indent the XAML in a text box, see http://www.knowdotnet.com/articles/indentxml.html */

            //// Exit if FlowDocument is null
            //if (value == null) return string.Empty;

            //// Get flow document from value passed in
            //var flowDocument = (FlowDocument)value;

            //// Convert to XAML and return
            //return XamlWriter.Save(flowDocument);
            StringReader stringReader = new StringReader(value.ToString());
            XmlReader xmlReader = XmlReader.Create(stringReader);
            
             
            string xamlText = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(value.ToString()));
            

            return value;
        }

        #endregion
    }
}