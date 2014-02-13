﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;

[assembly: EdmSchemaAttribute()]
#region EDM Relationship Metadata

[assembly: EdmRelationshipAttribute("POSLicenseModel", "FK_CustomerDetail_CustomerId", "Customer", System.Data.Metadata.Edm.RelationshipMultiplicity.One, typeof(KeyGenerator.Database.Customer), "CustomerDetail", System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(KeyGenerator.Database.CustomerDetail))]

#endregion

namespace KeyGenerator.Database
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class POSLicenseEntities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new POSLicenseEntities object using the connection string found in the 'POSLicenseEntities' section of the application configuration file.
        /// </summary>
        public POSLicenseEntities() : base("name=POSLicenseEntities", "POSLicenseEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new POSLicenseEntities object.
        /// </summary>
        public POSLicenseEntities(string connectionString) : base(connectionString, "POSLicenseEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new POSLicenseEntities object.
        /// </summary>
        public POSLicenseEntities(EntityConnection connection) : base(connection, "POSLicenseEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();
    
        #endregion
    
        #region ObjectSet Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<Customer> Customer
        {
            get
            {
                if ((_Customer == null))
                {
                    _Customer = base.CreateObjectSet<Customer>("Customer");
                }
                return _Customer;
            }
        }
        private ObjectSet<Customer> _Customer;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<CustomerDetail> CustomerDetail
        {
            get
            {
                if ((_CustomerDetail == null))
                {
                    _CustomerDetail = base.CreateObjectSet<CustomerDetail>("CustomerDetail");
                }
                return _CustomerDetail;
            }
        }
        private ObjectSet<CustomerDetail> _CustomerDetail;

        #endregion
        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Customer EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToCustomer(Customer customer)
        {
            base.AddObject("Customer", customer);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the CustomerDetail EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToCustomerDetail(CustomerDetail customerDetail)
        {
            base.AddObject("CustomerDetail", customerDetail);
        }

        #endregion
    }
    

    #endregion
    
    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="POSLicenseModel", Name="Customer")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class Customer : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new Customer object.
        /// </summary>
        /// <param name="id">Initial value of the Id property.</param>
        public static Customer CreateCustomer(global::System.Int64 id)
        {
            Customer customer = new Customer();
            customer.Id = id;
            return customer;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int64 Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (_Id != value)
                {
                    OnIdChanging(value);
                    ReportPropertyChanging("Id");
                    _Id = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("Id");
                    OnIdChanged();
                }
            }
        }
        private global::System.Int64 _Id;
        partial void OnIdChanging(global::System.Int64 value);
        partial void OnIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Company
        {
            get
            {
                return _Company;
            }
            set
            {
                OnCompanyChanging(value);
                ReportPropertyChanging("Company");
                _Company = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Company");
                OnCompanyChanged();
            }
        }
        private global::System.String _Company;
        partial void OnCompanyChanging(global::System.String value);
        partial void OnCompanyChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Phone
        {
            get
            {
                return _Phone;
            }
            set
            {
                OnPhoneChanging(value);
                ReportPropertyChanging("Phone");
                _Phone = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Phone");
                OnPhoneChanged();
            }
        }
        private global::System.String _Phone;
        partial void OnPhoneChanging(global::System.String value);
        partial void OnPhoneChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Fax
        {
            get
            {
                return _Fax;
            }
            set
            {
                OnFaxChanging(value);
                ReportPropertyChanging("Fax");
                _Fax = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Fax");
                OnFaxChanged();
            }
        }
        private global::System.String _Fax;
        partial void OnFaxChanging(global::System.String value);
        partial void OnFaxChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String CellPhone
        {
            get
            {
                return _CellPhone;
            }
            set
            {
                OnCellPhoneChanging(value);
                ReportPropertyChanging("CellPhone");
                _CellPhone = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("CellPhone");
                OnCellPhoneChanged();
            }
        }
        private global::System.String _CellPhone;
        partial void OnCellPhoneChanging(global::System.String value);
        partial void OnCellPhoneChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Address
        {
            get
            {
                return _Address;
            }
            set
            {
                OnAddressChanging(value);
                ReportPropertyChanging("Address");
                _Address = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Address");
                OnAddressChanged();
            }
        }
        private global::System.String _Address;
        partial void OnAddressChanging(global::System.String value);
        partial void OnAddressChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Email
        {
            get
            {
                return _Email;
            }
            set
            {
                OnEmailChanging(value);
                ReportPropertyChanging("Email");
                _Email = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Email");
                OnEmailChanged();
            }
        }
        private global::System.String _Email;
        partial void OnEmailChanging(global::System.String value);
        partial void OnEmailChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String ContactName
        {
            get
            {
                return _ContactName;
            }
            set
            {
                OnContactNameChanging(value);
                ReportPropertyChanging("ContactName");
                _ContactName = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("ContactName");
                OnContactNameChanged();
            }
        }
        private global::System.String _ContactName;
        partial void OnContactNameChanging(global::System.String value);
        partial void OnContactNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String ContactPhone
        {
            get
            {
                return _ContactPhone;
            }
            set
            {
                OnContactPhoneChanging(value);
                ReportPropertyChanging("ContactPhone");
                _ContactPhone = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("ContactPhone");
                OnContactPhoneChanged();
            }
        }
        private global::System.String _ContactPhone;
        partial void OnContactPhoneChanging(global::System.String value);
        partial void OnContactPhoneChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String ContactEmail
        {
            get
            {
                return _ContactEmail;
            }
            set
            {
                OnContactEmailChanging(value);
                ReportPropertyChanging("ContactEmail");
                _ContactEmail = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("ContactEmail");
                OnContactEmailChanged();
            }
        }
        private global::System.String _ContactEmail;
        partial void OnContactEmailChanging(global::System.String value);
        partial void OnContactEmailChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int16> TotalStore
        {
            get
            {
                return _TotalStore;
            }
            set
            {
                OnTotalStoreChanging(value);
                ReportPropertyChanging("TotalStore");
                _TotalStore = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("TotalStore");
                OnTotalStoreChanged();
            }
        }
        private Nullable<global::System.Int16> _TotalStore;
        partial void OnTotalStoreChanging(Nullable<global::System.Int16> value);
        partial void OnTotalStoreChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String UserCreated
        {
            get
            {
                return _UserCreated;
            }
            set
            {
                OnUserCreatedChanging(value);
                ReportPropertyChanging("UserCreated");
                _UserCreated = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("UserCreated");
                OnUserCreatedChanged();
            }
        }
        private global::System.String _UserCreated;
        partial void OnUserCreatedChanging(global::System.String value);
        partial void OnUserCreatedChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String UserUpdated
        {
            get
            {
                return _UserUpdated;
            }
            set
            {
                OnUserUpdatedChanging(value);
                ReportPropertyChanging("UserUpdated");
                _UserUpdated = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("UserUpdated");
                OnUserUpdatedChanged();
            }
        }
        private global::System.String _UserUpdated;
        partial void OnUserUpdatedChanging(global::System.String value);
        partial void OnUserUpdatedChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.DateTime> DateCreated
        {
            get
            {
                return _DateCreated;
            }
            set
            {
                OnDateCreatedChanging(value);
                ReportPropertyChanging("DateCreated");
                _DateCreated = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("DateCreated");
                OnDateCreatedChanged();
            }
        }
        private Nullable<global::System.DateTime> _DateCreated;
        partial void OnDateCreatedChanging(Nullable<global::System.DateTime> value);
        partial void OnDateCreatedChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.DateTime> DateUpdated
        {
            get
            {
                return _DateUpdated;
            }
            set
            {
                OnDateUpdatedChanging(value);
                ReportPropertyChanging("DateUpdated");
                _DateUpdated = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("DateUpdated");
                OnDateUpdatedChanged();
            }
        }
        private Nullable<global::System.DateTime> _DateUpdated;
        partial void OnDateUpdatedChanging(Nullable<global::System.DateTime> value);
        partial void OnDateUpdatedChanged();

        #endregion
    
        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute("POSLicenseModel", "FK_CustomerDetail_CustomerId", "CustomerDetail")]
        public EntityCollection<CustomerDetail> CustomerDetail
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<CustomerDetail>("POSLicenseModel.FK_CustomerDetail_CustomerId", "CustomerDetail");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<CustomerDetail>("POSLicenseModel.FK_CustomerDetail_CustomerId", "CustomerDetail", value);
                }
            }
        }

        #endregion
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="POSLicenseModel", Name="CustomerDetail")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class CustomerDetail : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new CustomerDetail object.
        /// </summary>
        /// <param name="id">Initial value of the Id property.</param>
        public static CustomerDetail CreateCustomerDetail(global::System.Int64 id)
        {
            CustomerDetail customerDetail = new CustomerDetail();
            customerDetail.Id = id;
            return customerDetail;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int64 Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (_Id != value)
                {
                    OnIdChanging(value);
                    ReportPropertyChanging("Id");
                    _Id = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("Id");
                    OnIdChanged();
                }
            }
        }
        private global::System.Int64 _Id;
        partial void OnIdChanging(global::System.Int64 value);
        partial void OnIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String ApplicationId
        {
            get
            {
                return _ApplicationId;
            }
            set
            {
                OnApplicationIdChanging(value);
                ReportPropertyChanging("ApplicationId");
                _ApplicationId = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("ApplicationId");
                OnApplicationIdChanged();
            }
        }
        private global::System.String _ApplicationId;
        partial void OnApplicationIdChanging(global::System.String value);
        partial void OnApplicationIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> StoreCode
        {
            get
            {
                return _StoreCode;
            }
            set
            {
                OnStoreCodeChanging(value);
                ReportPropertyChanging("StoreCode");
                _StoreCode = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("StoreCode");
                OnStoreCodeChanged();
            }
        }
        private Nullable<global::System.Int32> _StoreCode;
        partial void OnStoreCodeChanging(Nullable<global::System.Int32> value);
        partial void OnStoreCodeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String LicenceCode
        {
            get
            {
                return _LicenceCode;
            }
            set
            {
                OnLicenceCodeChanging(value);
                ReportPropertyChanging("LicenceCode");
                _LicenceCode = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("LicenceCode");
                OnLicenceCodeChanged();
            }
        }
        private global::System.String _LicenceCode;
        partial void OnLicenceCodeChanging(global::System.String value);
        partial void OnLicenceCodeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.DateTime> GenDate
        {
            get
            {
                return _GenDate;
            }
            set
            {
                OnGenDateChanging(value);
                ReportPropertyChanging("GenDate");
                _GenDate = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("GenDate");
                OnGenDateChanged();
            }
        }
        private Nullable<global::System.DateTime> _GenDate;
        partial void OnGenDateChanging(Nullable<global::System.DateTime> value);
        partial void OnGenDateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> Period
        {
            get
            {
                return _Period;
            }
            set
            {
                OnPeriodChanging(value);
                ReportPropertyChanging("Period");
                _Period = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Period");
                OnPeriodChanged();
            }
        }
        private Nullable<global::System.Int32> _Period;
        partial void OnPeriodChanging(Nullable<global::System.Int32> value);
        partial void OnPeriodChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Boolean> IsLived
        {
            get
            {
                return _IsLived;
            }
            set
            {
                OnIsLivedChanging(value);
                ReportPropertyChanging("IsLived");
                _IsLived = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("IsLived");
                OnIsLivedChanged();
            }
        }
        private Nullable<global::System.Boolean> _IsLived;
        partial void OnIsLivedChanging(Nullable<global::System.Boolean> value);
        partial void OnIsLivedChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String POSId
        {
            get
            {
                return _POSId;
            }
            set
            {
                OnPOSIdChanging(value);
                ReportPropertyChanging("POSId");
                _POSId = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("POSId");
                OnPOSIdChanged();
            }
        }
        private global::System.String _POSId;
        partial void OnPOSIdChanging(global::System.String value);
        partial void OnPOSIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String RequestBy
        {
            get
            {
                return _RequestBy;
            }
            set
            {
                OnRequestByChanging(value);
                ReportPropertyChanging("RequestBy");
                _RequestBy = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("RequestBy");
                OnRequestByChanged();
            }
        }
        private global::System.String _RequestBy;
        partial void OnRequestByChanging(global::System.String value);
        partial void OnRequestByChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.DateTime> DateCreated
        {
            get
            {
                return _DateCreated;
            }
            set
            {
                OnDateCreatedChanging(value);
                ReportPropertyChanging("DateCreated");
                _DateCreated = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("DateCreated");
                OnDateCreatedChanged();
            }
        }
        private Nullable<global::System.DateTime> _DateCreated;
        partial void OnDateCreatedChanging(Nullable<global::System.DateTime> value);
        partial void OnDateCreatedChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> ExpireDate
        {
            get
            {
                return _ExpireDate;
            }
            set
            {
                OnExpireDateChanging(value);
                ReportPropertyChanging("ExpireDate");
                _ExpireDate = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("ExpireDate");
                OnExpireDateChanged();
            }
        }
        private Nullable<global::System.Int32> _ExpireDate;
        partial void OnExpireDateChanging(Nullable<global::System.Int32> value);
        partial void OnExpireDateChanged();

        #endregion
    
        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute("POSLicenseModel", "FK_CustomerDetail_CustomerId", "Customer")]
        public Customer Customer
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<Customer>("POSLicenseModel.FK_CustomerDetail_CustomerId", "Customer").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<Customer>("POSLicenseModel.FK_CustomerDetail_CustomerId", "Customer").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<Customer> CustomerReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<Customer>("POSLicenseModel.FK_CustomerDetail_CustomerId", "Customer");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<Customer>("POSLicenseModel.FK_CustomerDetail_CustomerId", "Customer", value);
                }
            }
        }

        #endregion
    }

    #endregion
    
}