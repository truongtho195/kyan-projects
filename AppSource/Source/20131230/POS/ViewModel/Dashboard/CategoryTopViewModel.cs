using System;
using System.Collections.Generic;
using System.Linq;
using CPC.Toolkit.Base;
using CPC.POS.Interfaces;
using System.Windows;
using De.TorstenMandelkow.MetroChart;
using System.Xml.Linq;
using System.Windows.Input;
using CPC.Toolkit.Command;
using CPC.Helper;
using CPC.POS.Repository;
using CPC.POS.Database;
using CPC.POS.Model;
using System.Windows.Controls;
using System.Windows.Data;

namespace CPC.POS.ViewModel
{
    public class CategoryTopViewModel : ViewModelBase, IDashboardItemFunction
    {
        #region Fields

        private Grid _gridView;

        private ChartBase _chartBase;

        private ChartSeries _chartSeries;

        private XElement _configuration;

        #endregion

        #region Constructors

        public CategoryTopViewModel(Grid grid, XElement configuration)
        {
            _gridView = grid;
            _configuration = configuration;
            _topList = new short[] { 5, 10, 15, 20, 25, 30 };

            if (_configuration != null)
            {
                if (configuration.Attribute("Total") != null)
                {
                    short.TryParse(configuration.Attribute("Total").Value, out _total);
                }
                if (configuration.Attribute("CategoryOrderByID") != null)
                {
                    short.TryParse(configuration.Attribute("CategoryOrderByID").Value, out _categoryOrderByID);
                }
                if (configuration.Attribute("OrderDirectionID") != null)
                {
                    short.TryParse(configuration.Attribute("OrderDirectionID").Value, out _orderDirectionID);
                }
                if (configuration.Attribute("ChartTypeID") != null)
                {
                    short.TryParse(configuration.Attribute("ChartTypeID").Value, out _chartTypeID);
                }
            }

            // Create ChartSeries Default used for all charts.
            _chartSeries = new ChartSeries();
            _chartSeries.DisplayMember = "Name";
            BindingOperations.SetBinding(_chartSeries, ChartSeries.ItemsSourceProperty, new Binding
            {
                Path = new PropertyPath("CategoryCollection"),
                Mode = BindingMode.OneWay
            });

            if (_total == 0)
            {
                _total = _topList.Min();
            }
            if (_categoryOrderByID == 0)
            {
                _categoryOrderByID = Common.CategoryOrderBy.First().Value;
            }
            if (_orderDirectionID == 0)
            {
                _orderDirectionID = Common.OrderDirection.First().Value;
            }
            if (_chartTypeID == 0)
            {
                _chartTypeID = Common.ChartType.First().Value;
            }

            SelectChart();

            GetCategories();
        }

        #endregion

        #region Properties

        #region GridViewVisibility

        private Visibility _gridViewVisibility = Visibility.Visible;
        public Visibility GridViewVisibility
        {
            get
            {
                return _gridViewVisibility;
            }
            set
            {
                if (_gridViewVisibility != value)
                {
                    _gridViewVisibility = value;
                    OnPropertyChanged(() => GridViewVisibility);
                }
            }
        }

        #endregion

        #region GridEditVisibility

        private Visibility _gridEditVisibility = Visibility.Collapsed;
        public Visibility GridEditVisibility
        {
            get
            {
                return _gridEditVisibility;
            }
            set
            {
                if (_gridEditVisibility != value)
                {
                    _gridEditVisibility = value;
                    OnPropertyChanged(() => GridEditVisibility);
                }
            }
        }

        #endregion

        #region ToolTipFormat

        public string ToolTipFormat
        {
            get
            {
                return "{0} has value '{1}'";
            }
        }

        public string ToolTipFormatWithPercent
        {
            get
            {
                return "{0} has value '{1}' ({3:P2})";
            }
        }

        #endregion

        #region TopList

        private short[] _topList;
        public short[] TopList
        {
            get
            {
                return _topList;
            }
            set
            {
                if (_topList != value)
                {
                    _topList = value;
                    OnPropertyChanged(() => TopList);
                }
            }
        }

        #endregion

        #region Total

        private short _total;
        public short Total
        {
            get
            {
                return _total;
            }
            set
            {
                if (_total != value)
                {
                    _total = value;
                    OnPropertyChanged(() => Total);
                }
            }
        }

        #endregion

        #region CategoryOrderByID

        private short _categoryOrderByID;
        public short CategoryOrderByID
        {
            get
            {
                return _categoryOrderByID;
            }
            set
            {
                if (_categoryOrderByID != value)
                {
                    _categoryOrderByID = value;
                    OnPropertyChanged(() => CategoryOrderByID);
                }
            }
        }

        #endregion

        #region OrderDirectionID

        private short _orderDirectionID;
        public short OrderDirectionID
        {
            get
            {
                return _orderDirectionID;
            }
            set
            {
                if (_orderDirectionID != value)
                {
                    _orderDirectionID = value;
                    OnPropertyChanged(() => OrderDirectionID);
                }
            }
        }

        #endregion

        #region ChartTypeID

        private short _chartTypeID;
        public short ChartTypeID
        {
            get
            {
                return _chartTypeID;
            }
            set
            {
                if (_chartTypeID != value)
                {
                    _chartTypeID = value;
                    OnPropertyChanged(() => ChartTypeID);
                }
            }
        }

        #endregion

        #region CategoryCollection

        private CollectionBase<base_DepartmentModel> _categoryCollection;
        public CollectionBase<base_DepartmentModel> CategoryCollection
        {
            get
            {
                return _categoryCollection;
            }
            set
            {
                if (_categoryCollection != value)
                {
                    _categoryCollection = value;
                    OnPropertyChanged(() => CategoryCollection);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region OKCommand

        private ICommand _OKCommand;
        /// <summary>
        /// When 'OK' button clicked, command will execute.
        /// </summary>
        public ICommand OKCommand
        {
            get
            {
                if (_OKCommand == null)
                {
                    _OKCommand = new RelayCommand(OKExecute);
                }
                return _OKCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region OKExecute

        private void OKExecute()
        {
            SelectChart();
            GetCategories();
            SaveConfiguration();
            Lock();
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #endregion

        #region Private Methods

        #region SelectChart

        /// <summary>
        /// Select Chart.
        /// </summary>
        private void SelectChart()
        {
            // Not change chart.
            if (_chartBase != null && ((short)_chartBase.Tag == _chartTypeID))
            {
                return;
            }

            double width = 0;
            double height = 0;
            if (_chartBase != null)
            {
                width = _chartBase.Width;
                height = _chartBase.Height;
            }

            // Clear old chart.
            if (_gridView.Children != null)
            {
                _gridView.Children.Clear();
            }

            // Create new chart.
            switch ((ChartType)_chartTypeID)
            {
                case ChartType.ColumnChart:

                    _chartBase = new ClusteredColumnChart();
                    _chartBase.Style = App.Current.FindResource("ColumnChartStyleVariableSize") as Style;

                    break;

                case ChartType.BarChart:

                    _chartBase = new ClusteredBarChart();
                    _chartBase.Style = App.Current.FindResource("BarChartStyleVariableSize") as Style;
                    break;

                case ChartType.StackedColumnChart:

                    _chartBase = new StackedColumnChart();
                    _chartBase.Style = App.Current.FindResource("StackedColumnChartStyleVariableSize") as Style;

                    break;

                case ChartType.StackedBarChart:

                    _chartBase = new StackedBarChart();
                    _chartBase.Style = App.Current.FindResource("StackedBarChartStyleVariableSize") as Style;

                    break;

                case ChartType.PieChart:

                    _chartBase = new PieChart();
                    _chartBase.Style = App.Current.FindResource("PieChartStyleVariableSize") as Style;

                    break;

                case ChartType.DoughnutChart:

                    _chartBase = new DoughnutChart();
                    _chartBase.Style = App.Current.FindResource("DoughnutChartStyleVariableSize") as Style;

                    break;

                default:

                    _chartBase = new ClusteredColumnChart();
                    _chartBase.Style = App.Current.FindResource("ColumnChartStyleVariableSize") as Style;

                    break;
            }

            _chartBase.ChartTitle = "Categories Sales Graph";
            _chartBase.ChartSubTitle = null;
            _chartBase.ToolTipFormat = ToolTipFormat;
            _chartBase.Tag = _chartTypeID;
            if (width > 0 && height > 0)
            {
                _chartBase.Width = width;
                _chartBase.Height = height;
            }
            _chartBase.Series.Add(_chartSeries);
            _gridView.Children.Add(_chartBase);
        }

        #endregion

        #region GetCategories

        /// <summary>
        /// Gets categories.
        /// </summary>
        private void GetCategories()
        {
            try
            {
                base_ProductStoreRepository productStoreRepository = new base_ProductStoreRepository();
                List<base_DepartmentModel> categoryList = null;

                switch ((CategoryOrderBy)_categoryOrderByID)
                {
                    case CategoryOrderBy.TotalSale:

                        if ((OrderDirection)_orderDirectionID == OrderDirection.Highest)
                        {
                            categoryList = UnitOfWork.GetIQueryable<base_ProductStore>(x => x.StoreCode == Define.StoreCode).
                                GroupBy(x => x.base_Product.ProductCategoryId).
                                Select(x => new
                                {
                                    Id = x.Key,
                                    TotalSale = x.Sum(y => y.TotalSale)
                                }).ToList().OrderByDescending(x => x.TotalSale).Skip(0).Take(_total).
                                Join(UnitOfWork.GetIQueryable<base_Department>(), x => x.Id, y => y.Id, (z, t) => new base_DepartmentModel
                                {
                                    Id = z.Id,
                                    Name = t.Name,
                                    TotalSale = z.TotalSale
                                }).Where(x => x.TotalSale > 0).ToList();
                        }
                        else
                        {
                            categoryList = UnitOfWork.GetIQueryable<base_ProductStore>(x => x.StoreCode == Define.StoreCode).
                                GroupBy(x => x.base_Product.ProductCategoryId).
                                Select(x => new
                                {
                                    Id = x.Key,
                                    TotalSale = x.Sum(y => y.TotalSale)
                                }).ToList().OrderBy(x => x.TotalSale).Skip(0).Take(_total).
                                Join(UnitOfWork.GetIQueryable<base_Department>(), x => x.Id, y => y.Id, (z, t) => new base_DepartmentModel
                                {
                                    Id = z.Id,
                                    Name = t.Name,
                                    TotalSale = z.TotalSale
                                }).Where(x => x.TotalSale > 0).ToList();
                        }

                        if ((ChartType)_chartTypeID == ChartType.PieChart || (ChartType)_chartTypeID == ChartType.DoughnutChart)
                        {
                            _chartBase.ToolTipFormat = ToolTipFormatWithPercent;
                            _chartBase.ChartSubTitle = "Total Sale";
                        }
                        _chartSeries.SeriesTitle = "Total Sale";
                        _chartSeries.ValueMember = "TotalSale";

                        break;

                    case CategoryOrderBy.SoldQuantity:

                        if ((OrderDirection)_orderDirectionID == OrderDirection.Highest)
                        {
                            categoryList = UnitOfWork.GetIQueryable<base_ProductStore>(x => x.StoreCode == Define.StoreCode).
                                GroupBy(x => x.base_Product.ProductCategoryId).
                                Select(x => new
                                {
                                    Id = x.Key,
                                    SoldQuantity = x.Sum(y => y.SoldQuantity)
                                }).ToList().OrderByDescending(x => x.SoldQuantity).Skip(0).Take(_total).
                                Join(UnitOfWork.GetIQueryable<base_Department>(), x => x.Id, y => y.Id, (z, t) => new base_DepartmentModel
                                {
                                    Id = z.Id,
                                    Name = t.Name,
                                    SoldQuantity = z.SoldQuantity
                                }).Where(x => x.SoldQuantity > 0).ToList();
                        }
                        else
                        {
                            categoryList = UnitOfWork.GetIQueryable<base_ProductStore>(x => x.StoreCode == Define.StoreCode).
                                GroupBy(x => x.base_Product.ProductCategoryId).
                                Select(x => new
                                {
                                    Id = x.Key,
                                    SoldQuantity = x.Sum(y => y.SoldQuantity)
                                }).ToList().OrderBy(x => x.SoldQuantity).Skip(0).Take(_total).
                                Join(UnitOfWork.GetIQueryable<base_Department>(), x => x.Id, y => y.Id, (z, t) => new base_DepartmentModel
                                {
                                    Id = z.Id,
                                    Name = t.Name,
                                    SoldQuantity = z.SoldQuantity
                                }).Where(x => x.SoldQuantity > 0).ToList();
                        }

                        if ((ChartType)_chartTypeID == ChartType.PieChart || (ChartType)_chartTypeID == ChartType.DoughnutChart)
                        {
                            _chartBase.ToolTipFormat = ToolTipFormatWithPercent;
                            _chartBase.ChartSubTitle = "Sold Quantity";
                        }
                        _chartSeries.SeriesTitle = "Sold Quantity";
                        _chartSeries.ValueMember = "SoldQuantity";

                        break;
                }

                if (categoryList != null && categoryList.Any())
                {
                    CategoryCollection = new CollectionBase<base_DepartmentModel>(categoryList);
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region SaveConfiguration

        /// <summary>
        /// Save configuration.
        /// </summary>
        private void SaveConfiguration()
        {
            _configuration = new XElement("Configuration",
                new XAttribute("Total", _total),
                new XAttribute("CategoryOrderByID", _categoryOrderByID),
                new XAttribute("OrderDirectionID", _orderDirectionID),
                new XAttribute("ChartTypeID", _chartTypeID));
        }

        #endregion

        #endregion

        #region Events

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
                return true;
            }
        }

        public void Lock()
        {
            GridViewVisibility = Visibility.Visible;
            GridEditVisibility = Visibility.Collapsed;
        }

        public void Unlock()
        {
            GridViewVisibility = Visibility.Collapsed;
            GridEditVisibility = Visibility.Visible;
        }

        public XElement GetConfiguration()
        {
            if (_configuration == null)
            {
                SaveConfiguration();
            }

            return _configuration;
        }

        public void UpdateSize(Size newSize)
        {
            double width = (newSize.Width / 3);
            double height = (newSize.Height / 2);

            if (_chartBase != null)
            {
                if (double.IsNaN(_chartBase.Width) || _chartBase.Width < width)
                {
                    _chartBase.Width = width;
                }
                if (double.IsNaN(_chartBase.Height) || _chartBase.Height < height)
                {
                    _chartBase.Height = height;
                }
            }
        }

        #endregion
    }
}
