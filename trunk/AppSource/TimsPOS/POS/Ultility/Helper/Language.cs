using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CPC.Helper
{
    public class Language
    {
        // Chuoi tieu de MessageBox.
        #region MessageBoxCaption

        /// <summary>
        /// Error.
        /// </summary>
        #region Error

        public static string Error
        {
            get
            {
                return Application.Current.FindResource("ErrorMessageBoxCaption") as string;
            }
        }

        #endregion

        /// <summary>
        /// Warning.
        /// </summary>
        #region Warning

        public static string Warning
        {
            get
            {
                return Application.Current.FindResource("WarningMessageBoxCaption") as string;
            }
        }

        #endregion

        /// <summary>
        /// Information.
        /// </summary>
        #region Information

        public static string Information
        {
            get
            {
                return Application.Current.FindResource("InformationMessageBoxCaption") as string;
            }
        }

        #endregion

        /// <summary>
        /// Delete item(s).
        /// </summary>
        #region DeleteItems

        public static string DeleteItems
        {
            get
            {
                return Application.Current.FindResource("DeleteItemsMessageBoxCaption") as string;
            }
        }

        #endregion

        /// <summary>
        /// Save.
        /// </summary>
        #region Save

        public static string Save
        {
            get
            {
                return Application.Current.FindResource("SaveMessageBoxCaption") as string;
            }
        }

        #endregion

        /// <summary>
        /// POS.
        /// </summary>
        #region POS

        public static string POS
        {
            get
            {
                return Application.Current.FindResource("POSMessageBoxCaption") as string;
            }
        }

        #endregion

        #endregion

        // Thong diep trong MessageBox.
        #region MessageBoxText

        /// <summary>
        /// Order quantity must Greater than or equal receive quantity.
        /// </summary>
        #region Text1

        public static string Text1
        {
            get
            {
                return Application.Current.FindResource("Text1MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Product name: {0} is marked  as 'Unorderable' in inventory. Are you sure you want to add this item ?
        /// </summary>
        #region Text2

        public static string Text2
        {
            get
            {
                return Application.Current.FindResource("Text2MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Exists an item has been received in this purchase order, can not delete purchase order.
        /// </summary>
        #region Text3

        public static string Text3
        {
            get
            {
                return Application.Current.FindResource("Text3MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Are you sure you want to delete item(s)?
        /// </summary>
        #region Text4

        public static string Text4
        {
            get
            {
                return Application.Current.FindResource("Text4MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Item has been received in this purchase order, can not delete this item.
        /// </summary>
        #region Text5

        public static string Text5
        {
            get
            {
                return Application.Current.FindResource("Text5MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Item has been returned in this purchase order, can not delete this item.
        /// </summary>
        #region Text6

        public static string Text6
        {
            get
            {
                return Application.Current.FindResource("Text6MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Some data has been changed. Do you want to save?
        /// </summary>
        #region Text7

        public static string Text7
        {
            get
            {
                return Application.Current.FindResource("Text7MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Fix error in current TabItem before change TabItem.
        /// </summary>
        #region Text8

        public static string Text8
        {
            get
            {
                return Application.Current.FindResource("Text8MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Are you sure you received this item ?
        /// </summary>
        #region Text9

        public static string Text9
        {
            get
            {
                return Application.Current.FindResource("Text9MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Fix error(s) before receive this item.
        /// </summary>
        #region Text10

        public static string Text10
        {
            get
            {
                return Application.Current.FindResource("Text10MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Are you sure you return this item ?
        /// </summary>
        #region Text11

        public static string Text11
        {
            get
            {
                return Application.Current.FindResource("Text11MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Fix error(s) before return this item.
        /// </summary>
        #region Text12

        public static string Text12
        {
            get
            {
                return Application.Current.FindResource("Text12MessageBoxText") as string;
            }
        }

        #endregion

        /// <summary>
        /// Some data has changed. Do you want to save ?
        /// </summary>
        #region Text13

        public static string Text13
        {
            get
            {
                return Application.Current.FindResource("Text13MessageBoxText") as string;
            }
        }
        #endregion
        /// <summary>
        /// When you apply that change pricing , You should close product view.
        /// </summary>
        #region Text14

        public static string Text14
        {
            get
            {
                return Application.Current.FindResource("Text14MessageBoxText") as string;
            }
        }
        #endregion
        /// <summary>
        /// When you apply that restore pricing , You should close product view.
        /// </summary>
        #region Text15

        public static string Text15
        {
            get
            {
                return Application.Current.FindResource("Text15MessageBoxText") as string;
            }
        }
        #endregion

        #endregion

        // Chuoi thong bao loi trong Validation.
        #region ErrorMessage

        /// <summary>
        /// Please select an item.
        /// </summary>
        #region Error1

        public static string Error1
        {
            get
            {
                return Application.Current.FindResource("Error1ErrorMessage") as string;
            }
        }

        #endregion

        /// <summary>
        /// Received quantity must greater than 0.
        /// </summary>
        #region Error2

        public static string Error2
        {
            get
            {
                return Application.Current.FindResource("Error2ErrorMessage") as string;
            }
        }

        #endregion

        /// <summary>
        /// Order quantity must Greater than or equal receive quantity.
        /// </summary>
        #region Error3

        public static string Error3
        {
            get
            {
                return Application.Current.FindResource("Error3ErrorMessage") as string;
            }
        }

        #endregion

        #endregion
    }
}
