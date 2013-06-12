using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using System.Collections.ObjectModel;
using CPC.Helper;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Specialized;

namespace CPC.POS.ViewModel
{
    class RewardEarnViewModel : ViewModelBase
    {
        #region Define
        public int GuestID { get; set; }
        public base_RewardManagerModel RewardProgram { get; set; }

        private ICollectionView _collectionView;

        private bool _breakChanged = true;
        #endregion

        #region Constructors
        public RewardEarnViewModel()
            : base()
        {
            _ownerViewModel = this;
            InitialCommand();
        }

        public RewardEarnViewModel(long guestId, base_RewardManagerModel rewardModel, CollectionBase<base_GuestRewardModel> guestRewardCollection)
            : this()
        {
            GuestID = (int)guestId;
            RewardProgram = rewardModel;
            //_guestRewardCollection = guestRewardCollection;
            GuestRewardList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(GuestRewardList_CollectionChanged);
            foreach (base_GuestRewardModel guestRewardModel in guestRewardCollection)
            {
                base_GuestRewardModel guestReward = guestRewardModel.CloneObj();
                guestReward.RewardAmount = rewardModel.RewardProgramText + " off";
                GuestRewardList.Add(guestReward);
            }

            FilterGuestReward();
        }


        #endregion

        #region Properties

        #region GuestRewardCollection
        private CollectionBase<base_GuestRewardModel> _guestRewardCollection;
        /// <summary>
        /// Gets or sets the GuestRewardCollection.
        /// </summary>
        public CollectionBase<base_GuestRewardModel> GuestRewardCollection
        {
            get { return _guestRewardCollection; }
            set
            {
                if (_guestRewardCollection != value)
                {
                    _guestRewardCollection = value;
                    OnPropertyChanged(() => GuestRewardCollection);
                }
            }
        }
        #endregion

        #region GuestRewardList
        private CollectionBase<base_GuestRewardModel> _guestRewardList = new CollectionBase<base_GuestRewardModel>();
        /// <summary>
        /// Gets or sets the GuestRewardList.
        /// <para>Using Binding in View</para>
        /// </summary>
        public CollectionBase<base_GuestRewardModel> GuestRewardList
        {
            get { return _guestRewardList; }
            set
            {
                if (_guestRewardList != value)
                {
                    _guestRewardList = value;
                    OnPropertyChanged(() => GuestRewardList);
                }
            }
        }
        #endregion

        #region CheckedAll
        private bool _checkedAll;
        /// <summary>
        /// Gets or sets the CheckAll.
        /// </summary>
        public bool CheckedAll
        {
            get { return _checkedAll; }
            set
            {
                if (_checkedAll != value)
                {
                    _checkedAll = value;
                    ChechedAllChanged();
                    OnPropertyChanged(() => CheckedAll);
                }
            }
        }


        #endregion

        #region Total
        private int _total;
        /// <summary>
        /// Gets or sets the Total.
        /// </summary>
        public int Total
        {
            get { return _total; }
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


        #endregion

        #region Commands Methods

        #region OkCommand

        /// <summary>
        /// Gets the Ok Command.
        /// <summary>

        public RelayCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Ok command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            _collectionView.Filter = null;

            _collectionView.Refresh();
            _guestRewardCollection = GuestRewardList;
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }
        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>

        public RelayCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelCommandExecute(object param)
        {
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #region IssueNewRewardCommand
        /// <summary>
        /// Gets the IssueNewReward Command.
        /// <summary>

        public RelayCommand<object> IssueNewRewardCommand { get; private set; }



        /// <summary>
        /// Method to check whether the IssueNewReward command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnIssueNewRewardCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the IssueNewReward command is executed.
        /// </summary>
        private void OnIssueNewRewardCommandExecute(object param)
        {
            base_GuestRewardModel issueNewReward = new base_GuestRewardModel();
            issueNewReward.Status = (short)GuestRewardStatus.Available;
            issueNewReward.Reason = "Manual";
            issueNewReward.SaleOrderNo = string.Empty;
            issueNewReward.SaleOrderResource = string.Empty;
            issueNewReward.Amount = 0;
            issueNewReward.RewardValue = 0;
            issueNewReward.EarnedDate = DateTime.Today;
            issueNewReward.Remark = string.Empty;
            issueNewReward.IsApply = false;
            issueNewReward.RewardId = RewardProgram.Id;
            issueNewReward.ActivedDate = DateTime.Today;
            int expireDay = Convert.ToInt32(Common.RewardExpirationTypes.Single(x => Convert.ToInt32(x.ObjValue) == RewardProgram.RewardExpiration).Detail);
            issueNewReward.ExpireDate = issueNewReward.ActivedDate.Value.AddDays(expireDay);
            issueNewReward.RewardAmount = RewardProgram.RewardProgramText + " off";
            issueNewReward.GuestId = GuestID;
            this.GuestRewardList.Add(issueNewReward);
            Total++;
        }
        #endregion

        #region DeleteRewardCommand

        /// <summary>
        /// Gets the DeleteReward Command.
        /// <summary>

        public RelayCommand<object> DeleteRewardCommand { get; private set; }


        /// <summary>
        /// Method to check whether the DeleteReward command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteRewardCommandCanExecute(object param)
        {
            if (GuestRewardList == null)
                return false;
            return GuestRewardList.Any(x => x.IsChecked);
        }


        /// <summary>
        /// Method to invoke when the DeleteReward command is executed.
        /// </summary>
        private void OnDeleteRewardCommandExecute(object param)
        {
            foreach (base_GuestRewardModel guestRewardModel in this.GuestRewardList.Where(x => x.IsChecked).ToList())
            {
                if (guestRewardModel.IsNew)
                    this.GuestRewardList.Remove(guestRewardModel);
                else
                {
                    guestRewardModel.Status = (int)GuestRewardStatus.Removed;
                    guestRewardModel.IsChecked = false;
                }
                Total--;
            }
            FilterGuestReward();
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
            IssueNewRewardCommand = new RelayCommand<object>(OnIssueNewRewardCommandExecute, OnIssueNewRewardCommandCanExecute);
            DeleteRewardCommand = new RelayCommand<object>(OnDeleteRewardCommandExecute, OnDeleteRewardCommandCanExecute);
        }

        private void FilterGuestReward()
        {
            if (_collectionView == null)
                _collectionView = CollectionViewSource.GetDefaultView(this.GuestRewardList);

            _collectionView.Filter = obj =>
            {
                base_GuestRewardModel guestRewardModel = obj as base_GuestRewardModel;
                return guestRewardModel != null && (guestRewardModel.Status.Value.Equals((short)GuestRewardStatus.Available) || guestRewardModel.Status.Value.Equals((short)GuestRewardStatus.Pending));
            };

            Total = _collectionView.OfType<object>().Count();
        }

        private void ChechedAllChanged()
        {
            _breakChanged = true;
            foreach (base_GuestRewardModel guestRewardModel in _collectionView.OfType<base_GuestRewardModel>())
            {
                guestRewardModel.IsChecked = _checkedAll;
            }
            _breakChanged = false;
        }

        private void GuestRewardList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base_GuestRewardModel guestRewardModel;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    guestRewardModel = item as base_GuestRewardModel;
                    guestRewardModel.PropertyChanged += new PropertyChangedEventHandler(guestRewardModel_PropertyChanged);
                }

            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    guestRewardModel = item as base_GuestRewardModel;
                    guestRewardModel.PropertyChanged -= new PropertyChangedEventHandler(guestRewardModel_PropertyChanged);
                }
            }
        }

        private void guestRewardModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_breakChanged)
                return;
            if (e.PropertyName.Equals("IsChecked"))
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_collectionView.OfType<base_GuestRewardModel>().Count(x => x.IsChecked) == _collectionView.OfType<base_GuestRewardModel>().Count())
                        _checkedAll = true;
                    else
                        _checkedAll = false;
                    OnPropertyChanged(() => CheckedAll);
                }), System.Windows.Threading.DispatcherPriority.Background);
                
            }
        }
        #endregion

        #region Public Methods
        #endregion
    }


}
