using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows;
using CPC.POS.Database;
using CPC.POS.Repository;
using CPC.POS.Model;
using System.Windows.Controls;
using CPC.POS.Interfaces;
using CPC.Service.FrameworkDialogs.OpenFile;
using System.IO;
using System.Diagnostics;
using CPC.Control;
using CPC.POS.View;
using System.Data;
using CPC.Toolkit.Scanner;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;

namespace CPC.POS.ViewModel
{
    class AttachmentViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// Used for scan.
        /// </summary>
        private WpfTwain _twainInterface = null;

        /// <summary>
        /// Gets folders on a separate thread.
        /// </summary>
        private BackgroundWorker _folderBackgroundWorker = new BackgroundWorker();

        /// <summary>
        /// Get files on a separate thread.
        /// </summary>
        private BackgroundWorker _fileBackgroundWorker = new BackgroundWorker();

        /// <summary>
        /// Get file search on a separate thread.
        /// </summary>
        private BackgroundWorker _fileSearchBackgroundWorker = new BackgroundWorker();

        /// <summary>
        /// Column on attachment table used for sort.
        /// </summary>
        private readonly string _attachmentColumnSort = "It.Id";

        /// <summary>
        /// Used for update folder.
        /// </summary>
        private EditFolderProvider _editFolderProvider = new EditFolderProvider();

        /// <summary>
        /// Used for update attachment.
        /// </summary>
        private EditAttachmentProvider _editAttachmentProvider = new EditAttachmentProvider();

        /// <summary>
        /// Default new folder name.
        /// </summary>
        private readonly string _defaulFolderName = "New Folder";

        /// <summary>
        /// Default root folder name.
        /// </summary>
        private readonly string _defaulRootFolderName = "All Folders";

        /// <summary>
        /// Used for Filter property of OpenFileDialogViewModel.
        /// </summary>
        private readonly string _filter = "All Files (*.*)|*.*|" +
            "Image Files |*.jpg; *.jpeg; *.bmp; *.gif; *.png; *.tif|" +
            "Office Files|*.doc; *.docx; *.xls; *.xlsx; *.ppt; *.pptx";

        /// <summary>
        /// Default image extension is *.jpg.
        /// </summary>
        private readonly string _defaultImageExtension = ".jpg";

        /// <summary>
        /// Image extension support ".bmp", ".jpg", ".jpeg", ".gif", ".png", ".tif", ".tiff".
        /// </summary>
        private readonly string[] _imageExtensionSupport = { ".bmp", ".jpg", ".jpeg", ".gif", ".png", ".tif", ".tiff" };

        /// <summary>
        /// Path used for saves images.
        /// </summary>
        private string _imagePathRoot;

        /// <summary>
        /// Length of file original name is 20.
        /// </summary>
        private readonly short _fileOriginalNameMaxLength = 20;

        /// <summary>
        /// Length of file name is 250.
        /// </summary>
        private readonly short _fileNameMaxLength = 250;

        #endregion

        #region Constructors

        public AttachmentViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            _folderBackgroundWorker.WorkerReportsProgress = true;
            _folderBackgroundWorker.DoWork += new DoWorkEventHandler(FolderWorkerDoWork);
            _folderBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(FolderWorkerProgressChanged);
            _folderBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FolderWorkerRunWorkerCompleted);

            _fileBackgroundWorker.WorkerReportsProgress = true;
            _fileBackgroundWorker.DoWork += new DoWorkEventHandler(FileWorkerDoWork);
            _fileBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(FileWorkerProgressChanged);
            _fileBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FileWorkerRunWorkerCompleted);

            _fileSearchBackgroundWorker.WorkerReportsProgress = true;
            _fileSearchBackgroundWorker.DoWork += new DoWorkEventHandler(FileSearchWorkerDoWork);
            _fileSearchBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(FileSearchWorkerProgressChanged);
            _fileSearchBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FileSearchWorkerRunWorkerCompleted);

            _twainInterface = new WpfTwain();
            _twainInterface.TwainTransferReady += new TwainTransferReadyHandler(TransferReady);
        }

        #endregion

        #region Properties

        #region IsSearchMode

        private bool _isSearchMode;
        /// <summary>
        /// True open search component. False close search component.
        /// </summary>
        public bool IsSearchMode
        {
            get
            {
                return _isSearchMode;
            }
            set
            {
                if (_isSearchMode != value)
                {
                    _isSearchMode = value;
                    OnPropertyChanged(() => IsSearchMode);
                }
            }
        }

        #endregion

        #region FolderCollection

        private CollectionBase<base_VirtualFolderModel> _folderCollection;
        /// <summary>
        /// Gets or sets folder collection that contain child folders.
        /// </summary>
        public CollectionBase<base_VirtualFolderModel> FolderCollection
        {
            get
            {
                return _folderCollection;
            }
            set
            {
                if (_folderCollection != value)
                {
                    _folderCollection = value;
                    OnPropertyChanged(() => FolderCollection);
                }
            }
        }

        #endregion

        #region SelectedFolder

        private base_VirtualFolderModel _selectedFolder;
        /// <summary>
        /// Gets or sets selected folder on TreeView.
        /// </summary>
        public base_VirtualFolderModel SelectedFolder
        {
            get
            {
                return _selectedFolder;
            }
            set
            {
                if (_selectedFolder != value)
                {
                    OnSelectedFolderChanging();
                    _selectedFolder = value;
                    OnPropertyChanged(() => SelectedFolder);
                    OnSelectedFolderChanged();
                }
            }
        }

        #endregion

        #region SearchString

        private string _searchString = string.Empty;
        /// <summary>
        /// Gets or sets key word used for search files.
        /// </summary>
        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                if (_searchString != value)
                {
                    _searchString = value;
                    OnPropertyChanged(() => SearchString);
                    OnSearchStringChanged();
                }
            }
        }

        #endregion

        #region SearchResultCollection

        private CollectionBase<base_AttachmentModel> _searchResultCollection;
        /// <summary>
        /// Gets or sets search result collection.
        /// </summary>
        public CollectionBase<base_AttachmentModel> SearchResultCollection
        {
            get
            {
                return _searchResultCollection;
            }
            set
            {
                if (_searchResultCollection != value)
                {
                    _searchResultCollection = value;
                    OnPropertyChanged(() => SearchResultCollection);
                }
            }
        }

        #endregion

        #region FileSearchTotal

        private int _fileSearchTotal;
        /// <summary>
        /// Gets or sets file search total.
        /// </summary>
        public int FileSearchTotal
        {
            get
            {
                return _fileSearchTotal;
            }
            set
            {
                if (_fileSearchTotal != value)
                {
                    _fileSearchTotal = value;
                    OnPropertyChanged(() => FileSearchTotal);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region OpenOrCloseSearchCommand

        private ICommand _openOrCloseSearchCommand;
        /// <summary>
        /// When 'Search' Button, and 'Back' Button clicked, command will executes.
        /// </summary>
        public ICommand OpenOrCloseSearchCommand
        {
            get
            {
                if (_openOrCloseSearchCommand == null)
                {
                    _openOrCloseSearchCommand = new RelayCommand(OpenOrCloseSearchExecute);
                }
                return _openOrCloseSearchCommand;
            }
        }

        #endregion

        #region SelectedItemChangedCommand

        private ICommand _selectedItemChangedCommand;
        /// <summary>
        /// When event SelectedItemChanged on TreeView occurs, SelectedItemChangedCommand will executes.
        /// </summary>
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (_selectedItemChangedCommand == null)
                {
                    _selectedItemChangedCommand = new RelayCommand<TreeView>(SelectedItemChangedCommandExecute);
                }
                return _selectedItemChangedCommand;
            }
        }

        #endregion

        #region GetFilesCommand

        private ICommand _getFilesCommand;
        /// <summary>
        /// When folder selected, or DataGrid scroll, command will executes.
        /// </summary>
        public ICommand GetFilesCommand
        {
            get
            {
                if (_getFilesCommand == null)
                {
                    _getFilesCommand = new RelayCommand(GetFilesExecute, CanGetFilesExecute);
                }
                return _getFilesCommand;
            }
        }

        #endregion

        #region CreateFolderCommand

        private ICommand _createFolderCommand;
        /// <summary>
        /// When 'New Folder' Button clicked, command will executes.
        /// </summary>
        public ICommand CreateFolderCommand
        {
            get
            {
                if (_createFolderCommand == null)
                {
                    _createFolderCommand = new RelayCommand(CreateFolderExecute, CanCreateFolderExecute);
                }
                return _createFolderCommand;
            }
        }

        #endregion

        #region EditFolderCommand

        private ICommand _editFolderCommand;
        /// <summary>
        /// When 'Edit' Button clicked, command will executes.
        /// </summary>
        public ICommand EditFolderCommand
        {
            get
            {
                if (_editFolderCommand == null)
                {
                    _editFolderCommand = new RelayCommand(EditFolderExecute, CanEditFolderExecute);
                }
                return _editFolderCommand;
            }
        }

        #endregion

        #region EndEditFolderCommand

        private ICommand _endEditFolderCommand;
        /// <summary>
        /// When presses 'Enter' key on TextBox that contains folder'name, command will executes.
        /// </summary>
        public ICommand EndEditFolderCommand
        {
            get
            {
                if (_endEditFolderCommand == null)
                {
                    _endEditFolderCommand = new RelayCommand(EndEditFolderExecute, CanEndEditFolderExecute);
                }
                return _endEditFolderCommand;
            }
        }

        #endregion

        #region DeleteFolderCommand

        private ICommand _deleteFolderCommand;
        /// <summary>
        /// When 'Delete' Button clicked, command will executes.
        /// </summary>
        public ICommand DeleteFolderCommand
        {
            get
            {
                if (_deleteFolderCommand == null)
                {
                    _deleteFolderCommand = new RelayCommand(DeleteFolderExecute, CanDeleteFolderExecute);
                }
                return _deleteFolderCommand;
            }
        }

        #endregion

        #region ImportFilesCommand

        private ICommand _importFilesCommand;
        /// <summary>
        /// When 'Attach' Button clicked, command will executes.
        /// </summary>
        public ICommand ImportFilesCommand
        {
            get
            {
                if (_importFilesCommand == null)
                {
                    _importFilesCommand = new RelayCommand(ImportFilesExecute, CanImportFilesExecute);
                }
                return _importFilesCommand;
            }
        }

        #endregion

        #region DeleteFilesCommand

        private ICommand _deleteFilesCommand;
        /// <summary>
        /// When 'Delete' Button clicked, command will executes.
        /// </summary>
        public ICommand DeleteFilesCommand
        {
            get
            {
                if (_deleteFilesCommand == null)
                {
                    _deleteFilesCommand = new RelayCommand(DeleteFilesExecute, CanDeleteFilesExecute);
                }
                return _deleteFilesCommand;
            }
        }

        #endregion

        #region OpenFileCommand

        private ICommand _openFileCommand;
        /// <summary>
        /// When 'Open' MenuItem clicked, command will executes.
        /// </summary>
        public ICommand OpenFileCommand
        {
            get
            {
                if (_openFileCommand == null)
                {
                    _openFileCommand = new RelayCommand<base_AttachmentModel>(OpenFileExecute);
                }
                return _openFileCommand;
            }
        }

        #endregion

        #region OpenFileWithCommand

        private ICommand _openFileWithCommand;
        /// <summary>
        /// When 'Open With...' MenuItem clicked, command will executes.
        /// </summary>
        public ICommand OpenFileWithCommand
        {
            get
            {
                if (_openFileWithCommand == null)
                {
                    _openFileWithCommand = new RelayCommand<base_AttachmentModel>(OpenFileWithExecute);
                }
                return _openFileWithCommand;
            }
        }

        #endregion

        #region GetSearchResultCommand

        private ICommand _getSearchResultCommand;
        public ICommand GetSearchResultCommand
        {
            get
            {
                if (_getSearchResultCommand == null)
                {
                    _getSearchResultCommand = new RelayCommand(GetSearchResultExecute);
                }
                return _getSearchResultCommand;
            }
        }

        #endregion

        #region ScanCommand

        private ICommand _scanCommand;
        /// <summary>
        /// When 'Scan' Button clicked, command will executes.
        /// </summary>
        public ICommand ScanCommand
        {
            get
            {
                if (_scanCommand == null)
                {
                    _scanCommand = new RelayCommand(ScanExecute, CanScanExecute);
                }
                return _scanCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region OpenOrCloseSearchExecute

        /// <summary>
        /// Open or close search component.
        /// </summary>
        private void OpenOrCloseSearchExecute()
        {
            IsSearchMode = !_isSearchMode;
        }

        #endregion

        #region SelectedItemChangedCommandExecute

        /// <summary>
        /// Corresponds with event SelectedItemChanged on TreeView.
        /// </summary>
        private void SelectedItemChangedCommandExecute(TreeView treeView)
        {
            // Gets SelectedItem on TreeView.
            SelectedFolder = treeView.SelectedItem as base_VirtualFolderModel;
        }

        #endregion

        #region GetFilesExecute

        /// <summary>
        /// Gets a range file by page.
        /// </summary>
        private void GetFilesExecute()
        {
            if (IsBusy)
            {
                return;
            }

            _fileBackgroundWorker.RunWorkerAsync();
        }

        #endregion

        #region CanGetFilesExecute

        private bool CanGetFilesExecute()
        {
            if (_selectedFolder == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region CreateFolderExecute

        /// <summary>
        /// Create new folder.
        /// </summary>
        private void CreateFolderExecute()
        {
            CreateFolder();
        }

        #endregion

        #region CanCreateFolderExecute

        private bool CanCreateFolderExecute()
        {
            if (_selectedFolder == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region EditFolderExecute

        /// <summary>
        /// Edit folder.
        /// </summary>
        private void EditFolderExecute()
        {
            if (!_selectedFolder.IsEdit)
            {
                _selectedFolder.IsEdit = true;
            }
        }

        #endregion

        #region CanEditFolderExecute

        private bool CanEditFolderExecute()
        {
            if (_selectedFolder == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region EndEditFolderExecute

        /// <summary>
        /// End edit folder.
        /// </summary>
        private void EndEditFolderExecute()
        {
            if (_selectedFolder.IsEdit)
            {
                _selectedFolder.IsEdit = false;
            }
        }

        #endregion

        #region CanEndEditFolderExecute

        private bool CanEndEditFolderExecute()
        {
            if (_selectedFolder == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region DeleteFolderExecute

        /// <summary>
        /// Delete folder.
        /// </summary>
        private void DeleteFolderExecute()
        {
            DeleteFolder();
        }

        #endregion

        #region CanDeleteFolderExecute

        private bool CanDeleteFolderExecute()
        {
            if (_selectedFolder == null || _selectedFolder.IsRoot)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region ImportFilesExecute

        /// <summary>
        /// Imports files.
        /// </summary>
        private void ImportFilesExecute()
        {
            ImportFiles();
        }

        #endregion

        #region CanImportFilesExecute

        private bool CanImportFilesExecute()
        {
            if (_selectedFolder == null || _selectedFolder.FileCollection == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region DeleteFilesExecute

        /// <summary>
        /// Delete files.
        /// </summary>
        private void DeleteFilesExecute()
        {
            DeleteFiles();
        }

        #endregion

        #region CanDeleteFilesExecute

        private bool CanDeleteFilesExecute()
        {
            if (_selectedFolder == null || _selectedFolder.FileCollection == null || !_selectedFolder.FileCollection.Any(x => x.IsChecked))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region OpenFileExecute

        /// <summary>
        /// Open a file.
        /// </summary>
        private void OpenFileExecute(base_AttachmentModel file)
        {
            Open(file);
        }

        #endregion

        #region OpenFileWithExecute

        /// <summary>
        /// Open a file with a selected program.
        /// </summary>
        private void OpenFileWithExecute(base_AttachmentModel file)
        {
            OpenWith(file);
        }

        #endregion

        #region GetSearchResultExecute

        /// <summary>
        /// Get search result.
        /// </summary>
        private void GetSearchResultExecute()
        {
            if (IsBusy)
            {
                return;
            }

            _fileSearchBackgroundWorker.RunWorkerAsync();
        }

        #endregion

        #region ScanExecute

        /// <summary>
        /// Scan.
        /// </summary>
        private void ScanExecute()
        {
            Scan();
        }

        #endregion

        #region CanScanExecute

        private bool CanScanExecute()
        {
            if (_selectedFolder == null || _selectedFolder.FileCollection == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #region OnSelectedFolderChanging

        /// <summary>
        /// Occur before SelectedFolder property changed.
        /// </summary>
        private void OnSelectedFolderChanging()
        {
            if (_selectedFolder != null)
            {
                if (_selectedFolder.IsEdit)
                {
                    _selectedFolder.IsEdit = false;
                }

                if (_selectedFolder.FileCollection != null)
                {
                    ListCollectionView fileCollectionView = CollectionViewSource.GetDefaultView(_selectedFolder.FileCollection) as ListCollectionView;
                    if (fileCollectionView != null && fileCollectionView.IsEditingItem)
                    {
                        (fileCollectionView.CurrentEditItem as base_AttachmentModel).EndEdit();
                        fileCollectionView.CommitEdit();
                    }
                }

                // Load file at next selected.
                _selectedFolder.FileCollection = null;
                _selectedFolder.IsCheckedAllFiles = false;
            }
        }

        #endregion

        #region OnSelectedFolderChanged

        /// <summary>
        /// Occur after SelectedFolder property changed.
        /// </summary>
        private void OnSelectedFolderChanged()
        {
            if (_selectedFolder != null)
            {
                // Load files for selected folder.
                if (!_selectedFolder.IsFilesLoaded)
                {
                    _fileBackgroundWorker.RunWorkerAsync();
                }
            }
        }

        #endregion

        #region OnSearchStringChanged

        /// <summary>
        /// Occur after SearchString property changed.
        /// </summary>
        private void OnSearchStringChanged()
        {
            // Reset seach result.
            SearchResultCollection = null;
            FileSearchTotal = 0;

            // Search files.
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                _fileSearchBackgroundWorker.RunWorkerAsync();
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region Initialize

        /// <summary>
        /// Initialize data.
        /// </summary>
        private void Initialize()
        {
            try
            {
                // Get default image path.
                _imagePathRoot = Define.CONFIGURATION.DefautlImagePath;

                if (!Directory.Exists(_imagePathRoot))
                {
                    Directory.CreateDirectory(_imagePathRoot);
                }

                // Create root folder.
                base_VirtualFolderRepository virtualFolderRepository = new base_VirtualFolderRepository();
                base_VirtualFolder rootFolder = virtualFolderRepository.Get(x => x.ParentFolderId == null && x.IsActived);
                if (rootFolder == null)
                {
                    // Insert root folder.
                    DateTime now = DateTime.Now;
                    base_VirtualFolder folder = new base_VirtualFolder
                    {
                        ParentFolderId = null,
                        FolderName = _defaulRootFolderName,
                        DateCreated = now,
                        DateUpdated = now,
                        IsActived = true,
                    };
                    virtualFolderRepository.Add(folder);
                    virtualFolderRepository.Commit();
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region GetFolders

        /// <summary>
        /// Get all folders.
        /// </summary>
        private void GetFolders()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    // Get root folder.
                    base_VirtualFolderRepository virtualFolderRepository = new base_VirtualFolderRepository();
                    base_VirtualFolder rootFolder = virtualFolderRepository.Get(x => x.ParentFolderId == null && x.IsActived);
                    if (rootFolder != null && virtualFolderRepository.Refresh(rootFolder) != null)
                    {
                        _folderBackgroundWorker.ReportProgress(0, rootFolder);
                    }
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region GetFiles

        /// <summary>
        /// Get all files of selected folder.
        /// </summary>
        private void GetFiles()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_AttachmentRepository attachmentRepository = new base_AttachmentRepository();

                    if (!_selectedFolder.IsFilesLoaded)
                    {
                        _selectedFolder.FileCollection = new CollectionBase<base_AttachmentModel>();
                        _selectedFolder.FileTotal = attachmentRepository.GetIQueryable(x => x.VirtualFolderId == _selectedFolder.Id && x.IsActived).Count();
                        _selectedFolder.IsDirty = false;
                    }

                    // Get range of files.
                    IList<base_Attachment> files = attachmentRepository.GetRange<long>(_selectedFolder.FileCollection.Count, NumberOfDisplayItems, x => x.Id, x => x.VirtualFolderId == _selectedFolder.Id && x.IsActived);

                    foreach (base_Attachment file in files)
                    {
                        if (attachmentRepository.Refresh(file) != null)
                        {
                            _fileBackgroundWorker.ReportProgress(0, file);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region GetFolder

        /// <summary>
        /// Gets a folder.
        /// </summary>
        /// <param name="folder">Folder to gets.</param>
        private void GetFolder(base_VirtualFolder folder)
        {
            // Initialize.
            base_VirtualFolderModel rootFolder = new base_VirtualFolderModel(folder)
            {
                FolderCollection = new CollectionBase<base_VirtualFolderModel>(),
                EditFolderProvider = _editFolderProvider,
                IsExpanded = true,
                IsNew = false,
                IsDirty = false
            };

            // Add root folder to folder collection.
            _folderCollection.Add(rootFolder);

            // Gets child folders of root folder.
            base_VirtualFolderRepository virtualFolderRepository = new base_VirtualFolderRepository();
            List<base_VirtualFolder> activeFolders;
            lock (UnitOfWork.Locker)
            {
                virtualFolderRepository.Refresh(folder.base_VirtualFolder1);
                activeFolders = folder.base_VirtualFolder1.Where(x => x.IsActived).ToList();
            }
            foreach (base_VirtualFolder childFolder in activeFolders)
            {
                GetFolder(rootFolder, childFolder, virtualFolderRepository);
            }
        }

        /// <summary>
        /// Gets a folder.
        /// </summary>
        /// <param name="parentFolder">Parent folder.</param>
        /// <param name="folder">Folder to gets.</param>
        private void GetFolder(base_VirtualFolderModel parentFolder, base_VirtualFolder folder, base_VirtualFolderRepository virtualFolderRepository)
        {
            // Initialize.
            base_VirtualFolderModel currentFolder = new base_VirtualFolderModel(folder)
            {
                FolderCollection = new CollectionBase<base_VirtualFolderModel>(),
                ParentFolder = parentFolder,
                EditFolderProvider = _editFolderProvider,
                IsNew = false,
                IsDirty = false
            };

            // Add current folder to folder collection of parent.
            parentFolder.FolderCollection.Add(currentFolder);

            // Gets child folders of current folder.
            List<base_VirtualFolder> activeFolders;
            lock (UnitOfWork.Locker)
            {
                virtualFolderRepository.Refresh(folder.base_VirtualFolder1);
                activeFolders = folder.base_VirtualFolder1.Where(x => x.IsActived).ToList();
            }
            foreach (base_VirtualFolder childFolder in activeFolders)
            {
                GetFolder(currentFolder, childFolder, virtualFolderRepository);
            }
        }

        #endregion

        #region GetFile

        /// <summary>
        /// Gets a file.
        /// </summary>
        /// <param name="file">File to gets.</param>
        private void GetFile(base_Attachment file)
        {
            if (_selectedFolder.FileCollection.FirstOrDefault(x => x.Id == file.Id) == null)
            {
                base_AttachmentModel newFile = new base_AttachmentModel(file)
                {
                    ParentFolder = _selectedFolder,
                    EditAttachmentProvider = _editAttachmentProvider,
                    IsNew = false,
                    IsDirty = false
                };

                if (_selectedFolder.IsCheckedAllFiles == true)
                {
                    newFile.Checked();
                }

                _selectedFolder.FileCollection.Add(newFile);
            }
        }

        #endregion

        #region CreateFolder

        /// <summary>
        /// Create new folder.
        /// </summary>
        private void CreateFolder()
        {
            try
            {
                base_VirtualFolderRepository virtualFolderRepository = new base_VirtualFolderRepository();

                // Find default name for new folder.
                string folderName;
                int i = 1;
                bool isExist = true;
                do
                {
                    folderName = string.Format("{0} {1}", _defaulFolderName, i++);
                    isExist = virtualFolderRepository.Get(x =>
                        x.ParentFolderId == _selectedFolder.Id &&
                        x.FolderName.ToLower() == folderName.ToLower()) != null;
                }
                while (isExist);

                // Insert new folder.
                DateTime now = DateTime.Now;
                base_VirtualFolder folder = new base_VirtualFolder
                {
                    ParentFolderId = _selectedFolder.Id,
                    FolderName = folderName,
                    DateCreated = now,
                    DateUpdated = now,
                    IsActived = true
                };
                virtualFolderRepository.Add(folder);
                virtualFolderRepository.Commit();

                // Create Model.
                base_VirtualFolderModel newFolder = new base_VirtualFolderModel(folder)
                {
                    FolderCollection = new CollectionBase<base_VirtualFolderModel>(),
                    FileCollection = new CollectionBase<base_AttachmentModel>(),
                    ParentFolder = _selectedFolder,
                    EditFolderProvider = _selectedFolder.EditFolderProvider,
                    IsNew = false,
                    IsDirty = false,
                };

                // Add new folder to folder collection of parent.
                _selectedFolder.FolderCollection.Add(newFolder);
                if (!_selectedFolder.IsExpanded)
                {
                    _selectedFolder.IsExpanded = true;
                }

                // Begin edit new folder.
                newFolder.IsSelected = true;
                newFolder.IsEdit = true;
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region DeleteFolder

        /// <summary>
        /// Delete folder.
        /// </summary>
        private void DeleteFolder()
        {
            try
            {
                base_VirtualFolderRepository virtualFolderRepository = new base_VirtualFolderRepository();

                // Deactive this folder.
                _selectedFolder.DateUpdated = DateTime.Now;
                _selectedFolder.IsActived = false;
                _selectedFolder.IsDeleted = true;
                _selectedFolder.ToEntity();
                virtualFolderRepository.Commit();

                // Remove this folder on TreeView.
                _selectedFolder.ParentFolder.FolderCollection.Remove(_selectedFolder);
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region ImportFiles

        /// <summary>
        /// Import files.
        /// </summary>
        private void ImportFiles()
        {
            OpenFileDialogViewModel openFileDialogViewModel = new OpenFileDialogViewModel();
            openFileDialogViewModel.Multiselect = true;
            openFileDialogViewModel.Filter = _filter;
            System.Windows.Forms.DialogResult result = _dialogService.ShowOpenFileDialog(_ownerViewModel, openFileDialogViewModel);

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string fileName in openFileDialogViewModel.FileNames)
                {
                    InsertFile(new FileInfo(fileName));
                }

                _selectedFolder.DetermineIsCheckedAllFiles();
                _selectedFolder.IsDirty = false;
            }
        }

        #endregion

        #region InsertFile

        /// <summary>
        /// Insert a file.
        /// </summary>
        /// <param name="fileInfo">File information to insert.</param>
        private void InsertFile(FileInfo fileInfo)
        {
            try
            {
                // Get unique name.
                string name = string.Format("{0}{1}", DateTime.Now.ToString("yyMMddHHmmssfff"), fileInfo.Extension.ToLower());
                if (name.Length > _fileOriginalNameMaxLength || fileInfo.Name.Length > _fileNameMaxLength)
                {
                    throw new Exception("File name too long.");
                }

                string destFileName = Path.Combine(_imagePathRoot, name);

                // Save file on disk.
                File.Copy(fileInfo.FullName, destFileName, false);

                // Save file on database.
                base_AttachmentRepository attachmentRepository = new base_AttachmentRepository();

                DateTime now = DateTime.Now;
                base_Attachment file = new base_Attachment()
                {
                    FileOriginalName = name,
                    FileName = fileInfo.Name,
                    FileExtension = fileInfo.Extension.ToLower(),
                    VirtualFolderId = _selectedFolder.Id,
                    DateCreated = now,
                    DateUpdated = now,
                    IsActived = true
                };

                attachmentRepository.Add(file);
                attachmentRepository.Commit();

                base_AttachmentModel newFile = new base_AttachmentModel(file)
                {
                    ParentFolder = _selectedFolder,
                    EditAttachmentProvider = _editAttachmentProvider,
                    IsNew = false,
                    IsDirty = false
                };

                _selectedFolder.FileCollection.Add(newFile);
                _selectedFolder.FileTotal++;
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region DeleteFiles

        /// <summary>
        /// Delete files.
        /// </summary>
        private void DeleteFiles()
        {
            try
            {
                base_AttachmentRepository attachmentRepository = new base_AttachmentRepository();

                List<base_AttachmentModel> filesChecked = _selectedFolder.FileCollection.Where(x => x.IsChecked).ToList();
                foreach (base_AttachmentModel file in filesChecked)
                {
                    // Deactive this file.
                    file.DateUpdated = DateTime.Now;
                    file.IsActived = false;
                    file.IsDeleted = true;
                    file.ToEntity();
                    attachmentRepository.Commit();

                    // Remove this file on selected folder.
                    _selectedFolder.FileCollection.Remove(file);
                    _selectedFolder.FileTotal--;
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                _selectedFolder.DetermineIsCheckedAllFiles();
                _selectedFolder.IsDirty = false;
            }
        }

        #endregion

        #region Open

        /// <summary>
        /// Open a file.
        /// </summary>
        /// <param name="file">File to open.</param>
        private void Open(base_AttachmentModel file)
        {
            try
            {
                // Determine file path.
                string filePath = Path.Combine(_imagePathRoot, file.FileOriginalName);

                // Check support.
                bool hasSupport = _imageExtensionSupport.Contains(file.FileExtension);
                if (hasSupport)
                {
                    _dialogService.ShowDialog<ImageView>(_ownerViewModel, new ImageViewModel(filePath), "Image Viewer", true, false, true);
                }
                else
                {
                    Process.Start(filePath);
                }

                UpdateNumberOfViews(file);
            }
            catch (Win32Exception win32Exception)
            {
                if (win32Exception.NativeErrorCode == 1155)
                {
                    _log4net.Error(string.Format("Message: {0}. Source: {1}", win32Exception.Message, win32Exception.Source));
                    OpenWith(file);
                }
                else
                {
                    _log4net.Error(string.Format("Message: {0}. Source: {1}", win32Exception.Message, win32Exception.Source));
                    MessageBox.Show(win32Exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region OpenWith

        /// <summary>
        /// Open a file with a selected program.
        /// </summary>
        /// <param name="file">File to open.</param>
        private void OpenWith(base_AttachmentModel file)
        {
            try
            {
                // Determine file path.
                string filePath = Path.Combine(_imagePathRoot, file.FileOriginalName);

                //Run 'Open With' window.
                string args = string.Format("{0},{1} {2}",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll"),
                    "OpenAs_RunDLL",
                    filePath);

                Process.Start("rundll32.exe", args);

                UpdateNumberOfViews(file);
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region UpdateNumberOfViews

        /// <summary>
        /// Update number of views of a file.
        /// </summary>
        private void UpdateNumberOfViews(base_AttachmentModel file)
        {
            try
            {
                base_AttachmentRepository attachmentRepository = new base_AttachmentRepository();
                file.Counter++;
                file.ToEntity();
                attachmentRepository.Commit();
                file.IsDirty = false;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Search

        /// <summary>
        /// Search files.
        /// </summary>

        private void Search()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_AttachmentRepository attachmentRepository = new base_AttachmentRepository();

                    if (_searchResultCollection == null)
                    {
                        SearchResultCollection = new CollectionBase<base_AttachmentModel>();
                        FileSearchTotal = attachmentRepository.GetIQueryable(x => x.FileName.ToLower().Contains(_searchString.ToLower()) && x.IsActived).Count();
                    }

                    // Get range of files.
                    IList<base_Attachment> files = attachmentRepository.GetRange(_searchResultCollection.Count, NumberOfDisplayItems, _attachmentColumnSort, x => x.FileName.ToLower().Contains(_searchString.ToLower()) && x.IsActived);
                    foreach (base_Attachment file in files)
                    {
                        if (attachmentRepository.Refresh(file) != null)
                        {
                            _fileSearchBackgroundWorker.ReportProgress(0, file);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region GetFileSearch

        /// <summary>
        /// Gets a file search.
        /// </summary>
        /// <param name="file">File to gets.</param>
        private void GetFileSearch(base_Attachment file)
        {
            if (_searchResultCollection.FirstOrDefault(x => x.Id == file.Id) == null)
            {
                _searchResultCollection.Add(new base_AttachmentModel(file)
                {
                    ParentFolder = new base_VirtualFolderModel(file.base_VirtualFolder),
                    IsNew = false,
                    IsDirty = false
                });
            }
        }

        #endregion

        #region Scan

        /// <summary>
        /// Scan documents.
        /// </summary>
        private void Scan()
        {
            if (_twainInterface.Select() == TwRC.Success)
            {
                _twainInterface.Acquire(false);
            }
            else
            {
                MessageBox.Show("Scanner device not found!");
            }
        }

        #endregion

        #region SaveFileScan

        /// <summary>
        /// Save a file after scan.
        /// </summary>
        private void SaveFileScan(ImageSource imageSource)
        {
            try
            {
                // Get unique name.
                string name = string.Format("{0}{1}", DateTime.Now.ToString("yyMMddHHmmssfff"), _defaultImageExtension);
                string filePath = Path.Combine(_imagePathRoot, name);

                // Save file on disk.
                CachedBitmap cachedBitmap = imageSource as CachedBitmap;
                JpegBitmapEncoder jpegBitmapEncoder = new JpegBitmapEncoder();
                jpegBitmapEncoder.Frames.Add(BitmapFrame.Create(cachedBitmap));
                using (FileStream imageFile = File.OpenWrite(filePath))
                {
                    jpegBitmapEncoder.Save(imageFile);
                }

                // Save file on database.
                base_AttachmentRepository attachmentRepository = new base_AttachmentRepository();
                DateTime now = DateTime.Now;
                base_Attachment file = new base_Attachment()
                {
                    FileOriginalName = name,
                    FileName = name,
                    FileExtension = _defaultImageExtension,
                    VirtualFolderId = _selectedFolder.Id,
                    DateCreated = now,
                    DateUpdated = now,
                    IsActived = true,
                };

                attachmentRepository.Add(file);
                attachmentRepository.Commit();

                base_AttachmentModel newFile = new base_AttachmentModel(file)
                {
                    ParentFolder = _selectedFolder,
                    EditAttachmentProvider = _editAttachmentProvider,
                    IsNew = false,
                    IsDirty = false
                };

                _selectedFolder.FileCollection.Add(newFile);
                _selectedFolder.FileTotal++;
                _selectedFolder.DetermineIsCheckedAllFiles();

                Open(newFile);
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #endregion

        #region Override Methods

        #region LoadData

        public override void LoadData()
        {
            // Gets all folders.
            FolderCollection = new CollectionBase<base_VirtualFolderModel>();
            Initialize();
            _folderBackgroundWorker.RunWorkerAsync();
        }

        #endregion

        #region OnViewChangingCommandCanExecute

        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            if (_selectedFolder != null && _selectedFolder.FileCollection != null)
            {
                ListCollectionView fileCollectionView = CollectionViewSource.GetDefaultView(_selectedFolder.FileCollection) as ListCollectionView;
                if (fileCollectionView != null && fileCollectionView.IsEditingItem)
                {
                    (fileCollectionView.CurrentEditItem as base_AttachmentModel).EndEdit();
                    fileCollectionView.CommitEdit();
                }
            }
            return true;
        }

        #endregion

        #endregion

        #region BackgroundWorker Events

        #region FolderBackgroundWorker

        private void FolderWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;
            GetFolders();
        }

        private void FolderWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GetFolder(e.UserState as base_VirtualFolder);
        }

        private void FolderWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
        }

        #endregion

        #region FileBackgroundWorker

        private void FileWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;
            GetFiles();
        }

        private void FileWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GetFile(e.UserState as base_Attachment);
        }

        private void FileWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
        }

        #endregion

        #region FileSearchBackgroundWorker

        private void FileSearchWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;
            Search();
        }

        private void FileSearchWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GetFileSearch(e.UserState as base_Attachment);
        }

        private void FileSearchWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
        }

        #endregion

        #endregion

        #region Events

        #region TransferReady

        private void TransferReady(WpfTwain sender, List<ImageSource> imageSources)
        {
            foreach (ImageSource image in imageSources)
            {
                SaveFileScan(image);
            }

            _selectedFolder.IsDirty = false;
        }

        #endregion

        #endregion

        #region Class EditFolderProvider

        private class EditFolderProvider : IEditableFolder
        {
            #region IFolderProvider Members

            /// <summary>
            /// Update current folder.
            /// </summary>
            /// <param name="currentFolder">Current folder to update.</param>
            void IEditableFolder.Update(base_VirtualFolderModel currentFolder)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(currentFolder.FolderName))
                    {
                        // Get old name.
                        currentFolder.FolderName = currentFolder.base_VirtualFolder.FolderName;
                    }
                    else
                    {
                        currentFolder.FolderName = currentFolder.FolderName.Trim();

                        // Update when difference current name.
                        if (string.Compare(currentFolder.FolderName, currentFolder.base_VirtualFolder.FolderName, false) != 0)
                        {
                            base_VirtualFolderRepository virtualFolderRepository = new base_VirtualFolderRepository();

                            // Check duplicate name.
                            bool isExist = virtualFolderRepository.Get(x =>
                                x.Id != currentFolder.Id &&
                                x.ParentFolderId == currentFolder.ParentFolderId &&
                                x.FolderName.ToLower() == currentFolder.FolderName.ToLower()) != null;

                            if (isExist)
                            {
                                // Get old name.
                                currentFolder.FolderName = currentFolder.base_VirtualFolder.FolderName;
                                MessageBox.Show(string.Format("Cannot rename {0}. A folder with the name you specified already exists.", currentFolder.FolderName));
                            }
                            else
                            {
                                // Update.
                                currentFolder.DateUpdated = DateTime.Now;
                                currentFolder.ToEntity();
                                virtualFolderRepository.Commit();
                            }
                        }
                    }

                    currentFolder.IsDirty = false;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }

            #endregion
        }

        #endregion

        #region Class EditAttachmentProvider

        private class EditAttachmentProvider : IEditableAttachment
        {
            #region IEditableAttachment Members

            /// <summary>
            /// Update current attachment.
            /// </summary>
            /// <param name="currentAttachment">Current attachment to update.</param>
            void IEditableAttachment.Update(base_AttachmentModel currentAttachment)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(currentAttachment.FileName))
                    {
                        // Get old name.
                        currentAttachment.FileName = currentAttachment.base_Attachment.FileName;
                    }
                    else
                    {
                        currentAttachment.FileName = currentAttachment.FileName.Trim();

                        // Update when difference current name.
                        if (string.Compare(currentAttachment.FileName, currentAttachment.base_Attachment.FileName, false) != 0)
                        {
                            base_AttachmentRepository attachmentRepository = new base_AttachmentRepository();

                            // Check duplicate name.
                            bool isExist = attachmentRepository.Get(x =>
                                x.Id != currentAttachment.Id &&
                                x.VirtualFolderId == currentAttachment.VirtualFolderId &&
                                x.FileName.ToLower() == currentAttachment.FileName.ToLower()) != null;

                            if (isExist)
                            {
                                // Get old name.
                                currentAttachment.FileName = currentAttachment.base_Attachment.FileName;
                                MessageBox.Show(string.Format("Cannot rename {0}. A file with the name you specified already exists.", currentAttachment.FileName));
                            }
                            else
                            {
                                // Update.
                                currentAttachment.DateUpdated = DateTime.Now;
                                currentAttachment.ToEntity();
                                attachmentRepository.Commit();
                            }
                        }
                    }

                    currentAttachment.IsDirty = false;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }

            #endregion
        }

        #endregion
    }
}
