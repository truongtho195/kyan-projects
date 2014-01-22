using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.Helper;
using System.Windows;

namespace CPC.POS.ViewModel
{
    class PickPackViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }



        private ICollectionView collectionView;

        private bool IsEdit = false;

        #endregion

        #region Constructors
        public PickPackViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();
        }
        public PickPackViewModel(IEnumerable<base_SaleOrderDetailModel> saleOrderDetailCollection)
            : this()
        {
            IsEdit = false;
            if (saleOrderDetailCollection.Any(x => x.DueQty > 0))
            {
                SaleOrderDetailList = new CollectionBase<base_SaleOrderDetailModel>();
                foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderDetailCollection.CloneList())
                    SaleOrderDetailList.Add(saleOrderDetailModel);

                SaleOrderDetailCollection = SaleOrderDetailList;

                foreach (base_SaleOrderDetailModel saleOrderDetailModel in SaleOrderDetailCollection.Where(x => x.DueQty > 0).OrderBy(x=>x.Id))
                {
                    if (saleOrderDetailModel.ProductModel == null)
                        continue;

                    //Set item is parent with Product is Group
                    saleOrderDetailModel.IsParent = (saleOrderDetailModel.ProductModel != null && saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group));

                    //Set color
                    if (saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))//Parent Of Group
                        saleOrderDetailModel.ItemType = 1;
                    else if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource))//Child item of group
                        saleOrderDetailModel.ItemType = 2;
                    else
                        saleOrderDetailModel.ItemType = 0;

                    saleOrderDetailModel.IsPickProcess = true;
                    saleOrderDetailModel.QtyOfPick = saleOrderDetailModel.DueQty;
                }

                collectionView = CollectionViewSource.GetDefaultView(SaleOrderDetailCollection);
                collectionView.Filter = o =>
                {
                    return (o as base_SaleOrderDetailModel).DueQty > 0;
                };
            }
        }

        public PickPackViewModel(IEnumerable<base_SaleOrderDetailModel> saleOrderDetailCollection, base_SaleOrderShipModel saleOrderShipModel, bool isView = false)
            : this()
        {
            IsEdit = true;
            IsView = isView;

            SaleOrderDetailList = new CollectionBase<base_SaleOrderDetailModel>();
            foreach (base_SaleOrderDetailModel saleOrderDetailModel in saleOrderDetailCollection.CloneList())
                SaleOrderDetailList.Add(saleOrderDetailModel);

            SaleOrderDetailCollection = SaleOrderDetailList as CollectionBase<base_SaleOrderDetailModel>;

            SaleOrderShipModel = saleOrderShipModel;
            foreach (base_SaleOrderDetailModel saleOrderDetailModel in SaleOrderDetailCollection.OrderBy(x=>x.Id))
            {
                //Set item is parent with Product is Group
                saleOrderDetailModel.IsParent = (saleOrderDetailModel.ProductModel != null && saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group));

                //Set color
                if (saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))//Parent Of Group
                    saleOrderDetailModel.ItemType = 1;
                else if (!string.IsNullOrWhiteSpace(saleOrderDetailModel.ParentResource))//Child item of group
                    saleOrderDetailModel.ItemType = 2;
                else
                    saleOrderDetailModel.ItemType = 0;


                saleOrderDetailModel.IsPickProcess = true;
                decimal qtyPacked = 0;
                //Item Group is not pick or pack
                if(!saleOrderDetailModel.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group))
                   qtyPacked= SaleOrderShipModel.SaleOrderShipDetailCollection.SingleOrDefault(x => x.SaleOrderDetailResource == saleOrderDetailModel.Resource.ToString()).PackedQty;

                saleOrderDetailModel.PickQty -= qtyPacked;
                saleOrderDetailModel.QtyOfPick = qtyPacked;
                saleOrderDetailModel.CalcDueQty();
            }
        }


        #endregion

        #region Properties

        #region SaleOrderDetailCollection
        private CollectionBase<base_SaleOrderDetailModel> _saleOrderdDetailCollection;
        /// <summary>
        /// Gets or sets the SaleOrderDetailCollection.
        /// </summary>
        public CollectionBase<base_SaleOrderDetailModel> SaleOrderDetailCollection
        {
            get { return _saleOrderdDetailCollection; }
            set
            {
                if (_saleOrderdDetailCollection != value)
                {
                    _saleOrderdDetailCollection = value;
                    OnPropertyChanged(() => SaleOrderDetailCollection);
                }
            }
        }
        #endregion

        #region SaleOrderDetailList
        private CollectionBase<base_SaleOrderDetailModel> _saleOrderDetailList;
        /// <summary>
        /// Gets or sets the SaleOrderDetailList.
        /// </summary>
        public CollectionBase<base_SaleOrderDetailModel> SaleOrderDetailList
        {
            get { return _saleOrderDetailList; }
            set
            {
                if (_saleOrderDetailList != value)
                {
                    _saleOrderDetailList = value;
                    OnPropertyChanged(() => SaleOrderDetailList);
                }
            }
        }
        #endregion

        #region SaleOrderShipModel
        private base_SaleOrderShipModel _saleOrderShipModel;
        /// <summary>
        /// Gets or sets the SaleOrderShipCollection.
        /// </summary>
        public base_SaleOrderShipModel SaleOrderShipModel
        {
            get { return _saleOrderShipModel; }
            set
            {
                if (_saleOrderShipModel != value)
                {
                    _saleOrderShipModel = value;
                    OnPropertyChanged(() => SaleOrderShipModel);
                }
            }
        }
        #endregion

        #region IsOrderValid
        /// <summary>
        /// Gets the IsShipValid.
        /// Check Ship Has Error or is null set return true
        /// </summary>
        public bool IsOrderValid
        {
            get
            {
                if (SaleOrderDetailCollection == null || (SaleOrderDetailCollection != null && !SaleOrderDetailCollection.Any()))
                    return false;
                return (SaleOrderDetailCollection != null && !SaleOrderDetailCollection.Any(x => x.IsError));
            }

        }
        #endregion

        #region IsView
        private bool _isView;
        /// <summary>
        /// Gets or sets the IsView.
        /// </summary>
        public bool IsView
        {
            get { return _isView; }
            set
            {
                if (_isView != value)
                {
                    _isView = value;
                    OnPropertyChanged(() => IsView);
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
            if (SaleOrderDetailCollection == null)
                return false;

            return IsOrderValid && SaleOrderDetailCollection.Any(x => x.DueQty > 0) && SaleOrderDetailCollection.Any(x => x.QtyOfPick > 0);
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            try
            {
                if (!IsView)
                {
                    if (IsEdit)
                    {
                        foreach (var item in SaleOrderShipModel.SaleOrderShipDetailCollection.ToList())
                            SaleOrderShipModel.SaleOrderShipDetailCollection.Remove(item);
                    }
                    else
                    {
                        SaleOrderShipModel = new base_SaleOrderShipModel();
                        SaleOrderShipModel.IsShipped = false;
                        SaleOrderShipModel.BoxNo = 1;
                        SaleOrderShipModel.ShipDate = DateTime.Today;
                        SaleOrderShipModel.Resource = Guid.NewGuid();
                        SaleOrderShipModel.SaleOrderShipDetailCollection = new CollectionBase<base_SaleOrderShipDetailModel>();
                    }

                    if (SaleOrderDetailCollection.Any(x => x.QtyOfPick > 0))
                    {
                        //Pack item with quantity >0 & is not a Product Group
                        foreach (base_SaleOrderDetailModel saleOrderDetailModel in SaleOrderDetailCollection.Where(x => !x.ProductModel.ItemTypeId.Equals((short)ItemTypes.Group) && x.QtyOfPick > 0))
                        {
                            saleOrderDetailModel.PickQty += saleOrderDetailModel.QtyOfPick;
                            base_SaleOrderShipDetailModel saleOrderShipDetailModel = new base_SaleOrderShipDetailModel();
                            saleOrderShipDetailModel.Resource = Guid.NewGuid();
                            saleOrderShipDetailModel.SaleOrderShipResource = SaleOrderShipModel.Resource.ToString();
                            saleOrderShipDetailModel.SaleOrderDetailResource = saleOrderDetailModel.Resource.ToString();
                            saleOrderShipDetailModel.ProductResource = saleOrderDetailModel.ProductResource;
                            saleOrderShipDetailModel.ItemCode = saleOrderDetailModel.ItemCode;
                            saleOrderShipDetailModel.ItemName = saleOrderDetailModel.ItemName;
                            saleOrderShipDetailModel.ItemAtribute = saleOrderDetailModel.ItemAtribute;
                            saleOrderShipDetailModel.ItemSize = saleOrderDetailModel.ItemSize;
                            saleOrderShipDetailModel.PackedQty = saleOrderDetailModel.QtyOfPick;
                            SaleOrderShipModel.SaleOrderShipDetailCollection.Add(saleOrderShipDetailModel);
                        }
                        SaleOrderShipModel.Remark = SaleOrderShipModel.SaleOrderShipDetailCollection.Count().ToString();
                    }
                    SaleOrderDetailList = SaleOrderDetailCollection;
                }

                FindOwnerWindow(_ownerViewModel).DialogResult = true;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, Language.GetMsg("ErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
          
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
            if (SaleOrderDetailCollection != null)
            {
                IEditableCollectionView editableView = CollectionViewSource.GetDefaultView(SaleOrderDetailCollection) as IEditableCollectionView;
                editableView.CommitEdit();
            }
            if (!IsEdit)
                SaleOrderShipModel = null;
            
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            // Route the commands
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
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


        public IEnumerable<base_SaleOrderDetailModel> Clone(IEnumerable<base_SaleOrderDetailModel> list)
        {
            foreach (var item in list)
            {
                yield return (base_SaleOrderDetailModel)Clone(item as object);
            }
        }

        public IEnumerable<base_SaleOrderDetailModel> Convert(IEnumerable<base_SaleOrderDetailModel> list)
        {
            foreach (var item in list)
            {
                object a = new object();
                yield return (base_SaleOrderDetailModel)ConvertObject(item as object, a);
            }
        }
        #endregion

        #region Public Methods
      
        #endregion
       
    }
}