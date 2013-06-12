using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using CPC.Helper;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Windows;
using System.Collections.Generic;
using CPC.POS.Repository;

namespace CPC.POS.ViewModel
{
    class ProblemDetectionViewModel : ViewModelBase
    {
        #region Define

        public RelayCommand OKCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        private base_SaleOrderRepository _saleOrderRepository = new base_SaleOrderRepository();
        private base_PurchaseOrderRepository _purchaseOrderRepository = new base_PurchaseOrderRepository();
        private base_SaleCommissionRepository _saleCommissionRepository = new base_SaleCommissionRepository();
        #endregion

        #region Constructors

        /// <summary>
        /// Require ItemCollection & param
        /// </summary>
        public ProblemDetectionViewModel()
        {
        }

        public ProblemDetectionViewModel(List<ItemModel> ItemCollection, string param)
        {
            this.InitialCommand();
            this.GetProblemDetection(ItemCollection, param);
        }
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
            System.Windows.Window window = FindOwnerWindow(this);
            window.DialogResult = true;
        }

        #endregion

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the ProblemDetectionCollection.
        /// </summary>
        private ObservableCollection<ProblemDetectionModel> _problemDetectionCollection;
        public ObservableCollection<ProblemDetectionModel> ProblemDetectionCollection
        {
            get { return _problemDetectionCollection; }
            set
            {
                if (_problemDetectionCollection != value)
                {
                    _problemDetectionCollection = value;
                    OnPropertyChanged(() => ProblemDetectionCollection);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        private string _content;
        public string Content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(() => Content);
                }
            }
        }
        /// <summary>
        /// Gets or sets the Message.
        /// </summary>
        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(() => Message);
                }
            }
        }
        #endregion

        #region Private Methods

        private void InitialCommand()
        {
            // Route the commands
            this.OKCommand = new RelayCommand(OnOKCommandExecute, OnOKCommandCanExecute);
         }

        private void GetProblemDetection(List<ItemModel> ItemCollection, string param)
        {
            this.ProblemDetectionCollection = new ObservableCollection<ProblemDetectionModel>();
            switch (param)
            {
                case "SaleOrder":
                    this.Content = "SO#:";
                    foreach (var item in ItemCollection)
                    {
                        ProblemDetectionModel model = new ProblemDetectionModel();
                        model.Id = item.Id;
                        model.Text = item.Text;
                        model.Resource = item.Resource;
                        var query = _saleOrderRepository.GetAll(x => x.CustomerResource == model.Resource);//.Where(x => x.CustomerResource == model.Resource);
                        model.AssociateCollection = new ObservableCollection<ItemModel>();
                        foreach (var itemSale in query)
                            model.AssociateCollection.Add(new ItemModel { Id = itemSale.Id, Text = itemSale.SONumber, Resource = itemSale.Resource.ToString() });
                        this.ProblemDetectionCollection.Add(model);
                    }
                    break;

                case "PurchaseOrder":
                    this.Content = "PO#:";
                    foreach (var item in ItemCollection)
                    {
                        ProblemDetectionModel model = new ProblemDetectionModel();
                        model.Id = item.Id;
                        model.Text = item.Text;
                        model.Resource = item.Resource;
                        model.AssociateCollection = new ObservableCollection<ItemModel>();
                        var query = _purchaseOrderRepository.GetAll(x => x.VendorResource == model.Resource);//.Where(x => x.VendorCode == model.Resource);
                        foreach (var itemPo in query)
                            model.AssociateCollection.Add(new ItemModel { Id = itemPo.Id, Text = itemPo.PurchaseOrderNo, Resource = itemPo.Resource.ToString() });
                        this.ProblemDetectionCollection.Add(model);
                    }
                    break;
                case "Employee":
                    this.Content = "SO#:";
                    foreach (var item in ItemCollection)
                    {
                        ProblemDetectionModel model = new ProblemDetectionModel();
                        model.Id = item.Id;
                        model.Text = item.Text;
                        model.Resource = item.Resource;
                        model.AssociateCollection = new ObservableCollection<ItemModel>();
                        var query = _saleCommissionRepository.GetAll(x => x.GuestResource == model.Resource);//.Where(x => x.VendorCode == model.Resource);
                        foreach (var itemPo in query)
                            model.AssociateCollection.Add(new ItemModel { Id = itemPo.Id, Text = itemPo.SONumber, Resource = itemPo.SOResource});
                        this.ProblemDetectionCollection.Add(model);
                    }
                    break;
            }
            string itemContent = string.Empty;
            if (this.ProblemDetectionCollection.Count > 1)
                itemContent = this.ProblemDetectionCollection.Count + " items";
            else
                itemContent = "1 item";
            this.Message = String.Format("{0} listed on transaction.", itemContent);
        }
        #endregion
    }
}
