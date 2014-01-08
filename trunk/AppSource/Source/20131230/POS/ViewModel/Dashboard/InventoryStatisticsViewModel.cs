using System;
using System.Linq;
using CPC.Toolkit.Base;
using CPC.POS.Interfaces;
using System.Xml.Linq;
using System.Windows;
using CPC.Helper;
using CPC.POS.Repository;

namespace CPC.POS.ViewModel
{
    public class InventoryStatisticsViewModel : ViewModelBase, IDashboardItemFunction
    {
        #region Fields

        private XElement _configuration;

        #endregion

        #region Contructors

        public InventoryStatisticsViewModel(XElement configuration)
        {
            _configuration = configuration;

            CountProduct();
            CountCustomer();
            CalculateCost();
            CalculatePrice();
        }

        #endregion

        #region Properties

        #region TotalProduct

        private long _totalProduct;
        public long TotalProduct
        {
            get
            {
                return _totalProduct;
            }
            set
            {
                if (_totalProduct != value)
                {
                    _totalProduct = value;
                    OnPropertyChanged(() => TotalProduct);
                }
            }
        }

        #endregion

        #region TotalCustomer

        private long _totalCustomer;
        public long TotalCustomer
        {
            get
            {
                return _totalCustomer;
            }
            set
            {
                if (_totalCustomer != value)
                {
                    _totalCustomer = value;
                    OnPropertyChanged(() => TotalCustomer);
                }
            }
        }

        #endregion

        #region Cost

        private decimal _cost;
        public decimal Cost
        {
            get
            {
                return _cost;
            }
            set
            {
                if (_cost != value)
                {
                    _cost = value;
                    OnPropertyChanged(() => Cost);
                }
            }
        }

        #endregion

        #region Price

        private decimal _price;
        public decimal Price
        {
            get
            {
                return _price;
            }
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged(() => Price);
                }
            }
        }

        #endregion

        #region Width

        private double _width;
        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged(() => Width);
                }
            }
        }

        #endregion

        #region Height

        private double _height;
        public double Height
        {
            get
            {
                return _height;
            }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged(() => Height);
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region CountGuest

        /// <summary>
        /// Count guest.
        /// </summary>
        private long CountGuest(MarkType guestType)
        {
            long total = 0;

            try
            {
                string type = guestType.ToDescription();
                base_GuestRepository guestRepository = new base_GuestRepository();
                total = guestRepository.GetIQueryable(x => x.Mark == type).LongCount();
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }

            return total;
        }

        /// <summary>
        /// Count customers.
        /// </summary>
        private void CountCustomer()
        {
            try
            {
                string type = MarkType.Customer.ToDescription();
                base_GuestRepository guestRepository = new base_GuestRepository();
                TotalCustomer = guestRepository.GetIQueryable(x => x.Mark == type).LongCount();
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region CountProduct

        /// <summary>
        /// Count product.
        /// </summary>
        private void CountProduct()
        {
            try
            {
                base_ProductStoreRepository productStoreRepository = new base_ProductStoreRepository();
                short serviceProduct = (short)ItemTypes.Services;
                TotalProduct = productStoreRepository.GetIQueryable(x => x.StoreCode == Define.StoreCode && x.base_Product.ItemTypeId != serviceProduct).GroupBy(x => x.ProductId).LongCount();
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region CalculateCost

        /// <summary>
        /// Calculate cost.
        /// </summary>
        private void CalculateCost()
        {
            try
            {
                base_ProductStoreRepository productStoreRepository = new base_ProductStoreRepository();
                short serviceProduct = (short)ItemTypes.Services;
                if (Define.StoreCode == 0)
                {
                    var query = productStoreRepository.GetIQueryable(x => x.base_Product.ItemTypeId != serviceProduct).GroupBy(x => new
                    {
                        Id = x.ProductId,
                        AverageUnitCost = x.base_Product.AverageUnitCost
                    });
                    if (query.Any())
                    {
                        Cost = query.Sum(x => x.Sum(y => y.QuantityOnHand) * x.Key.AverageUnitCost);
                    }
                }
                else
                {
                    var query = productStoreRepository.GetIQueryable(x => x.StoreCode == Define.StoreCode && x.base_Product.ItemTypeId != serviceProduct).GroupBy(x => new
                    {
                        Id = x.ProductId,
                        AverageUnitCost = x.base_Product.AverageUnitCost
                    });
                    if (query.Any())
                    {
                        Cost = query.Sum(x => x.Sum(y => y.QuantityOnHand) * x.Key.AverageUnitCost);
                    }
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region CalculatePrice

        /// <summary>
        /// Calculate price.
        /// </summary>
        private void CalculatePrice()
        {
            try
            {
                base_ProductStoreRepository productStoreRepository = new base_ProductStoreRepository();
                short serviceProduct = (short)ItemTypes.Services;
                if (Define.StoreCode == 0)
                {
                    var query = productStoreRepository.GetIQueryable(x => x.base_Product.ItemTypeId != serviceProduct).GroupBy(x => new
                    {
                        Id = x.ProductId,
                        RegularPrice = x.base_Product.RegularPrice
                    });
                    if (query.Any())
                    {
                        Price = query.Sum(x => x.Sum(y => y.QuantityOnHand) * x.Key.RegularPrice);
                    }
                }
                else
                {
                    var query = productStoreRepository.GetIQueryable(x => x.StoreCode == Define.StoreCode && x.base_Product.ItemTypeId != serviceProduct).GroupBy(x => new
                    {
                        Id = x.ProductId,
                        RegularPrice = x.base_Product.RegularPrice
                    });
                    if (query.Any())
                    {
                        Price = query.Sum(x => x.Sum(y => y.QuantityOnHand) * x.Key.RegularPrice);
                    }
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #endregion

        #region WriteLog

        private void WriteLog(Exception exception)
        {
            _log4net.Error(string.Format("Message: {0}. Source: {1}.", exception.Message, exception.Source));
            if (exception.InnerException != null)
            {
                _log4net.Error(exception.InnerException.ToString());
            }
        }

        #endregion

        #region IDashboardItemFunction Members

        public bool CanEdit
        {
            get
            {
                return false;
            }
        }

        public void Lock()
        {

        }

        public void Unlock()
        {

        }

        public XElement GetConfiguration()
        {
            return _configuration;
        }

        public void UpdateSize(Size newSize)
        {
            double width = newSize.Width / 3;
            double height = newSize.Height / 2;

            if (_width < width)
            {
                Width = width;
            }
            if (_height < height)
            {
                Height = height;
            }
        }

        #endregion
    }
}
