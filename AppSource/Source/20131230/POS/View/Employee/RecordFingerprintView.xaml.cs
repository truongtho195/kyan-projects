using System.Windows;
using System.Windows.Input;
using CPC.POS.ViewModel;
namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for PopupConfirmSSN.xaml
    /// </summary>
    public partial class RecordFingerprintView 
    {
        #region Properties
        //public byte[] Temp
        //{
        //    get { return viewModel.Temp; }
        //}

        //public int FingerID
        //{
        //    get { return viewModel.FingerID; }
        //    set
        //    {
        //        if (viewModel.FingerID != value)
        //        {
        //            viewModel.FingerID = value;
        //            viewModel.IsEdit = false;
        //        }
        //    }
        //}

        //public bool IsLeft
        //{
        //    get { return viewModel.IsLeft; }
        //    set
        //    {
        //        if (viewModel.IsLeft != value)
        //        {
        //            viewModel.IsLeft = value;
        //        }
        //    }
        //}

        //protected RecordFingerprintViewModel viewModel
        //{
        //    get { return this.DataContext as RecordFingerprintViewModel; }
        //}
        #endregion

        #region Constructors
        public RecordFingerprintView()
        {
            this.InitializeComponent();
        }
        #endregion

        #region Events

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            //this.DragMove();

            base.OnMouseLeftButtonDown(e);
        }

        #endregion
    }
}