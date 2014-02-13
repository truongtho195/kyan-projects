using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Windows.Markup;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using CPCToolkitExtLibraries;
using CPCToolkitExt.Command;

namespace CPCToolkitExt.AddressControl
{
    public partial class PopupAddressView : Window
    {
        #region Constructors
        public PopupAddressView()
        {
            this.InitializeComponent();
            this.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive == true);
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(PopupAddress_MouseLeftButtonDown);
            this.Closed += new EventHandler(PopupAddressView_Closed);
            this.cmbAddressType.SelectionChanged += new SelectionChangedEventHandler(cmbAddressType_SelectionChanged);
        }

        void cmbAddressType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.txtStreet.Focus();
        }
        #endregion

        #region Fields
        public bool IsCancel { get; set; }
        #endregion

        #region Event Control

        void PopupAddress_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void PopupAddressView_Closed(object sender, EventArgs e)
        {
            this.IsCancel = false;
        }
        #endregion

    }
}