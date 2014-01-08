using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class AddSerialNumberViewModel : ViewModelBase
    {
        #region Define

        public RelayCommand OKCommand
        {
            get;
            private set;
        }
        public RelayCommand CancelCommand
        {
            get;
            private set;
        }

        private base_ProductRepository _productRepository = new base_ProductRepository();

        #endregion

        #region Constructors

        public AddSerialNumberViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();
        }

        #endregion

        #region Properties

        #region PurchaseOrderDetailModel

        private base_PurchaseOrderDetailModel _purchaseOrderDetailModel;
        /// <summary>
        /// Gets or sets the PurchaseOrderDetailModel.
        /// </summary>
        public base_PurchaseOrderDetailModel PurchaseOrderDetailModel
        {
            get
            {
                return _purchaseOrderDetailModel;
            }
            set
            {
                if (_purchaseOrderDetailModel != value)
                {
                    _purchaseOrderDetailModel = value;
                    OnPropertyChanged(() => PurchaseOrderDetailModel);
                    PurchaseOrderDetailModelChanged();
                }
            }
        }

        private void PurchaseOrderDetailModelChanged()
        {
            base_Product product = _productRepository.GetAll().ToList().SingleOrDefault(x => x.Resource.ToString().Equals(PurchaseOrderDetailModel.ProductResource));
            //Get Product 
            ProductModel = new base_ProductModel(product);
            if (IsShowQuantity)
                Quantity = PurchaseOrderDetailModel.Quantity;

            //Backup Model
            SelectedPurchaseOrderDetail = Clone(PurchaseOrderDetailModel) as base_PurchaseOrderDetailModel;

            PurchaseOrderDetailCollection = new ObservableCollection<base_PurchaseOrderDetailModel>();
            //set PurchaseOrderDetailModel to New item
            PurchaseOrderDetailCollection.Add(SelectedPurchaseOrderDetail);
            if (PurchaseOrderDetailModel.Quantity > 1)
            {
                for (int i = 0; i < PurchaseOrderDetailModel.Quantity - 1; i++)
                {
                    base_PurchaseOrderDetailModel newPurchaseOrderDetail = NewPurchaseOrderDetail();
                    //Add New Item
                    PurchaseOrderDetailCollection.Add(newPurchaseOrderDetail);
                }
            }


        }

        #endregion

        #region SelectedPurchaseOrderDetail

        private base_PurchaseOrderDetailModel _selectedPurchaseOrderDetail;
        /// <summary>
        /// Gets or sets the SelectedPurchaseOrderDetail.
        /// </summary>
        public base_PurchaseOrderDetailModel SelectedPurchaseOrderDetail
        {
            get
            {
                return _selectedPurchaseOrderDetail;
            }
            set
            {
                if (_selectedPurchaseOrderDetail != value)
                {
                    _selectedPurchaseOrderDetail = value;
                    OnPropertyChanged(() => SelectedPurchaseOrderDetail);
                }
            }
        }

        #endregion

        #region PurchaseOrderDetailCollection

        private ObservableCollection<base_PurchaseOrderDetailModel> _purchaseOrderDetailCollection = new ObservableCollection<base_PurchaseOrderDetailModel>();
        /// <summary>
        /// Gets or sets the PurchaseOrderDetailCollection.
        /// </summary>
        public ObservableCollection<base_PurchaseOrderDetailModel> PurchaseOrderDetailCollection
        {
            get
            {
                return _purchaseOrderDetailCollection;
            }
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

        #region ProductModel

        private base_ProductModel _productModel;
        /// <summary>
        /// Gets or sets the ProductModel.
        /// </summary>
        public base_ProductModel ProductModel
        {
            get
            {
                return _productModel;
            }
            set
            {
                if (_productModel != value)
                {
                    _productModel = value;
                    OnPropertyChanged(() => ProductModel);
                }
            }
        }

        #endregion

        #region Quantity

        private decimal _quantity;
        /// <summary>
        /// Gets or sets the Quantity.
        /// </summary>
        public decimal Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(() => Quantity);
                }
            }
        }

        #endregion

        #region IsShowQuantity

        private bool _isShowQuantity = false;
        /// <summary>
        /// Gets or sets the IsShowQuantity.
        /// </summary>
        public bool IsShowQuantity
        {
            get
            {
                return _isShowQuantity;
            }
            set
            {
                if (_isShowQuantity != value)
                {
                    _isShowQuantity = value;
                    OnPropertyChanged(() => IsShowQuantity);
                }
            }
        }

        #endregion

        #endregion

        #region Commands Methods

        #region OKCommand

        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOKCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnOKCommandExecute()
        {
            Window window = FindOwnerWindow(_ownerViewModel);

            if (PurchaseOrderDetailCollection.Any())
            {
                // Count item have null serial.
                int qty = PurchaseOrderDetailCollection.Count(x => string.IsNullOrWhiteSpace(x.Serial));

                // Remove item have null serial.
                foreach (var item in PurchaseOrderDetailCollection.Where(x => string.IsNullOrWhiteSpace(x.Serial)).ToList())
                    PurchaseOrderDetailCollection.Remove(item);

                if (!string.IsNullOrWhiteSpace(SelectedPurchaseOrderDetail.Serial))
                    SelectedPurchaseOrderDetail.Quantity = 1;
                else
                    SelectedPurchaseOrderDetail.Quantity = qty;

                if (qty > 0 && !string.IsNullOrWhiteSpace(SelectedPurchaseOrderDetail.Serial))
                {
                    base_PurchaseOrderDetailModel purchaseOrderDetailModel = NewPurchaseOrderDetail();
                    purchaseOrderDetailModel.Quantity = qty;
                    PurchaseOrderDetailCollection.Add(purchaseOrderDetailModel);
                }
            }
            PurchaseOrderDetailModel = ConvertObject(SelectedPurchaseOrderDetail, PurchaseOrderDetailModel) as base_PurchaseOrderDetailModel;
            window.DialogResult = true;
        }

        #endregion

        #region Cancel Command

        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = FindOwnerWindow(_ownerViewModel);
            window.DialogResult = false;
        }

        #endregion

        #region QuatityChangedCommand

        /// <summary>
        /// Gets the QuatityChanged Command.
        /// <summary>
        public RelayCommand<object> QuatityChangedCommand
        {
            get;
            private set;
        }


        /// <summary>
        /// Method to check whether the QuatityChanged command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnQuatityChangedCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the QuatityChanged command is executed.
        /// </summary>
        private void OnQuatityChangedCommandExecute(object param)
        {
            if ("Enter".Equals(param.ToString()))
            {
                SelectedPurchaseOrderDetail.Quantity = Quantity;
                if (PurchaseOrderDetailCollection.Count < Quantity)
                {
                    decimal numberItemAdded = Quantity - PurchaseOrderDetailCollection.Count;
                    for (int i = 0; i < numberItemAdded; i++)
                        PurchaseOrderDetailCollection.Add(NewPurchaseOrderDetail());
                }
                else if (PurchaseOrderDetailCollection.Count > Quantity)
                {
                    int numberItemRemove = PurchaseOrderDetailCollection.Count - (int)Quantity;
                    for (int i = 0; i < numberItemRemove; i++)
                        PurchaseOrderDetailCollection.RemoveAt(PurchaseOrderDetailCollection.Count() - 1);
                }
            }
            else if ("LostFocus".Equals(param.ToString()))
            {
                Quantity = SelectedPurchaseOrderDetail.Quantity;
            }

        }

        #endregion

        #endregion

        #region Private Methods

        private void InitialCommand()
        {
            // Route the commands
            OKCommand = new RelayCommand(OnOKCommandExecute, OnOKCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
            QuatityChangedCommand = new RelayCommand<object>(OnQuatityChangedCommandExecute, OnQuatityChangedCommandCanExecute);
        }

        private base_PurchaseOrderDetailModel NewPurchaseOrderDetail()
        {
            base_PurchaseOrderDetailModel newPurchaseOrderDetail = new base_PurchaseOrderDetailModel();
            newPurchaseOrderDetail.UOMCollection = _purchaseOrderDetailModel.UOMCollection;
            newPurchaseOrderDetail.PurchaseOrderId = _purchaseOrderDetailModel.PurchaseOrderId;
            newPurchaseOrderDetail.ProductResource = _purchaseOrderDetailModel.ProductResource;
            newPurchaseOrderDetail.ItemCode = _purchaseOrderDetailModel.ItemCode;
            newPurchaseOrderDetail.ItemName = _purchaseOrderDetailModel.ItemName;
            newPurchaseOrderDetail.ItemAtribute = _purchaseOrderDetailModel.ItemAtribute;
            newPurchaseOrderDetail.ItemSize = _purchaseOrderDetailModel.ItemSize;
            newPurchaseOrderDetail.IsSerialTracking = _purchaseOrderDetailModel.IsSerialTracking;
            newPurchaseOrderDetail.Price = _purchaseOrderDetailModel.Price;
            newPurchaseOrderDetail.UOMId = _purchaseOrderDetailModel.UOMId;
            newPurchaseOrderDetail.UnitName = _purchaseOrderDetailModel.UnitName;
            newPurchaseOrderDetail.BaseUOM = _purchaseOrderDetailModel.BaseUOM;
            newPurchaseOrderDetail.Price = _purchaseOrderDetailModel.Price;
            newPurchaseOrderDetail.Quantity = 1;
            newPurchaseOrderDetail.Resource = Guid.NewGuid();
            return newPurchaseOrderDetail;
        }

        public static object ConvertObject(object object_1, object object_2)
        {
            // Get all the fields of the type, also the privates.
            // Loop through all the fields and copy the information from the parameter class
            // to the newPerson class.
            foreach (PropertyInfo oPropertyInfo in
                object_1.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.CanRead && x.CanWrite)
                .Where(x => x.GetSetMethod(true).IsPublic))
            {
                oPropertyInfo.SetValue(object_2, oPropertyInfo.GetValue(object_1, null), null);
            }
            // Return the cloned object.
            return object_2;
        }

        public static object Clone(object obj)
        {
            try
            {

                // an instance of target type.
                object _object = (object)Activator.CreateInstance(obj.GetType());
                //To get type of value.
                Type type = obj.GetType();
                //To Copy value from input value.
                foreach (PropertyInfo oPropertyInfo in
                    type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.CanRead && x.CanWrite)
                    .Where(x => x.GetSetMethod(true).IsPublic))
                {
                    oPropertyInfo.SetValue(_object, type.GetProperty(oPropertyInfo.Name).GetValue(obj, null), null);
                }

                return _object;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        #endregion
    }
}