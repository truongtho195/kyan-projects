using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.ComponentModel;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Markup;

namespace FlashCard.UserControls
{
    public class RichTextBoxControl : RichTextBox
    {
        #region Constructor
        public RichTextBoxControl()
        {
            //this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.V, ModifierKeys.Control)));
            //this.InputBindings.Add(new System.Windows.Input.KeyBinding(System.Windows.Input.ApplicationCommands.NotACommand, new System.Windows.Input.KeyGesture(Key.Z, ModifierKeys.Control)));
            this.ContextMenu = null;
            this.TextChanged += new TextChangedEventHandler(RichTextBoxControl_TextChanged);
        }
        #endregion

        #region Fields
        protected bool IsSetValue = false;
        #endregion

        #region DependencyProperties
        public new FlowDocument Content
        {
            get { return (FlowDocument)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(FlowDocument), typeof(RichTextBoxControl), new PropertyMetadata(OnDocumentChanged));

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                RichTextBoxControl control = (RichTextBoxControl)d;
                if (control.IsFocused) return;
                if (e.NewValue == null)
                {
                    //Document is not amused by null :)
                    control.Document.Blocks.Clear();
                    return;
                }
                else if (e.NewValue != e.OldValue)
                {
                    control.Document.Blocks.Clear();
                    MemoryStream ms = new MemoryStream();
                    XamlWriter.Save(e.NewValue as FlowDocument, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    control.Document = XamlReader.Load(ms) as FlowDocument;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<OnDocumentChanged>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion

        #region Event
        void RichTextBoxControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!this.IsFocused) return;
                this.Content = this.Document;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<RichTextBoxControl_TextChanged>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }
        #endregion
    }
}
