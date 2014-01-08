using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using CPC.Toolkit.Base;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Diagnostics;
using CPC.Toolkit.Command;
using System.Globalization;
using KeyGenerator.ViewModel;
using KeyGenerator.Model;

namespace KeyGenerator
{
    /// <summary>
    /// Interaction logic for InsertLicense.xaml
    /// </summary>
    public partial class KeyGeneratorView : Window
    {
        #region Ctor
        public KeyGeneratorView()
        {
            this.InitializeComponent();
            BrdTopBar.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(BrdTopBar_PreviewMouseLeftButtonDown);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewModel"></param>
        public KeyGeneratorView(object viewModel)
        {
            this.InitializeComponent();
            BrdTopBar.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(BrdTopBar_PreviewMouseLeftButtonDown);
            
            this.DataContext = viewModel;
        }

        #endregion


        #region Event
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            string licenseName = txtLicenseName.Text;
            string applicationId = txtApplicationID.Text;


            //int storeQty=0;
            //if (!string.IsNullOrWhiteSpace(txtAStoreQty.Text) && !int.TryParse(txtAStoreQty.Text,out storeQty))
            //{
            //    MessageBox.Show("Store Quatity is Interger");
            //    return;
            //}
            ////Addtional info 
            //int storeCode=0;
            //if (!string.IsNullOrWhiteSpace(txtStoreCode.Text) && !int.TryParse(txtStoreCode.Text, out storeCode))
            //{
            //    MessageBox.Show("Store Code is Interger");
            //    return;
            //}

            //int posId=0;
            //if (!string.IsNullOrWhiteSpace(txtPosId.Text) && !int.TryParse(txtPosId.Text, out posId))
            //{
            //    MessageBox.Show("POS ID is Interger");
            //    return;
            //}

            //DateTime? expireDate = dtExpireDate.SelectedDate;


            //LicenseModel licenseModel = new LicenseModel(licenseName, applicationId, storeQty, expireDate);

            //ProductKeyModel productKeyModel = new ProductKeyModel(licenseModel, storeCode, posId);
            //string keyGen = productKeyModel.ProductKey;

            //StringBuilder sb = new StringBuilder();

            //sb.AppendFormat("Rijindael Key : {0} ({1})\n", keyGen, keyGen.Length);

            //string descrytKey = ProductKeyGenerator.DescrytProductKey(keyGen);
            //sb.AppendFormat("Descryt Hex Key(License Key) : {0} ({1})\n", descrytKey, descrytKey.Length);

            //MessageBox.Show(sb.ToString());
        }

        void BrdTopBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        #endregion

    }
}