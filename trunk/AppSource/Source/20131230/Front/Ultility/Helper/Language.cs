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
                return Application.Current.FindResource("ErrorCaption") as string;
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
                return Application.Current.FindResource("WarningCaption") as string;
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
                return Application.Current.FindResource("InformationCaption") as string;
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
                return Application.Current.FindResource("DeleteCaption") as string;
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
                return Application.Current.FindResource("SaveCaption") as string;
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
                return Application.Current.FindResource("POSCaption") as string;
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
                return Application.Current.FindResource("M100") as string;
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
                return Application.Current.FindResource("M101") as string;
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
                return Application.Current.FindResource("M102") as string;
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
                return Application.Current.FindResource("M103") as string;
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
                return Application.Current.FindResource("M104") as string;
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
                return Application.Current.FindResource("M105") as string;
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
                return Application.Current.FindResource("M106") as string;
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
                return Application.Current.FindResource("M107") as string;
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
                return Application.Current.FindResource("M108") as string;
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
                return Application.Current.FindResource("M109") as string;
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
                return Application.Current.FindResource("M110") as string;
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
                return Application.Current.FindResource("M111") as string;
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
                return Application.Current.FindResource("M112") as string;
            }
        }
        #endregion

        /// <summary>
        /// You should close product view before apply to change pricing !
        /// </summary>
        #region Text14

        public static string Text14
        {
            get
            {
                return Application.Current.FindResource("M113") as string;
            }
        }
        #endregion

        /// <summary>
        /// You should close product view before apply to retore pricing !
        /// </summary>
        #region Text15

        public static string Text15
        {
            get
            {
                return Application.Current.FindResource("M114") as string;
            }
        }
        #endregion

        /// <summary>
        /// Categories have been edited, do you want to save?
        /// </summary>
        #region Text16

        public static string Text16
        {
            get
            {
                return Application.Current.FindResource("M115") as string;
            }
        }
        #endregion

        /// <summary>
        /// Scanner device not found!
        /// </summary>
        #region Text17

        public static string Text17
        {
            get
            {
                return Application.Current.FindResource("M116") as string;
            }
        }
        #endregion

        /// <summary>
        /// Cannot rename {0}. A folder with the name you specified already exists.
        /// </summary>
        #region Text18

        public static string Text18
        {
            get
            {
                return Application.Current.FindResource("M117") as string;
            }
        }
        #endregion

        /// <summary>
        /// Cannot rename {0}. A file with the name you specified already exists.
        /// </summary>
        #region Text19

        public static string Text19
        {
            get
            {
                return Application.Current.FindResource("M118") as string;
            }
        }
        #endregion


        /// <summary>
        /// Search Option is required.
        /// </summary>
        #region Text20

        public static string Text20
        {
            get
            {
                return Application.Current.FindResource("M119") as string;
            }
        }
        #endregion

        /// <summary>
        /// You should close product view before apply to adjust product(s) in store(s) !
        /// </summary>
        #region Text21

        public static string Text21
        {
            get
            {
                return Application.Current.FindResource("M120") as string;
            }
        }
        #endregion

        /// <summary>
        /// Do you want to adjust numbers of product(s) in store(s) ?
        /// </summary>
        #region Text22

        public static string Text22
        {
            get
            {
                return Application.Current.FindResource("M121") as string;
            }
        }
        #endregion


        /// <summary>
        ///  The products aren't counted.Do you want that application will count them ?"
        /// </summary>
        #region Text23

        public static string Text23
        {
            get
            {
                return Application.Current.FindResource("M122") as string;
            }
        }
        #endregion

        /// <summary>
        /// You should close product view before apply to transfer product(s) in store(s) !
        /// </summary>
        #region Text24

        public static string Text24
        {
            get
            {
                return Application.Current.FindResource("M123") as string;
            }
        }
        #endregion

        /// <summary>
        /// You should close product view before apply to revert product(s) in store(s) !
        /// </summary>
        #region Text25

        public static string Text25
        {
            get
            {
                return Application.Current.FindResource("M124") as string;
            }
        }
        #endregion

        /// <summary>
        /// Select folder used for save images.
        /// </summary>
        #region Text26

        public static string Text26
        {
            get
            {
                return Application.Current.FindResource("M126") as string;
            }
        }
        #endregion

        /// <summary>
        /// Select folder used for save backup files.
        /// </summary>
        #region Text27

        public static string Text27
        {
            get
            {
                return Application.Current.FindResource("M127") as string;
            }
        }
        #endregion

        /// <summary>
        /// Current total refund is less than reality total refund.
        /// </summary>
        #region Text28

        public static string Text28
        {
            get
            {
                return Application.Current.FindResource("M125") as string;
            }
        }
        #endregion

        #region Text29

        /// <summary>
        /// Size is existed
        /// </summary>
        public static string Text29
        {
            get
            {
                return Application.Current.FindResource("M128") as string;
            }
        }

        #endregion

        #region Text30

        /// <summary>
        /// Attribute is existed
        /// </summary>
        public static string Text30
        {
            get
            {
                return Application.Current.FindResource("M129") as string;
            }
        }

        #endregion

        #region Text31

        /// <summary>
        /// Size is required
        /// </summary>
        public static string Text31
        {
            get
            {
                return Application.Current.FindResource("M130") as string;
            }
        }

        #endregion

        #region Text32

        /// <summary>
        /// Attribute is required
        /// </summary>
        public static string Text32
        {
            get
            {
                return Application.Current.FindResource("M131") as string;
            }
        }

        #endregion

        #region Text33

        /// <summary>
        /// Attribute is required
        /// </summary>
        public static string Text33
        {
            get
            {
                return Application.Current.FindResource("M132") as string;
            }
        }

        #endregion

        #region Text34

        /// <summary>
        /// Attribute is required
        /// </summary>
        public static string Text34
        {
            get
            {
                return Application.Current.FindResource("M133") as string;
            }
        }

        #endregion

        #region Text35

        /// <summary>
        /// Attribute is required
        /// </summary>
        public static string Text35
        {
            get
            {
                return Application.Current.FindResource("M134") as string;
            }
        }

        #endregion

        #region Text36

        /// <summary>
        /// Attribute is required
        /// </summary>
        public static string Text36
        {
            get
            {
                return Application.Current.FindResource("M135") as string;
            }
        }

        #endregion

        #region Text37

        /// <summary>
        /// Attribute is required
        /// </summary>
        public static string Text37
        {
            get
            {
                return Application.Current.FindResource("M136") as string;
            }
        }

        #endregion

        #region Text38

        /// <summary>
        /// Attribute is required
        /// </summary>
        public static string Text38
        {
            get
            {
                return Application.Current.FindResource("M137") as string;
            }
        }

        #endregion

        #region Text39

        /// <summary>
        /// Attribute is required
        /// </summary>
        public static string Text39
        {
            get
            {
                return Application.Current.FindResource("M138") as string;
            }
        }

        #endregion

        #region Text40

        /// <summary>
        /// Attribute is required
        /// </summary>
        public static string Text40
        {
            get
            {
                return Application.Current.FindResource("M139") as string;
            }
        }

        #endregion

        /// <summary>
        /// Are you sure you want to complete item(s)?
        /// </summary>
        #region Text41

        public static string Text41
        {
            get
            {
                return Application.Current.FindResource("M140") as string;
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
                return Application.Current.FindResource("E100") as string;
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
                return Application.Current.FindResource("E101") as string;
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
                return Application.Current.FindResource("E102") as string;
            }
        }

        #endregion

        /// <summary>
        /// Name is required.
        /// </summary>
        #region Error4

        public static string Error4
        {
            get
            {
                return Application.Current.FindResource("E103") as string;
            }
        }

        #endregion

        /// <summary>
        /// Tax code is required.
        /// </summary>
        #region Error5

        public static string Error5
        {
            get
            {
                return Application.Current.FindResource("E104") as string;
            }
        }

        #endregion

        /// <summary>
        /// Company's name is required.
        /// </summary>
        #region Error6

        public static string Error6
        {
            get
            {
                return Application.Current.FindResource("E105") as string;
            }
        }

        #endregion

        /// <summary>
        /// Address is required.
        /// </summary>
        #region Error7

        public static string Error7
        {
            get
            {
                return Application.Current.FindResource("E106") as string;
            }
        }

        #endregion

        /// <summary>
        /// Required select country.
        /// </summary>
        #region Error8

        public static string Error8
        {
            get
            {
                return Application.Current.FindResource("E107") as string;
            }
        }

        #endregion

        /// <summary>
        /// City is required.
        /// </summary>
        #region Error9

        public static string Error9
        {
            get
            {
                return Application.Current.FindResource("E108") as string;
            }
        }

        #endregion

        /// <summary>
        /// Required select state.
        /// </summary>
        #region Error10

        public static string Error10
        {
            get
            {
                return Application.Current.FindResource("E109") as string;
            }
        }

        #endregion

        /// <summary>
        /// Zip is wrong format.
        /// </summary>
        #region Error11

        public static string Error11
        {
            get
            {
                return Application.Current.FindResource("E110") as string;
            }
        }

        #endregion

        /// <summary>
        /// Fax is wrong format.
        /// </summary>
        #region Error12

        public static string Error12
        {
            get
            {
                return Application.Current.FindResource("E111") as string;
            }
        }

        #endregion

        /// <summary>
        /// Phone is wrong format.
        /// </summary>
        #region Error13

        public static string Error13
        {
            get
            {
                return Application.Current.FindResource("E112") as string;
            }
        }

        #endregion

        /// <summary>
        /// Email is wrong format.
        /// </summary>
        #region Error14

        public static string Error14
        {
            get
            {
                return Application.Current.FindResource("E113") as string;
            }
        }

        #endregion

        /// <summary>
        /// Inexact password
        /// </summary>
        #region Error15

        public static string Error15
        {
            get
            {
                return Application.Current.FindResource("E114") as string;
            }
        }

        #endregion

        #endregion

        #region NoteModule

        #region AddSticky

        /// <summary>
        /// Gets the AddSticky string
        /// </summary>
        public static string AddSticky
        {
            get
            {
                return Application.Current.FindResource("N_AddSticky") as string;
            }
        }

        #endregion

        #region ShowStickies

        /// <summary>
        /// Gets the ShowStickies string  
        /// </summary>
        public static string ShowStickies
        {
            get
            {
                return Application.Current.FindResource("N_ShowStickies") as string;
            }
        }

        #endregion

        #region HideStickies

        /// <summary>
        /// Gets the HideStickies string  
        /// </summary>
        public static string HideStickies
        {
            get
            {
                return Application.Current.FindResource("N_HideStickies") as string;
            }
        }

        #endregion

        #endregion

        #region PopupAttributeAndSize

        #region DeleteAttribute

        /// <summary>
        /// Gets the DeleteAttribute string
        /// </summary>
        public static string DeleteAttribute
        {
            get
            {
                return Application.Current.FindResource("PD_DeleteAttribute") as string;
            }
        }

        #endregion

        #region DeleteSize

        /// <summary>
        /// Gets the DeleteSize string
        /// </summary>
        public static string DeleteSize
        {
            get
            {
                return Application.Current.FindResource("PD_DeleteSize") as string;
            }
        }

        #endregion

        #endregion

        #region MainView

        /// <summary>
        /// Gets the RealMode string
        /// </summary>
        public static string RealMode
        {
            get
            {
                return Application.Current.FindResource("Main_RealMode") as string;
            }
        }

        /// <summary>
        /// Gets the PracticeMode string
        /// </summary>
        public static string PracticeMode
        {
            get
            {
                return Application.Current.FindResource("Main_PracticeMode") as string;
            }
        }

        #endregion

        /// <summary>
        /// Get message with resource key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetMsg(string key)
        {
            return Application.Current.FindResource(key) as string;
        }
    }
}