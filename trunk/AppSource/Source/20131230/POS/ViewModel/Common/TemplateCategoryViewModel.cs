using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Interfaces;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Service.FrameworkDialogs.OpenFile;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class TemplateCategoryViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// Used for update folder.
        /// </summary>
        private EditFolderProvider _editFolderProvider = new EditFolderProvider();

        /// <summary>
        /// Used for update attachment.
        /// </summary>
        private EditAttachmentProvider _editAttachmentProvider = new EditAttachmentProvider();

        /// <summary>
        /// Path used for saves images.
        /// </summary>
        private readonly string _imagePathRoot = Define.CONFIGURATION.DefautlImagePath;

        /// <summary>
        /// Default new folder name.
        /// </summary>
        private const string _defaulFolderName = "New Category";

        /// <summary>
        /// Default root folder name.
        /// </summary>
        private const string _defaulRootFolderName = "All Categories";

        /// <summary>
        /// Iddentity folder in TemplateCategoryView.
        /// </summary>
        private readonly string _id = FolderIn.TemplateCategory.ToDescription();

        /// <summary>
        /// Used for Filter property of OpenFileDialogViewModel.
        /// </summary>
        private readonly string _filter = "Image Files |*.jpg; *.jpeg; *.bmp; *.gif; *.png; *.tif";

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

        public TemplateCategoryViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            Initialize();
        }

        #endregion

        #region Properties

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

        #region SelectedImage

        public string SelectedImage = null;

        #endregion

        #endregion

        #region Command Properties

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

        #region SelectImageCommand

        private ICommand _selectImageCommand;
        public ICommand SelectImageCommand
        {
            get
            {
                if (_selectImageCommand == null)
                {
                    _selectImageCommand = new RelayCommand<base_AttachmentModel>(SelectImageExecute);
                }
                return _selectImageCommand;
            }
        }

        #endregion

        #region CancelCommand

        private ICommand _cancelCommand;
        /// <summary>
        /// Cancel.
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(CancelExecute);
                }
                return _cancelCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

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
            if (_selectedFolder == null || !_selectedFolder.IsRoot)
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
            EditFolder();
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
            EndEditFolder();
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
            if (Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.DeleteItems, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DeleteFolder();
            }
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
            if (_selectedFolder == null || _selectedFolder.IsRoot || _selectedFolder.FileCollection == null)
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
            if (Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.DeleteItems, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DeleteFiles();
            }
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

        #region SelectImageExecute

        private void SelectImageExecute(base_AttachmentModel attachment)
        {
            SelectImage(attachment);
        }

        #endregion

        #region CancelExecute

        /// <summary>
        /// Cancel.
        /// </summary>
        private void CancelExecute()
        {
            Cancel();
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
                    GetFiles();
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region Initialize

        /// <summary>
        /// Initialize
        /// </summary>
        private void Initialize()
        {
            CreateDefaultFolder();
            GetFolders();
            SelectDefaultFolder();
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            Close(false);
        }

        #endregion

        #region Close

        /// <summary>
        /// Close popup.
        /// </summary>
        private void Close(bool result)
        {
            FindOwnerWindow(this).DialogResult = result;
        }

        #endregion

        #region CreateDefaultFolder

        /// <summary>
        /// Create default folder.
        /// </summary>
        private void CreateDefaultFolder()
        {
            try
            {
                // Creates image folder.
                if (!Directory.Exists(_imagePathRoot))
                {
                    Directory.CreateDirectory(_imagePathRoot);
                }

                // Create root folder.
                base_VirtualFolderRepository virtualFolderRepository = new base_VirtualFolderRepository();
                base_VirtualFolder rootFolder = virtualFolderRepository.Get(x => x.ParentFolderId == null && x.IsActived && x.Code == _id);
                if (rootFolder == null)
                {
                    // Insert root folder.
                    DateTime now = DateTime.Now;
                    base_VirtualFolder folder = new base_VirtualFolder
                    {
                        ParentFolderId = null,
                        FolderName = _defaulRootFolderName,
                        Code = _id,
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
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                // Get root folder.
                base_VirtualFolderRepository virtualFolderRepository = new base_VirtualFolderRepository();
                FolderCollection = new CollectionBase<base_VirtualFolderModel>();
                base_VirtualFolder rootFolder = virtualFolderRepository.Get(x => x.ParentFolderId == null && x.IsActived && x.Code == _id);
                if (rootFolder != null && virtualFolderRepository.Refresh(rootFolder) != null)
                {
                    GetFolder(rootFolder);
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
            virtualFolderRepository.Refresh(folder.base_VirtualFolder1);
            List<base_VirtualFolder> activeFolders = folder.base_VirtualFolder1.Where(x => x.IsActived).ToList();
            foreach (base_VirtualFolder childFolder in activeFolders)
            {
                // Add current folder to folder collection of parent.
                rootFolder.FolderCollection.Add(new base_VirtualFolderModel(childFolder)
                {
                    ParentFolder = rootFolder,
                    EditFolderProvider = _editFolderProvider,
                    IsNew = false,
                    IsDirty = false
                });
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
                base_AttachmentRepository attachmentRepository = new base_AttachmentRepository();

                _selectedFolder.FileCollection = new CollectionBase<base_AttachmentModel>();
                _selectedFolder.IsDirty = false;

                // Get range of files.
                IList<base_Attachment> files = attachmentRepository.GetAll(x => x.VirtualFolderId == _selectedFolder.Id && x.IsActived);
                foreach (base_Attachment file in files)
                {
                    if (attachmentRepository.Refresh(file) != null)
                    {
                        GetFile(file);
                    }
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
            base_AttachmentModel newFile = new base_AttachmentModel(file)
            {
                ParentFolder = _selectedFolder,
                EditAttachmentProvider = _editAttachmentProvider,
                IsNew = false,
                IsDirty = false
            };

            _selectedFolder.FileCollection.Add(newFile);
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
                    Code = _id,
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
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region EditFolder

        private void EditFolder()
        {
            if (!_selectedFolder.IsEdit)
            {
                _selectedFolder.IsEdit = true;
            }
        }

        #endregion

        #region EndEditFolder

        /// <summary>
        /// End edit folder.
        /// </summary>
        private void EndEditFolder()
        {
            if (_selectedFolder.IsEdit)
            {
                _selectedFolder.IsEdit = false;
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
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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

                //_selectedFolder.DetermineIsCheckedAllFiles();
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
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            finally
            {
                //_selectedFolder.DetermineIsCheckedAllFiles();
                _selectedFolder.IsDirty = false;
            }
        }

        #endregion

        #region SelectImage

        /// <summary>
        /// Select image
        /// </summary>
        private void SelectImage(base_AttachmentModel attachment)
        {
            SelectedImage = attachment.FilePath;
            Close(true);
        }

        #endregion

        #region SelectDefaultFolder

        /// <summary>
        /// Select default folder
        /// </summary>
        private void SelectDefaultFolder()
        {
            if (_folderCollection != null)
            {
                base_VirtualFolderModel rootFolder = _folderCollection.First(x => x.IsRoot);
                if (rootFolder != null)
                {
                    base_VirtualFolderModel childFolder = rootFolder.FolderCollection.FirstOrDefault(x => !x.IsRoot);
                    if (childFolder != null)
                    {
                        childFolder.IsSelected = true;
                        SelectedFolder = childFolder;
                    }
                    else
                    {
                        rootFolder.IsSelected = true;
                        SelectedFolder = rootFolder;
                    }
                }
            }
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
                                Xceed.Wpf.Toolkit.MessageBox.Show(string.Format(Language.Text18, currentFolder.FolderName));
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
                    Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                                Xceed.Wpf.Toolkit.MessageBox.Show(string.Format(Language.Text19, currentAttachment.FileName));
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
                    Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }

            #endregion
        }

        #endregion
    }
}