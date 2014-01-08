using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupProductManualAdvanceSearchViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the AdvanceSearchPredicate
        /// </summary>
        public Expression<Func<base_Product, bool>> AdvanceSearchPredicate { get; private set; }

        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty { get; set; }

        private string _productName;
        /// <summary>
        /// Gets or sets the ProductName.
        /// </summary>
        public string ProductName
        {
            get { return _productName; }
            set
            {
                if (_productName != value)
                {
                    this.IsDirty = true;
                    _productName = value;
                    OnPropertyChanged(() => ProductName);
                }
            }
        }

        private string _vendor;
        /// <summary>
        /// Gets or sets the Vendor.
        /// </summary>
        public string Vendor
        {
            get { return _vendor; }
            set
            {
                if (_vendor != value)
                {
                    this.IsDirty = true;
                    _vendor = value;
                    OnPropertyChanged(() => Vendor);
                }
            }
        }

        /// <summary>
        /// Gets or sets the VendorList.
        /// </summary>
        public List<ComboItem> VendorList { get; set; }

        private string _barcode;
        /// <summary>
        /// Gets or sets the Barcode.
        /// </summary>
        public string Barcode
        {
            get { return _barcode; }
            set
            {
                if (_barcode != value)
                {
                    this.IsDirty = true;
                    _barcode = value;
                    OnPropertyChanged(() => Barcode);
                }
            }
        }

        private string _alu;
        /// <summary>
        /// Gets or sets the ALU.
        /// </summary>
        public string ALU
        {
            get { return _alu; }
            set
            {
                if (_alu != value)
                {
                    this.IsDirty = true;
                    _alu = value;
                    OnPropertyChanged(() => ALU);
                }
            }
        }

        private string _attribute;
        /// <summary>
        /// Gets or sets the Attribute.
        /// </summary>
        public string Attribute
        {
            get { return _attribute; }
            set
            {
                if (_attribute != value)
                {
                    this.IsDirty = true;
                    _attribute = value;
                    OnPropertyChanged(() => Attribute);
                }
            }
        }

        private string _size;
        /// <summary>
        /// Gets or sets the Size.
        /// </summary>
        public string Size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    this.IsDirty = true;
                    _size = value;
                    OnPropertyChanged(() => Size);
                }
            }
        }

        /// <summary>
        /// Gets or sets the CategoryList.
        /// </summary>
        public List<ComboItem> CategoryList { get; set; }

        private int _categoryID;
        /// <summary>
        /// Gets or sets the CategoryID.
        /// </summary>
        public int CategoryID
        {
            get { return _categoryID; }
            set
            {
                if (_categoryID != value)
                {
                    this.IsDirty = true;
                    _categoryID = value;
                    OnPropertyChanged(() => CategoryID);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SaleTaxLocationList
        /// </summary>
        public List<string> SaleTaxLocationList { get; set; }

        private string _taxCode;
        /// <summary>
        /// Gets or sets the TaxCode.
        /// </summary>
        public string TaxCode
        {
            get { return _taxCode; }
            set
            {
                if (_taxCode != value)
                {
                    this.IsDirty = true;
                    _taxCode = value;
                    OnPropertyChanged(() => TaxCode);
                }
            }
        }

        private string _description;
        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    this.IsDirty = true;
                    _description = value;
                    OnPropertyChanged(() => Description);
                }
            }
        }

        /// <summary>
        /// Gets or sets the UOMList.
        /// </summary>
        public List<ComboItem> UOMList { get; set; }

        private int _uomID;
        /// <summary>
        /// Gets or sets the UOMID.
        /// </summary>
        public int UOMID
        {
            get { return _uomID; }
            set
            {
                if (_uomID != value)
                {
                    this.IsDirty = true;
                    _uomID = value;
                    OnPropertyChanged(() => UOMID);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupProductManualAdvanceSearchViewModel()
        {
            LoadStaticData();
            InitialCommand();
        }

        #endregion

        #region Command Methods

        #region OkCommand

        /// <summary>
        /// Gets the OkCommand command.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Method to check whether the OkCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            return IsDirty;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            // Create advance search predicate
            AdvanceSearchPredicate = CreateAdvanceSearchPredicate();

            Window window = FindOwnerWindow(this);
            window.DialogResult = true;
        }

        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the CancelCommand command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CancelCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CancelCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Load static data
        /// </summary>
        private void LoadStaticData()
        {
            base_GuestRepository guestRepository = new base_GuestRepository();
            base_DepartmentRepository departmentRepository = new base_DepartmentRepository();
            base_SaleTaxLocationRepository saleTaxLocationRepository = new base_SaleTaxLocationRepository();
            base_UOMRepository uomRepository = new base_UOMRepository();

            string vendorMark = MarkType.Vendor.ToDescription();

            // Load vendor list
            VendorList = new List<ComboItem>(guestRepository.GetAll(x => !x.IsPurged && x.Mark.Equals(vendorMark)).
                OrderBy(x => x.Company).
                Select(x => new ComboItem
                {
                    LongValue = x.Id,
                    Text = x.Company
                }));

            // Load category list
            CategoryList = new List<ComboItem>(departmentRepository.GetAll(x => x.IsActived == true && x.LevelId == 1).
                OrderBy(x => x.Name).
                Select(x => new ComboItem
                {
                    IntValue = x.Id,
                    Text = x.Name
                }));
            CategoryList.Insert(0, new ComboItem());

            // Load sale tax location list
            base_SaleTaxLocation taxLocationPrimary = saleTaxLocationRepository.Get(x => x.ParentId == 0 && x.IsPrimary);
            if (taxLocationPrimary != null)
            {
                SaleTaxLocationList = new List<string>(saleTaxLocationRepository.
                    GetIQueryable(x => x.ParentId > 0 && x.ParentId.Equals(taxLocationPrimary.Id)).
                    OrderBy(x => x.TaxCode).
                    Select(x => x.TaxCode));
            }
            SaleTaxLocationList.Insert(0, string.Empty);

            // Load UOM list
            UOMList = new List<ComboItem>(uomRepository.GetAll(x => x.IsActived).
                OrderBy(x => x.Name).
                Select(x => new ComboItem
                {
                    IntValue = x.Id,
                    Text = x.Name
                }));
            UOMList.Insert(0, new ComboItem());
        }

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Product, bool>> CreateAdvanceSearchPredicate()
        {
            // Create predicate
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

            if (!string.IsNullOrWhiteSpace(ProductName))
            {
                // Get all products that ProductName contain keyword
                predicate = predicate.And(x => x.ProductName.ToLower().Contains(ProductName.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Vendor))
            {
                // Get all vendors contain keyword
                IEnumerable<ComboItem> vendors = VendorList.Where(x => x.Text.ToLower().Contains(Vendor.ToLower()));
                IEnumerable<long> vendorIDList = vendors.Select(x => x.LongValue);

                // Get all products that Vendor contain keyword
                predicate = predicate.And(x => vendorIDList.Contains(x.VendorId));
            }
            if (!string.IsNullOrWhiteSpace(Barcode))
            {
                // Get all products that Barcode contain keyword
                predicate = predicate.And(x => x.Barcode.ToLower().Contains(Barcode.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(ALU))
            {
                // Get all products that ALU contain keyword
                predicate = predicate.And(x => x.ALU.ToLower().Contains(ALU.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Attribute))
            {
                // Get all products that Attribute contain keyword
                predicate = predicate.And(x => x.Attribute.ToLower().Contains(Attribute.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Size))
            {
                // Get all products that Size contain keyword
                predicate = predicate.And(x => x.Size.ToLower().Contains(Size.ToLower()));
            }
            if (CategoryID > 0)
            {
                // Get all products that Category equal keyword
                predicate = predicate.And(x => x.ProductCategoryId.Equals(CategoryID));
            }
            if (!string.IsNullOrWhiteSpace(TaxCode))
            {
                // Get all products that TaxCode equal keyword
                predicate = predicate.And(x => x.TaxCode.Equals(TaxCode));
            }
            if (!string.IsNullOrWhiteSpace(Description))
            {
                // Get all products that Description contain keyword
                predicate = predicate.And(x => x.Description.ToLower().Contains(Description.ToLower()));
            }
            if (UOMID > 0)
            {
                // Get all products that UOM equal keyword
                predicate = predicate.And(x => x.BaseUOMId.Equals(UOMID));
            }

            short itemTypeID = (short)ItemTypes.Group;

            // Default condition
            predicate = predicate.And(x => x.IsPurge == false && x.ItemTypeId == itemTypeID);

            if (!IsMainStore)
            {
                // Get all products by store code
                predicate = predicate.And(x => x.base_ProductStore.Any(y => y.StoreCode.Equals(Define.StoreCode)));
            }

            return predicate;
        }

        #endregion
    }
}