using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit;
using HTMLConverter;
using System.Windows.Documents;
using System.IO;
using System.Windows.Markup;
using System.Xml.Serialization;
using System.Xml;
using System.Windows;

namespace FlashCard.Converters
{
    public class HtmlFormatConverter : ITextFormatter
    {
        public string GetText(System.Windows.Documents.FlowDocument document)
        {
            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Xaml);
                return ASCIIEncoding.Default.GetString(ms.ToArray());
                
            }
        }

        public void SetText(System.Windows.Documents.FlowDocument document, string text)
        {
            if (text != null && !string.IsNullOrWhiteSpace(text))
            {
                TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
                using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(text)))
                {
                    tr.Load(ms, DataFormats.Xaml);
                }
            }
        }
    }
}
