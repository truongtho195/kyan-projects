using System.Collections.Generic;
using System.Linq;
using System.Waf.Applications;
using FlashCard.Model;
using System.Windows.Input;
using MVVMHelper.Commands;
using FlashCard.DataAccess;
using System.Collections.ObjectModel;
using FlashCard.Views;
using MVVMHelper.Common;
using System;
using log4net;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;

namespace FlashCard.ViewModels
{
    public class LessonViewModel : ViewModel<LessonManageView>
    {
        #region Constructors
        public LessonViewModel(LessonManageView view)
            : base(view)
        {
            Initialize();
            this.Titles = "Lesson Management";
        }


        public LessonViewModel(LessonManageView view, bool isFromPopup)
            : this(view)
        {
            IsFromPopup = isFromPopup;

        }
        #endregion

        #region Variables
        StudyConfigView _studyConfigView;
        private ICollectionView _lessonCollectionView;
        private ICollectionView _categoryCollectionView;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Properties
        #region For Views
        private string _titles;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Titles
        {
            get
            {
                return _titles;
            }
            set
            {
                if (_titles != value)
                {
                    _titles = value;
                    RaisePropertyChanged(() => Titles);
                }
            }
        }

        #endregion

        //Lesson Region
        #region"  SelectedLesson"
        private LessonModel _selectedLesson;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public LessonModel SelectedLesson
        {
            get { return _selectedLesson; }
            set
            {
                if (_selectedLesson != value)
                {
                    _selectedLesson = value;
                    RaisePropertyChanged(() => SelectedLesson);
                }
            }
        }
        #endregion

        #region"  LessonCollection"
        private ObservableCollection<LessonModel> _lessonCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<LessonModel> LessonCollection
        {
            get { return _lessonCollection; }
            set
            {
                if (_lessonCollection != value)
                {
                    _lessonCollection = value;
                    RaisePropertyChanged(() => LessonCollection);
                }
            }
        }
        #endregion

        #region"  LessonTypeCollection"
        private List<TypeModel> _lessonTypeCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public List<TypeModel> LessonTypeCollection
        {
            get { return _lessonTypeCollection; }
            set
            {
                if (_lessonTypeCollection != value)
                {
                    _lessonTypeCollection = value;
                    RaisePropertyChanged(() => LessonTypeCollection);
                }
            }
        }
        #endregion

        //Category Region
        #region "  SelectedCategory"
        private CategoryModel _selectedCategory;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CategoryModel SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;

                    RaisePropertyChanged(() => SelectedCategory);
                }
            }
        }
        #endregion

        #region"  CategoryCollection"
        private ObservableCollection<CategoryModel> _categoryCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CategoryModel> CategoryCollection
        {
            get { return _categoryCollection; }
            set
            {
                if (_categoryCollection != value)
                {
                    _categoryCollection = value;
                    RaisePropertyChanged(() => CategoryCollection);
                }
            }
        }
        #endregion


        #region CategoryList
        private List<CategoryModel> _categoryList;
        /// <summary>
        /// Gets or sets the CategoryList.
        /// </summary>
        public List<CategoryModel> CategoryList
        {
            get { return _categoryList; }
            set
            {
                if (_categoryList != value)
                {
                    _categoryList = value;
                    RaisePropertyChanged(() => CategoryList);
                }
            }
        }
        #endregion
        
        public bool IsFromPopup { get; set; }

        #region"  IsCategoryHandle"
        private bool _isCategoryHandle;
        /// <summary>
        /// Gets or sets the IsCategoryHandle.
        /// </summary>
        public bool IsCategoryHandle
        {
            get { return _isCategoryHandle; }
            set
            {
                if (_isCategoryHandle != value)
                {
                    _isCategoryHandle = value;
                    RaisePropertyChanged(() => IsCategoryHandle);
                }
            }
        }
        #endregion

        #endregion

        #region Commands

        //Lesson Command
        #region "  NewCommand"
        private ICommand _newCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new RelayCommand(this.NewExecute, this.CanNewExecute);

                }
                return _newCommand;
            }
        }

        private bool CanNewExecute(object param)
        {
            //var edit = LessonCollection.Count(x => x.IsEdit);
            //var ne = LessonCollection.Count(x => x.IsNew);

            return LessonCollection!=null &&  (LessonCollection.Count(x => x.IsEdit) == 0 || LessonCollection.Count(x => x.IsNew)==0) && (SelectedLesson!=null && !SelectedLesson.IsNew);
        }

        private void NewExecute(object param)
        {
            SelectedLesson = new LessonModel();
            SelectedLesson.TypeID = LessonTypeCollection.First().TypeID;
            SelectedLesson.BackSideCollection = new ObservableCollection<BackSideModel>();
            SelectedLesson.BackSideModel = new BackSideModel();
            SelectedLesson.BackSideModel.IsCorrect = false;
            SelectedLesson.CategoryModel = CategoryCollection.First();
            SelectedLesson.TypeModel = LessonTypeCollection.First();
            SelectedLesson.IsEdit = false;
            SelectedLesson.IsNew = true;
            SelectedLesson.IsDelete = false;
            SelectedLesson.IsEditing = true;
        }
        #endregion

        #region "  SaveCommand"
        private ICommand _saveCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(this.SaveExecute, this.CanSaveExecute);
                }
                return _saveCommand;
            }
        }

        private bool CanSaveExecute(object param)
        {
            if (SelectedLesson == null)
                return false;
            return SelectedLesson.IsEdit && SelectedLesson.Errors.Count == 0 || (SelectedLesson.BackSideCollection != null && SelectedLesson.BackSideCollection.Count(x => x.IsEdit) > 0);
        }

        private void SaveExecute(object param)
        {
            LessonDataAccess lessonDataAccess = new LessonDataAccess();
            switch (SelectedLesson.TypeModel.TypeOf)
            {
                case 1:// Is A list
                    if (SelectedLesson.BackSideCollection == null)
                        SelectedLesson.BackSideCollection = new ObservableCollection<BackSideModel>();
                    SelectedLesson.BackSideCollection.Clear();
                    SelectedLesson.BackSideModel.LessonID = SelectedLesson.LessonID;
                    if (SelectedLesson.BackSideModel.BackSideID == 0)
                        SelectedLesson.BackSideModel.IsNew = true;
                    SelectedLesson.BackSideCollection.Add(SelectedLesson.BackSideModel);
                    break;
            }

            if (SelectedLesson.IsNew)
            {
                lessonDataAccess.Insert(SelectedLesson);
                LessonCollection.Add(SelectedLesson);
            }
            else
            {
                lessonDataAccess.Update(SelectedLesson);
            }

            if (SelectedLesson.IsNewCate)
            {
                SelectedLesson.IsNewCate = false;
                CategoryCollection.Add(SelectedLesson.CategoryModel);
            }

            SelectedLesson.IsEditing = false;
        }
        #endregion

        #region "  AddBackSideCommand"
        private ICommand _addBackSideCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand AddBackSideCommand
        {
            get
            {
                if (_addBackSideCommand == null)
                {
                    _addBackSideCommand = new RelayCommand(this.AddBackSideExecute, this.CanAddBackSideExecute);
                }
                return _addBackSideCommand;
            }
        }

        private bool CanAddBackSideExecute(object param)
        {
            return true;
        }

        private void AddBackSideExecute(object param)
        {
            if (this.SelectedLesson.BackSideCollection == null)
                this.SelectedLesson.BackSideCollection = new ObservableCollection<BackSideModel>();
            this.SelectedLesson.BackSideModel = new BackSideModel() { IsCorrect = false, IsNew = true };
            this.SelectedLesson.BackSideCollection.Add(this.SelectedLesson.BackSideModel); ;
        }
        #endregion

        #region "  CloseCommand"
        /// <summary>
        /// Gets the Close Command.
        /// <summary>
        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(this.OnCloseExecute, this.OnCloseCanExecute);
                return _closeCommand;
            }
        }

        /// <summary>
        /// Method to check whether the Close command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCloseCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the Close command is executed.
        /// </summary>
        private void OnCloseExecute(object param)
        {
            ViewCore.Close();
        }
        #endregion

        #region "  NewTypeCommand"
        /// <summary>
        /// Gets the NewType Command.
        /// <summary>
        private ICommand _newTypeCommand;
        public ICommand NewTypeCommand
        {
            get
            {
                if (_newTypeCommand == null)
                    _newTypeCommand = new RelayCommand(this.OnNewTypeExecute, this.OnNewTypeCanExecute);
                return _newTypeCommand;
            }
        }

        /// <summary>
        /// Method to check whether the NewType command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewTypeCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewType command is executed.
        /// </summary>
        private void OnNewTypeExecute(object param)
        {
            if ("Add".Equals(param.ToString()))
            {
                SelectedLesson.IsNewType = true;
                SelectedLesson.TypeModel = new TypeModel();
                SelectedLesson.TypeModel.IsNew = true;
                SelectedLesson.TypeModel.TypeID = -1;
            }
            else
            {
                SelectedLesson.IsNewType = false;
                SelectedLesson.TypeModel = LessonTypeCollection.First();
                SelectedLesson.TypeModel.IsNew = false;
            }
            RaisePropertyChanged(() => SelectedLesson);
        }

        #endregion

        #region "  SelectionChangedCommand"
        /// <summary>
        /// Gets the SelectionChanged Command.
        /// <summary>
        private ICommand _selectionChangedCommand;
        public ICommand SelectionChangedCommand
        {
            get
            {
                if (_selectionChangedCommand == null)
                    _selectionChangedCommand = new RelayCommand(this.OnSelectionChangedExecute, this.OnSelectionChangedCanExecute);
                return _selectionChangedCommand;
            }
        }

        /// <summary>
        /// Method to check whether the SelectionChanged command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSelectionChangedCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the SelectionChanged command is executed.
        /// </summary>
        private void OnSelectionChangedExecute(object param)
        {

            try
            {
                if (SelectedLesson != null && SelectedLesson.IsNew)
                {
                    LessonCollection.Remove(SelectedLesson);
                }
                else if (SelectedLesson != null && SelectedLesson.IsEdit)
                {
                    LessonDataAccess lessonDataAccess = new LessonDataAccess();
                    var lessonModel = lessonDataAccess.GetItem(SelectedLesson.LessonID);

                    var lessonIndex = LessonCollection.IndexOf(SelectedLesson);
                    if (lessonIndex > -1)
                    {
                        LessonCollection.RemoveAt(lessonIndex);
                        LessonCollection.Insert(lessonIndex, lessonModel);
                    }
                    RaisePropertyChanged(() => LessonCollection);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }



            SelectedLesson = param as LessonModel;
            if (SelectedLesson.BackSideModel == null)
            {
                SelectedLesson.BackSideModel = new BackSideModel();
                RaisePropertyChanged(() => SelectedLesson);
            }
        }

        #endregion

        #region"  SearchLesson"

        /// <summary>
        /// Gets the SearchLesson Command.
        /// <summary>
        private ICommand _searchLessonCommand;
        public ICommand SearchLessonCommand
        {
            get
            {
                if (_searchLessonCommand == null)
                    _searchLessonCommand = new RelayCommand(this.OnSearchLessonExecute, this.OnSearchLessonCanExecute);
                return _searchLessonCommand;
            }
        }

        /// <summary>
        /// Method to check whether the SearchLesson command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchLessonCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SearchLesson command is executed.
        /// </summary>
        private void OnSearchLessonExecute(object param)
        {
            _lessonCollectionView = CollectionViewSource.GetDefaultView(this.LessonCollection);
            string keywordLesson = string.Empty;

            try
            {
                this._lessonCollectionView.Filter = (item) =>
                {
                    var lessonModel = item as LessonModel;
                    if (lessonModel == null)
                        return false;

                    if (param == null)
                        return false;
                    else
                        keywordLesson = param.ToString();

                    if (string.IsNullOrWhiteSpace(keywordLesson))
                        return true;
                    else
                    {
                        if (lessonModel.LessonName.ToLower().Contains(keywordLesson.TrimStart().ToLower()) || lessonModel.CategoryModel.CategoryName.ToLower().Contains(keywordLesson.TrimStart().ToLower()))
                            return true;
                    }
                    return false;
                };
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }
        #endregion

        #region"  DeleteLessonCommand"

        /// <summary>
        /// Gets the DeleteLesson Command.
        /// <summary>
        private ICommand _deleteLessonCommand;
        public ICommand DeleteLessonCommand
        {
            get
            {
                if (_deleteLessonCommand == null)
                    _deleteLessonCommand = new RelayCommand(this.OnDeleteLessonExecute, this.OnDeleteLessonCanExecute);
                return _deleteLessonCommand;
            }
        }

        /// <summary>
        /// Method to check whether the DeleteLesson command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteLessonCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteLesson command is executed.
        /// </summary>
        private void OnDeleteLessonExecute(object param)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to remove this Lesson", "Remove Lesson", MessageBoxButton.YesNo);
            if (result.Equals(MessageBoxResult.Yes))
            {
                LessonModel lessonModel;
                if (param != null)
                {
                    lessonModel = param as LessonModel;
                    LessonDataAccess lessonDA = new LessonDataAccess();
                    lessonDA.Delete(lessonModel);
                    LessonCollection.Remove(lessonModel);
                }
            }
        }
        #endregion

        //Category Command
        #region "  NewCategoryCommand"
        /// <summary>
        /// Gets the NewCategory Command.
        /// <summary>
        private ICommand _newCategoryCommand;
        public ICommand NewCategoryCommand
        {
            get
            {
                if (_newCategoryCommand == null)
                    _newCategoryCommand = new RelayCommand(this.OnNewCategoryExecute, this.OnNewCategoryCanExecute);
                return _newCategoryCommand;
            }
        }

        /// <summary>
        /// Method to check whether the NewCategory command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCategoryCanExecute(object param)
        {
            if (CategoryCollection == null)
                return true;
            return SelectedCategory != null && !SelectedCategory.IsNew;
        }

        /// <summary>
        /// Method to invoke when the NewCategory command is executed.
        /// </summary>
        private void OnNewCategoryExecute(object param)
        {
            SelectedCategory = new CategoryModel();
            SelectedCategory.IsNew = true;
            SelectedCategory.CategoryID = -2;
        }
        #endregion

        #region"  SaveCategoryCommand"
        private ICommand _saveCategoryCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand SaveCategoryCommand
        {
            get
            {
                if (_saveCategoryCommand == null)
                {
                    _saveCategoryCommand = new RelayCommand(this.SaveCategoryExecute, this.CanSaveCategoryExecute);
                }
                return _saveCategoryCommand;
            }
        }

        private bool CanSaveCategoryExecute(object param)
        {
            if (SelectedCategory == null)
                return false;

            return SelectedCategory.IsEdit;
        }

        private void SaveCategoryExecute(object param)
        {
            try
            {
                CategoryDataAccess categoryDataAccess = new CategoryDataAccess();
                if (SelectedCategory.IsNew)
                {
                    categoryDataAccess.Insert(SelectedCategory);
                    CategoryCollection.Add(SelectedCategory);
                    CategoryList.Add(SelectedCategory);

                }
                else
                {
                    categoryDataAccess.Update(SelectedCategory);
                }
                RaisePropertyChanged(() => CategoryList);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }

        }
        #endregion

        #region "  SelectionCategoryChangedCommand"
        /// <summary>
        /// Gets the SelectionChanged Command.
        /// <summary>
        private ICommand _selectionCategoryChangedCommand;
        public ICommand SelectionCategoryChangedCommand
        {
            get
            {
                if (_selectionCategoryChangedCommand == null)
                    _selectionCategoryChangedCommand = new RelayCommand(this.OnSelectionCategoryChangedExecute, this.OnSelectionCategoryChangedCanExecute);
                return _selectionCategoryChangedCommand;
            }
        }

        /// <summary>
        /// Method to check whether the SelectionChanged command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSelectionCategoryChangedCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the SelectionChanged command is executed.
        /// </summary>
        private void OnSelectionCategoryChangedExecute(object param)
        {
            try
            {
                if (SelectedCategory != null && SelectedCategory.IsNew)
                {
                    CategoryCollection.Remove(SelectedCategory);
                    CategoryList.Remove(SelectedCategory);
                    RaisePropertyChanged(() => CategoryList);
                }
                else if (SelectedCategory != null && SelectedCategory.IsEdit)
                {
                    CategoryDataAccess categoryDataAccess = new CategoryDataAccess();
                    var cateModel = categoryDataAccess.Get(SelectedCategory.CategoryID);
                    //reset data
                    SelectedCategory = cateModel;
                }
                SelectedCategory = param as CategoryModel;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }

        }

        #endregion

        #region"  SearchCategory"

        /// <summary>
        /// Gets the SearchCategory Command.
        /// <summary>
        private ICommand _searchCategoryCommand;
        public ICommand SearchCategoryCommand
        {
            get
            {
                if (_searchCategoryCommand == null)
                    _searchCategoryCommand = new RelayCommand(this.OnSearchCategoryExecute, this.OnSearchCategoryCanExecute);
                return _searchCategoryCommand;
            }
        }

        /// <summary>
        /// Method to check whether the SearchCategory command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchCategoryCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SearchCategory command is executed.
        /// </summary>
        private void OnSearchCategoryExecute(object param)
        {
            _categoryCollectionView = CollectionViewSource.GetDefaultView(this.CategoryCollection);
            string keywordCategory = string.Empty;

            try
            {
                this._categoryCollectionView.Filter = (item) =>
                {
                    var categoryModel = item as CategoryModel;
                    if (categoryModel == null)
                        return false;

                    if (param == null)
                        return false;
                    else
                        keywordCategory = param.ToString();

                    if (string.IsNullOrWhiteSpace(keywordCategory))
                        return true;
                    else
                    {
                        if (categoryModel.CategoryName.ToLower().Contains(keywordCategory.TrimStart().ToLower()))
                            return true;
                    }
                    return false;
                };
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        #endregion

        #region"  DeleteCategoryCommand"

        /// <summary>
        /// Gets the DeleteCategory Command.
        /// <summary>
        private ICommand _deleteCategoryCommand;
        public ICommand DeleteCategoryCommand
        {
            get
            {
                if (_deleteCategoryCommand == null)
                    _deleteCategoryCommand = new RelayCommand(this.OnDeleteCategoryExecute, this.OnDeleteCategoryCanExecute);
                return _deleteCategoryCommand;
            }
        }

        /// <summary>
        /// Method to check whether the DeleteCategory command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCategoryCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteCategory command is executed.
        /// </summary>
        private void OnDeleteCategoryExecute(object param)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to remove this Category & Lesson relation", "Remove Category", MessageBoxButton.YesNo);
            if (result.Equals(MessageBoxResult.Yes))
            {
                CategoryModel cateModel;
                if (param != null)
                {
                    cateModel = param as CategoryModel;
                    CategoryDataAccess cateDataAccess = new CategoryDataAccess();
                    var resultDel = cateDataAccess.DeleteWithRelation(cateModel);
                    if (resultDel)
                    {
                        foreach (var item in LessonCollection.Where(x => x.CategoryID.Equals(cateModel.CategoryID)).ToList())
                        {
                            if (item != null)
                            {
                                LessonCollection.Remove(item);
                                if (item == SelectedLesson)
                                    SelectedLesson = LessonCollection.First();
                            }

                        }
                        CategoryCollection.Remove(cateModel);
                        if (cateModel == SelectedCategory)
                            SelectedCategory = CategoryCollection.First();

                        CategoryList.Remove(cateModel);
                    }
                }
            }
        }
        #endregion

        //Study Command
        #region"  ShowStudyCommand"
        private ICommand _showStudyCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand ShowStudyCommand
        {
            get
            {
                if (_showStudyCommand == null)
                {
                    _showStudyCommand = new RelayCommand(this.ShowStudyExecute, this.CanShowStudyExecute);
                }
                return _showStudyCommand;
            }
        }

        private bool CanShowStudyExecute(object param)
        {
            return true;
        }

        private void ShowStudyExecute(object param)
        {
            try
            {
                if (IsCategoryHandle)
                {
                    if (SelectedCategory != null && SelectedCategory.IsNew)
                    {
                        CategoryCollection.Remove(SelectedCategory);
                    }
                    else if (SelectedCategory != null && SelectedCategory.IsEdit)
                    {
                        CategoryDataAccess categoryDataAccess = new CategoryDataAccess();
                        var cateModel = categoryDataAccess.Get(SelectedCategory.CategoryID);
                        //reset data
                        SelectedCategory = cateModel;
                    }
                }
                else
                {
                    if (SelectedLesson != null && SelectedLesson.IsNew)
                    {
                        LessonCollection.Remove(SelectedLesson);
                    }
                    else if (SelectedLesson != null && SelectedLesson.IsEdit)
                    {
                        LessonDataAccess lessonDataAccess = new LessonDataAccess();
                        var lessonModel = lessonDataAccess.GetItem(SelectedLesson.LessonID);

                        var lessonIndex = LessonCollection.IndexOf(SelectedLesson);
                        if (lessonIndex > -1)
                        {
                            LessonCollection.RemoveAt(lessonIndex);
                            LessonCollection.Insert(lessonIndex, lessonModel);
                        }
                        RaisePropertyChanged(() => LessonCollection);
                    }
                }


                if (!ViewCore.grdControl.Children.Contains(_studyConfigView))
                {
                    _studyConfigView = new StudyConfigView();
                    var studyConfigViewModel = _studyConfigView.GetViewModel<StudyConfigViewModel>();
                    studyConfigViewModel.LessonCollection =  this.LessonCollection.ToList();
                    ViewCore.grdUserControl.Visibility = System.Windows.Visibility.Visible;
                    studyConfigViewModel.ButtonClickHandler += new StudyConfigViewModel.handlerControl(LessonViewModel_DoNow);
                    ViewCore.grdControl.Children.Add(_studyConfigView);
                }
                else
                {
                    ViewCore.grdUserControl.Visibility = System.Windows.Visibility.Collapsed;
                    ViewCore.grdControl.Children.Clear();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void LessonViewModel_DoNow(string message)
        {
            try
            {
                if ("OkExecute".Equals(message))
                {
                    MainWindow mainWindow = new MainWindow();
                    
                    var lessonCollection = _studyConfigView.GetViewModel<StudyConfigViewModel>().LessonCollection;
                    var mainViewModel = mainWindow.GetViewModel<MainViewModel>();
                    mainViewModel.GetLesson(lessonCollection.ToList());
                    mainViewModel.ExcuteMainForm();
                    ViewCore.grdUserControl.Visibility = System.Windows.Visibility.Collapsed;
                    ViewCore.grdControl.Children.Clear();
                    App.LessonMangeView.Hide();
                }
                else
                {
                    ViewCore.grdUserControl.Visibility = System.Windows.Visibility.Collapsed;
                    ViewCore.grdControl.Children.Clear();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }
        #endregion

        #region"  ShortcutKeyNewItemCommand"
        /// <summary>
        /// Gets the ShortcutKeyNewItem Command.
        /// <summary>
        private ICommand _shortcutKeyNewItemCommand;
        public ICommand ShortcutKeyNewItemCommand
        {
            get
            {
                if (_shortcutKeyNewItemCommand == null)
                    _shortcutKeyNewItemCommand = new RelayCommand(this.OnShortcutKeyNewItemExecute, this.OnShortcutKeyNewItemCanExecute);
                return _shortcutKeyNewItemCommand;
            }
        }

        /// <summary>
        /// Method to check whether the ShortcutKeyNewItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnShortcutKeyNewItemCanExecute(object param)
        {
            if (IsCategoryHandle)
                return OnNewCategoryCanExecute(param);
            else
                return CanNewExecute(param);
        }

        /// <summary>
        /// Method to invoke when the ShortcutKeyNewItem command is executed.
        /// </summary>
        private void OnShortcutKeyNewItemExecute(object param)
        {
            try
            {
                if (IsCategoryHandle)
                {
                    if (OnNewCategoryCanExecute(null))
                    {
                        OnNewCategoryExecute(null);
                    }
                }
                else
                {
                    if (CanNewExecute(null))
                    {
                        NewExecute(null);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }

        }
        #endregion

        #region"  ShortCutKeySaveItemCommand"

        /// <summary>
        /// Gets the ShortCutKeySaveItem Command.
        /// <summary>
        private ICommand _shortcutKeySaveItemCommand;
        public ICommand ShortcutKeySaveItemCommand
        {
            get
            {
                if (_shortcutKeySaveItemCommand == null)
                    _shortcutKeySaveItemCommand = new RelayCommand(this.OnShortCutKeySaveItemExecute, this.OnShortCutKeySaveItemCanExecute);
                return _shortcutKeySaveItemCommand;
            }
        }

        /// <summary>
        /// Method to check whether the ShortCutKeySaveItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnShortCutKeySaveItemCanExecute(object param)
        {
            if (IsCategoryHandle)
                return CanSaveCategoryExecute(param);
            else
                return CanSaveExecute(param);
        }

        /// <summary>
        /// Method to invoke when the ShortCutKeySaveItem command is executed.
        /// </summary>
        private void OnShortCutKeySaveItemExecute(object param)
        {
            try
            {
                if (IsCategoryHandle)
                {
                    if (CanSaveCategoryExecute(param))
                    {
                        SaveCategoryExecute(param);
                    }
                }
                else
                {
                    if (CanSaveExecute(param))
                    {
                        SaveExecute(param);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }

        }
        #endregion

        #endregion

        #region Methods
        private void Initialize()
        {
            try
            {
                TypeDataAccess typeDataAccess = new TypeDataAccess();
                LessonTypeCollection = new List<TypeModel>(typeDataAccess.GetAll());

                CategoryDataAccess categoryDataAccess = new CategoryDataAccess();
                CategoryCollection = new ObservableCollection<CategoryModel>(categoryDataAccess.GetAll());

                CategoryList = CategoryCollection.ToList();

                LessonDataAccess lessonDataAccess = new LessonDataAccess();
                LessonCollection = new ObservableCollection<LessonModel>(lessonDataAccess.GetAllWithRelation());

                if (LessonCollection.Any())
                    SelectedLesson = LessonCollection.FirstOrDefault();
                else
                {
                    LessonCollection = new ObservableCollection<LessonModel>();
                    NewExecute(null);
                }

                if (CategoryCollection.Any())
                    SelectedCategory = CategoryCollection.FirstOrDefault();
                else
                {
                    CategoryCollection = new ObservableCollection<CategoryModel>();
                    OnNewCategoryExecute(null);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }
        #endregion
    }
}
