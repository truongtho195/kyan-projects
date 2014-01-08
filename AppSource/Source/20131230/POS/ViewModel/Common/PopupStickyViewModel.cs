using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    public class PopupStickyViewModel : ViewModelBase
    {
        #region Defines

        private base_ResourceNoteRepository _resourceNoteRepository = new base_ResourceNoteRepository();

        private PopupStickyView _popupStickyView;

        private double _height = 120;
        private double _width = 150;
        private double _minHeight = 120;
        private double _minWidth = 150;

        private double _stickyDistance = 20;
        private Point _position = new Point(400, 200);
        private string _parentResource;

        #endregion

        #region Properties

        private base_ResourceNoteModel _selectedResourceNote;
        /// <summary>
        /// Gets or sets the SelectedResourceNote.
        /// </summary>
        public base_ResourceNoteModel SelectedResourceNote
        {
            get { return _selectedResourceNote; }
            set
            {
                if (_selectedResourceNote != value)
                {
                    _selectedResourceNote = value;
                    OnPropertyChanged(() => SelectedResourceNote);
                }
            }
        }

        private CollectionBase<base_ResourceNoteModel> _resourceNoteCollection;
        /// <summary>
        /// Gets or sets the ResourceNoteCollection.
        /// </summary>
        public CollectionBase<base_ResourceNoteModel> ResourceNoteCollection
        {
            get { return _resourceNoteCollection; }
            set
            {
                if (_resourceNoteCollection != value)
                {
                    _resourceNoteCollection = value;
                    OnPropertyChanged(() => ResourceNoteCollection);

                    if (ResourceNoteCollection != null && ResourceNoteCollection.Count > 0)
                    {
                        foreach (base_ResourceNoteModel resourceNoteItem in ResourceNoteCollection)
                        {
                            // Set position
                            SetPositionSticky(resourceNoteItem);
                        }
                    }
                }
            }
        }

        private ObservableCollection<PopupStickyView> _popupStickyCollection = new ObservableCollection<PopupStickyView>();
        /// <summary>
        /// Gets or sets the PopupStickyCollection.
        /// </summary>
        public ObservableCollection<PopupStickyView> PopupStickyCollection
        {
            get { return _popupStickyCollection; }
            set
            {
                if (_popupStickyCollection != value)
                {
                    _popupStickyCollection = value;
                    OnPropertyChanged(() => PopupStickyCollection);
                }
            }
        }

        private string _addStickyText = Language.AddSticky;
        /// <summary>
        /// Gets or sets the AddStickyText.
        /// </summary>
        public string AddStickyText
        {
            get { return _addStickyText; }
            set
            {
                if (_addStickyText != value)
                {
                    _addStickyText = value;
                    OnPropertyChanged(() => AddStickyText);
                }
            }
        }

        /// <summary>
        /// Gets show or hidden sticky text
        /// </summary>
        public string ShowOrHiddenStickyText
        {
            get
            {
                if (PopupStickyCollection.Count == 0)
                    return Language.ShowStickies;
                else if (PopupStickyCollection.Count == ResourceNoteCollection.Count && PopupStickyCollection.Any(x => x.IsVisible))
                    return Language.HideStickies;
                else
                    return Language.ShowStickies;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupStickyViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            // Initial note collection
            PopupStickyCollection = new ObservableCollection<PopupStickyView>();
            PopupStickyCollection.CollectionChanged += (sender, e) =>
            {
                // Update text "Show/Hidden sticky"
                OnPropertyChanged(() => ShowOrHiddenStickyText);
            };

            InitialCommands();
        }

        /// <summary>
        /// Constructor to create new popup sticky
        /// </summary>
        /// <param name="resourceNoteModel"></param>
        /// <param name="resourceNoteCollection"></param>
        /// <param name="popupStickyCollection"></param>
        public PopupStickyViewModel(base_ResourceNoteModel resourceNoteModel,
            CollectionBase<base_ResourceNoteModel> resourceNoteCollection,
            ObservableCollection<PopupStickyView> popupStickyCollection)
            : this()
        {
            _parentResource = resourceNoteModel.Resource;
            SelectedResourceNote = resourceNoteModel;
            ResourceNoteCollection = resourceNoteCollection;
            PopupStickyCollection = popupStickyCollection;

            // Initial popup sticky view
            _popupStickyView = new PopupStickyView();
            _popupStickyView.DataContext = this;
            _popupStickyView.Owner = App.Current.MainWindow;

            // Add popup sticky view to collection
            PopupStickyCollection.Add(_popupStickyView);

            // Set size
            _popupStickyView.Height = _height;
            _popupStickyView.Width = _width;
            _popupStickyView.MinHeight = _minHeight;
            _popupStickyView.MinWidth = _minWidth;

            // Register Deactived event
            _popupStickyView.Deactivated += (sender, e) => { SaveSticky(SelectedResourceNote); };

            // Register Loaded event
            _popupStickyView.Loaded += (sender, e) =>
            {
                // Set position for sticky when display
                _popupStickyView.Left = SelectedResourceNote.Position.X;
                _popupStickyView.Top = SelectedResourceNote.Position.Y;

                _popupStickyView.txtContent.Focus();

                // Set key binding
                (_ownerViewModel as MainViewModel).SetKeyBinding(App.Current.MainWindow.InputBindings, _popupStickyView);
            };

            // Show popup sticky view
            _popupStickyView.Show();
        }

        #endregion

        #region Commands

        #region NewCommand

        /// <summary>
        /// Gets the NewCommand command.
        /// </summary>
        public ICommand NewCommand { get; private set; }

        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            NewSticky();
        }

        #endregion

        #region DeleteCommand

        /// <summary>
        /// Gets the DeleteCommand command.
        /// </summary>
        public ICommand DeleteCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            DeleteSticky(SelectedResourceNote);
        }

        #endregion

        #region ShowOrHiddenCommand

        /// <summary>
        /// Gets the ShowOrHiddenCommand command.
        /// </summary>
        public ICommand ShowOrHiddenCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ShowOrHiddenCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnShowOrHiddenCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ShowOrHiddenCommand command is executed.
        /// </summary>
        private void OnShowOrHiddenCommandExecute()
        {
            if (PopupStickyCollection.Count == ResourceNoteCollection.Count)
            {
                // Created popup notes, only show or hidden them
                if (ShowOrHiddenStickyText.Equals(Language.HideStickies))
                {
                    foreach (PopupStickyView popupContainer in PopupStickyCollection)
                        popupContainer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    foreach (PopupStickyView popupContainer in PopupStickyCollection)
                        popupContainer.Show();
                }
            }
            else
            {
                // Close all note
                CloseAllPopupSticky();

                foreach (base_ResourceNoteModel resourceNoteItem in ResourceNoteCollection)
                {
                    PopupStickyViewModel popupStickyViewModel = new PopupStickyViewModel(resourceNoteItem,
                        ResourceNoteCollection, PopupStickyCollection);
                }
            }

            // Update text "Show/Hidden sticky"
            OnPropertyChanged(() => ShowOrHiddenStickyText);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Initial commands
        /// </summary>
        private void InitialCommands()
        {
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            ShowOrHiddenCommand = new RelayCommand(OnShowOrHiddenCommandExecute, OnShowOrHiddenCommandCanExecute);
        }

        /// <summary>
        /// Create new and show sticky
        /// </summary>
        private void NewSticky()
        {
            if (ResourceNoteCollection.Count == Define.CONFIGURATION.DefaultMaximumSticky)
            {
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("You can not create more sticky?", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create a new resource note
            base_ResourceNoteModel resourceNoteModel = new base_ResourceNoteModel
            {
                Resource = _parentResource,
                Color = Define.DefaultColorNote,
                DateCreated = DateTimeExt.Now
            };

            // Add new resource note to collection
            ResourceNoteCollection.Add(resourceNoteModel);

            // Set position
            SetPositionSticky(resourceNoteModel);

            // Create and show popup sticky
            PopupStickyViewModel popupStickyViewModel = new PopupStickyViewModel(resourceNoteModel,
                ResourceNoteCollection, PopupStickyCollection);

            // Update text "Show/Hidden sticky"
            OnPropertyChanged(() => ShowOrHiddenStickyText);
        }

        private void SetPositionSticky(base_ResourceNoteModel resourceNoteModel)
        {
            // Create default position for resource note
            Point position = _position;

            if (ResourceNoteCollection.Count > 0)
            {
                int previousIndex = ResourceNoteCollection.IndexOf(resourceNoteModel) - 1;

                if (previousIndex >= 0)
                {
                    // Get position of last resource note
                    base_ResourceNoteModel previousResourceNoteModel = ResourceNoteCollection.ElementAt(previousIndex);

                    // Update position
                    position = new Point(previousResourceNoteModel.Position.X + _stickyDistance,
                        previousResourceNoteModel.Position.Y + _stickyDistance);
                }
            }

            // Set position for new resource note
            resourceNoteModel.Position = position;
        }

        /// <summary>
        /// Save sticky
        /// </summary>
        private void SaveSticky(base_ResourceNoteModel resourceNoteModel)
        {
            // Map data from model to entity
            resourceNoteModel.ToEntity();

            // Add new note to database
            if (resourceNoteModel.IsNew)
                _resourceNoteRepository.Add(resourceNoteModel.base_ResourceNote);

            // Accept changes
            _resourceNoteRepository.Commit();

            // Turn off IsDirty & IsNew
            resourceNoteModel.EndUpdate();
        }

        /// <summary>
        /// Delete sticky
        /// </summary>
        private void DeleteSticky(base_ResourceNoteModel resourceNoteModel)
        {
            _popupStickyView.Owner.Activate();

            // Close popup sticky view
            _popupStickyView.Close();

            if (!resourceNoteModel.IsNew)
            {
                // Delete resource note from database
                _resourceNoteRepository.Delete(resourceNoteModel.base_ResourceNote);

                // Accept changes
                _resourceNoteRepository.Commit();
            }

            // Remove resource note from collection
            ResourceNoteCollection.Remove(resourceNoteModel);

            // Remove popup sticky from collection
            PopupStickyCollection.Remove(_popupStickyView);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set parent resource for sticky
        /// </summary>
        /// <param name="parentResource"></param>
        /// <param name="resourceNoteCollection"></param>
        public void SetParentResource(string parentResource, CollectionBase<base_ResourceNoteModel> resourceNoteCollection)
        {
            _parentResource = parentResource;
            ResourceNoteCollection = resourceNoteCollection;
        }

        /// <summary>
        /// Close all popup sticky
        /// </summary>
        public void CloseAllPopupSticky()
        {
            // Remove all popup sticky
            foreach (PopupStickyView popupStickyViewItem in PopupStickyCollection)
                popupStickyViewItem.Close();

            // Clear popup sticky collection
            PopupStickyCollection.Clear();
        }

        /// <summary>
        /// Close and delete all resource note
        /// </summary>
        public void DeleteAllResourceNote()
        {
            // Close all popup sticky
            CloseAllPopupSticky();

            // Delete resource note from database
            foreach (base_ResourceNoteModel resourceNoteItem in ResourceNoteCollection)
                _resourceNoteRepository.Delete(resourceNoteItem.base_ResourceNote);

            // Clear resource note collection
            ResourceNoteCollection.Clear();

            // Accept changes
            _resourceNoteRepository.Commit();
        }

        /// <summary>
        /// Close and delete all resource note
        /// </summary>
        public void DeleteAllResourceNote(CollectionBase<base_ResourceNoteModel> resourceNoteCollection)
        {
            // Close all popup sticky
            CloseAllPopupSticky();

            // Delete resource note from database
            foreach (base_ResourceNoteModel resourceNoteItem in resourceNoteCollection)
                _resourceNoteRepository.Delete(resourceNoteItem.base_ResourceNote);

            // Clear resource note collection
            resourceNoteCollection.Clear();

            // Accept changes
            _resourceNoteRepository.Commit();
        }

        #endregion
    }
}