using System;
using System.Collections.Generic;
using System.Linq;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class MultiTrackingNumberViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand OkCommand { get; private set; }

        enum PopupTypes
        {
            SaleOrder = 1,
            PurchaseOrder = 2
        }

        private PopupTypes Popup { get; set; }

        /// <summary>
        /// Flag break form closing process
        /// Turn on when click ok command
        /// </summary>
        private bool _acceptedClosing = false;
        #endregion

        #region Constructors
        public MultiTrackingNumberViewModel()
            : base()
        {
            _ownerViewModel = this;
            InitialCommand();
        }
        /// <summary>
        /// Using for SaleOrder
        /// </summary>
        /// <param name="saleOrderDetailCollection">Collection Sale Order Detail has Serial tracking</param>
        public MultiTrackingNumberViewModel(IEnumerable<base_SaleOrderDetailModel> saleOrderDetailCollection)
            : this()
        {
            Popup = PopupTypes.SaleOrder;
            SaleOrderDetailCollection = saleOrderDetailCollection;
            foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderDetailCollection)
            {
                this.SerialCollection.Add(new ItemModel()
                {
                    Id = saleOrderDetailModel.Resource,
                    Resource = string.Format("{0} ({1} / {2})", saleOrderDetailModel.ProductModel.ProductName, saleOrderDetailModel.ItemAtribute, saleOrderDetailModel.ItemSize),
                    Text = string.Empty
                });
            }
        }

        /// <summary>
        /// Using for PurchaseOrder
        /// </summary>
        /// <param name="purchaseOrderDetailCollection">Collection Purchase Order Detail has Serial tracking</param>
        public MultiTrackingNumberViewModel(IEnumerable<base_PurchaseOrderDetailModel> purchaseOrderDetailCollection)
            : this()
        {
            Popup = PopupTypes.PurchaseOrder;
            PurchaseOrderDetailCollection = purchaseOrderDetailCollection;
            foreach (base_PurchaseOrderDetailModel purchaseOrderDetailModel in purchaseOrderDetailCollection)
            {
                this.SerialCollection.Add(new ItemModel()
                {
                    Id = purchaseOrderDetailModel.Resource,
                    Resource = string.Format("{0} ({1} / {2})", purchaseOrderDetailModel.ItemName, purchaseOrderDetailModel.ItemAtribute, purchaseOrderDetailModel.ItemSize),
                    Text = string.Empty
                });
            }
        }
        #endregion

        #region Properties

        #region SerialCollection
        private CollectionBase<ItemModel> _serialCollection = new CollectionBase<ItemModel>();
        /// <summary>
        /// Gets or sets the SerialTrackingNumber.
        /// <para>Id : Resource Detail SO or PO</para>
        /// <para>Resource : Product Text</para>
        /// <para>Text : Serial Text</para>
        /// </summary>
        public CollectionBase<ItemModel> SerialCollection
        {
            get { return _serialCollection; }
            set
            {
                if (_serialCollection != value)
                {
                    _serialCollection = value;
                    OnPropertyChanged(() => SerialCollection);
                }
            }
        }
        #endregion

        #region SaleOrderDetailCollection
        private IEnumerable<base_SaleOrderDetailModel> _saleOrderDetailCollection;
        /// <summary>
        /// Gets or sets the SaleOrderDetailCollection.
        /// </summary>
        public IEnumerable<base_SaleOrderDetailModel> SaleOrderDetailCollection
        {
            get { return _saleOrderDetailCollection; }
            set
            {
                if (_saleOrderDetailCollection != value)
                {
                    _saleOrderDetailCollection = value;
                    OnPropertyChanged(() => SaleOrderDetailCollection);
                }
            }
        }
        #endregion

        #region PurchaseOrderDetailCollection
        private IEnumerable<base_PurchaseOrderDetailModel> _purchaseOrderDetailCollection;
        /// <summary>
        /// Gets or sets the PurchaseOrderDetailCollection.
        /// </summary>
        public IEnumerable<base_PurchaseOrderDetailModel> PurchaseOrderDetailCollection
        {
            get { return _purchaseOrderDetailCollection; }
            set
            {
                if (_purchaseOrderDetailCollection != value)
                {
                    _purchaseOrderDetailCollection = value;
                    OnPropertyChanged(() => PurchaseOrderDetailCollection);
                }
            }
        }
        #endregion


        #endregion

        #region Commands Methods

        #region OkCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            if (!SerialCollection.Any())
                return true;
            return SerialCollection.Count(x => string.IsNullOrWhiteSpace(x.Text)) == 0;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            switch (Popup)
            {
                case PopupTypes.SaleOrder:
                    foreach (ItemModel itemModel in SerialCollection)
                    {
                        Guid guidID = Guid.Parse(itemModel.Id.ToString());
                        base_SaleOrderDetailModel saleOrderDetailModel = SaleOrderDetailCollection.SingleOrDefault(x => x.Resource.Equals(guidID));
                        if (saleOrderDetailModel != null)
                            saleOrderDetailModel.SerialTracking = itemModel.Text;
                    }
                    break;
                case PopupTypes.PurchaseOrder:
                    foreach (ItemModel itemModel in SerialCollection)
                    {
                        Guid guidID = Guid.Parse(itemModel.Id.ToString());
                        base_PurchaseOrderDetailModel purchaseOrderDetailModel = PurchaseOrderDetailCollection.SingleOrDefault(x => x.Resource.Equals(guidID));
                        if (purchaseOrderDetailModel != null)
                            purchaseOrderDetailModel.Serial = itemModel.Text;
                    }
                    break;

            }
            _acceptedClosing = true;
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
        }
        #endregion


        #region Override Methods
        protected override bool CanExecuteClosing()
        {
            return _acceptedClosing;
        }
        #endregion
    }
}