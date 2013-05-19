using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Windows;
using CPC.POS.Model;
using CPC.POS.Repository;
using System.Reflection;
using CPC.POS.Database;
using System.Data.Objects.SqlClient;
using System.Collections.ObjectModel;

namespace CPC.POS.ViewModel
{
    class SelectTrackingNumberViewModel : ViewModelBase
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
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();
        private base_GuestRepository _guestRepository = new base_GuestRepository();



        #endregion

        #region Constructors
        public SelectTrackingNumberViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();
        }

        public SelectTrackingNumberViewModel(base_SaleOrderDetailModel saleOrderDetailModel, bool isShowQuantity = false)
            : this()
        {
            IsSaleOrder = true;
            SaleOrderDetailModel = saleOrderDetailModel;
            IsShowQuantity = isShowQuantity;
            SaleOrderDetailModelChanged();
        }

        public SelectTrackingNumberViewModel(base_PurchaseOrderDetailModel purchaseOrderDetailModel, bool isShowQuantity = false)
            : this()
        {
            IsSaleOrder = false;
            PurchaseOrderDetailModel = purchaseOrderDetailModel;
            IsShowQuantity = isShowQuantity;
            PurchaseOrderDetailModelChanged();
        }
        #endregion

        #region Properties

        public bool IsSaleOrder
        {
            get;
            set;
        }

        #region SaleOrderDetailModel
        private base_SaleOrderDetailModel _saleOrderDetailModel;
        /// <summary>
        /// Gets or sets the SaleOrderDetailModel.
        /// </summary>
        public base_SaleOrderDetailModel SaleOrderDetailModel
        {
            get
            {
                return _saleOrderDetailModel;
            }
            set
            {
                if (_saleOrderDetailModel != value)
                {
                    _saleOrderDetailModel = value;
                    OnPropertyChanged(() => SaleOrderDetailModel);
                }
            }
        }

        private void SaleOrderDetailModelChanged()
        {
            base_Product product = _productRepository.GetAll().ToList().SingleOrDefault(x => x.Resource.ToString().Equals(SaleOrderDetailModel.ProductResource));
            _departmentRepository.Get(x => x.ParentId == 1 && x.Id == product.ProductCategoryId);
            //Get Product 
            ProductModel = new base_ProductModel(product);
            //Get Category
            base_Department category = _departmentRepository.Get(x => x.LevelId == Define.ProductCategoryLevel && x.Id == product.ProductCategoryId);
            if (category != null)
                ProductModel.CategoryName = category.Name;

            //Get VendorName
            base_GuestModel vendorModel = new base_GuestModel(_guestRepository.Get(x => x.Id == ProductModel.VendorId));
            if (vendorModel != null)
                ProductModel.VendorName = vendorModel.LegalName;
            Quantity = SaleOrderDetailModel.Quantity;

            //Backup Model
            BackupObject = Clone(SaleOrderDetailModel) as base_SaleOrderDetailModel;

            if (string.IsNullOrWhiteSpace(SaleOrderDetailModel.SerialTracking))
            {
                AddRowForSerialCollection(SaleOrderDetailModel.Quantity);
            }
            else
            {
                var serialList = SaleOrderDetailModel.SerialTracking.Split(',');
                foreach (string item in serialList.Take(Quantity))
                {
                    ItemModel itemModel = new ItemModel();
                    itemModel.Text = item.Trim();
                    itemModel.EndUpdate();
                    SerialTrackingCollection.Add(itemModel);
                }

                if (SerialTrackingCollection.Count() < SaleOrderDetailModel.Quantity)//Add more row follow quantity
                {
                    int remainRow = SaleOrderDetailModel.Quantity - SerialTrackingCollection.Count();
                    AddRowForSerialCollection(remainRow);
                }
            }
        }

        private void PurchaseOrderDetailModelChanged()
        {
            base_Product product = _productRepository.GetAll().ToList().SingleOrDefault(x => x.Resource.ToString().Equals(PurchaseOrderDetailModel.ProductResource));
            _departmentRepository.Get(x => x.ParentId == 1 && x.Id == product.ProductCategoryId);
            //Get Product 
            ProductModel = new base_ProductModel(product);

            //Get CategoryName
            base_Department category = _departmentRepository.Get(x => x.LevelId == Define.ProductCategoryLevel && x.Id == product.ProductCategoryId);
            if (category != null)
                ProductModel.CategoryName = category.Name;

            //Get VendorName
            base_GuestModel vendorModel = new base_GuestModel(_guestRepository.Get(x => x.Id == ProductModel.VendorId));
            if (vendorModel != null)
                ProductModel.VendorName = vendorModel.LegalName;

            Quantity = PurchaseOrderDetailModel.Quantity;

            //Backup Model
            BackupObject = Clone(PurchaseOrderDetailModel) as base_PurchaseOrderDetailModel;

            if (string.IsNullOrWhiteSpace(PurchaseOrderDetailModel.Serial))
            {
                AddRowForSerialCollection(PurchaseOrderDetailModel.Quantity);
            }
            else
            {
                var serialList = PurchaseOrderDetailModel.Serial.Split(',');
                foreach (string item in serialList.Take(Quantity))
                {
                    ItemModel itemModel = new ItemModel();
                    itemModel.Text = item.Trim();
                    itemModel.EndUpdate();
                    SerialTrackingCollection.Add(itemModel);
                }

                if (SerialTrackingCollection.Count() < PurchaseOrderDetailModel.Quantity)//Add more row follow quantity
                {
                    int remainRow = PurchaseOrderDetailModel.Quantity - SerialTrackingCollection.Count();
                    AddRowForSerialCollection(remainRow);
                }
            }
        }

        private void AddRowForSerialCollection(int qty)
        {
            if (qty > 0)
            {
                for (int i = 0; i < qty; i++)
                {
                    SerialTrackingCollection.Add(new ItemModel());
                }
            }
        }
        #endregion

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
                }
            }
        }
        #endregion

        #region BackupObject
        private object _backupObject;
        /// <summary>
        /// Gets or sets the SaleOrderDetailModel.
        /// </summary>
        public object BackupObject
        {
            get
            {
                return _backupObject;
            }
            set
            {
                if (_backupObject != value)
                {
                    _backupObject = value;
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
        private int _quantity;
        /// <summary>
        /// Gets or sets the Quantity.
        /// </summary>
        public int Quantity
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

        #region SerialTrackingCollection
        private ObservableCollection<ItemModel> _serialTrackingCollection = new ObservableCollection<ItemModel>();
        /// <summary>
        /// Gets or sets the SerialTrackingCollection.
        /// </summary>
        public ObservableCollection<ItemModel> SerialTrackingCollection
        {
            get
            {
                return _serialTrackingCollection;
            }
            set
            {
                if (_serialTrackingCollection != value)
                {
                    _serialTrackingCollection = value;
                    OnPropertyChanged(() => SerialTrackingCollection);
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
            if (SerialTrackingCollection == null)
                return false;
            return IsValid && SerialTrackingCollection.Count(x => !string.IsNullOrWhiteSpace(x.Text)) == Quantity;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnOKCommandExecute()
        {
            Window window = FindOwnerWindow(_ownerViewModel);

            if (IsSaleOrder)
            {
                (BackupObject as base_SaleOrderDetailModel).SerialTracking = string.Join(", ", SerialTrackingCollection.Where(x => !string.IsNullOrWhiteSpace(x.Text)).Select(x => x.Text.Trim()));
                SaleOrderDetailModel = ConvertObject(BackupObject, SaleOrderDetailModel) as base_SaleOrderDetailModel;
            }
            else
            {
                (BackupObject as base_PurchaseOrderDetailModel).Serial = string.Join(", ", SerialTrackingCollection.Where(x => !string.IsNullOrWhiteSpace(x.Text)).Select(x => x.Text.Trim()));
                PurchaseOrderDetailModel = ConvertObject(BackupObject, PurchaseOrderDetailModel) as base_PurchaseOrderDetailModel;
            }

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
            if (Quantity > 0)
            {
                if (IsSaleOrder)
                    (BackupObject as base_SaleOrderDetailModel).Quantity = Quantity;
                else
                    (BackupObject as base_PurchaseOrderDetailModel).Quantity = Quantity;
                if (SerialTrackingCollection.Count < Quantity)
                {
                    int numberItemAdded = Quantity - SerialTrackingCollection.Count;
                    for (int i = 0; i < numberItemAdded; i++)
                        SerialTrackingCollection.Add(new ItemModel());
                }
                else if (SerialTrackingCollection.Count > Quantity)
                {
                    int numberItemRemove = SerialTrackingCollection.Count - Quantity;
                    for (int i = 0; i < numberItemRemove; i++)
                        SerialTrackingCollection.RemoveAt(SerialTrackingCollection.Count() - 1);
                }
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (IsSaleOrder)
                        Quantity = (BackupObject as base_SaleOrderDetailModel).Quantity;
                    else
                        Quantity = (BackupObject as base_PurchaseOrderDetailModel).Quantity;
                }));
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
