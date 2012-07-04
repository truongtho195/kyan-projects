using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.Windows.Markup;
using System.ComponentModel;

namespace FlashCard.UserControls
{
    public class BindableRichTextBox : RichTextBox
    {
        public static readonly DependencyProperty DocumentProperty =
          DependencyProperty.Register("Document", typeof(FlowDocument),
          typeof(BindableRichTextBox), new FrameworkPropertyMetadata
          (null, new PropertyChangedCallback(OnDocumentChanged)));

        public new FlowDocument Document
        {
            get
            {
                return (FlowDocument)this.GetValue(DocumentProperty);
            }

            set
            {
                this.SetValue(DocumentProperty, value);
            }
        }

        public static void OnDocumentChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs args)
        {
            try
            {
                RichTextBox rtb = (RichTextBox)obj;
                rtb.Document = (FlowDocument)args.NewValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("========BindableRichTextBox = = OnDocumentChanged =========");
                Console.WriteLine(ex.ToString());
            }
          
        }
    }
}
