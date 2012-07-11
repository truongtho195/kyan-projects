using System.Collections.Generic;
using System.Linq;
using System.Waf.Applications;
using FlashCard.Models;
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
using FlashCard.Database;

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
            private set 
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
                if (value != null)
                {
                    _selectedLesson = value;
                    CategoryID = SelectedLesson.Lesson.CategoryID;
                    CardID = SelectedLesson.Lesson.CardID;
                    LessonName = SelectedLesson.Lesson.LessonName;
                    Description = SelectedLesson.Lesson.Description;
                    BackSideCollection = new ObservableCollection<BackSideModel>(SelectedLesson.Lesson.BackSides.ToList().Select(x => new BackSideModel(x)));
                }
                else if(value == null)
                {
                    _selectedLesson = value;
                    CategoryID = "1";
                    CardID = "1";

                    LessonName = string.Empty;
                    Description = string.Empty;
                    BackSideCollection = new ObservableCollection<BackSideModel>();
                }
                RaisePropertyChanged(() => SelectedLesson);
            }
        }


        //Lesson Binding Form

        #region CategoryID
        private string _categoryID;
        /// <summary>
        /// Gets or sets the KindID.
        /// </summary>
        public string CategoryID
        {
            get { return _categoryID; }
            set
            {
                if (_categoryID != value)
                {
                    _categoryID = value;
                    IsLessonDirty = true;
                    RaisePropertyChanged(() => CategoryID);
                }
            }
        }
        #endregion
        
        #region CardID
        private string _cardID;
        /// <summary>
        /// Gets or sets the CategoryID.
        /// </summary>
        public string CardID
        {
            get { return _cardID; }
            set
            {
                if (_cardID != value)
                {
                    _cardID = value;
                    IsLessonDirty = true;
                    RaisePropertyChanged(() => CardID);
                }
            }
        }
        #endregion

        #region LessonName
        private string _lessonName;
        /// <summary>
        /// Gets or sets the LessonName.
        /// </summary>
        public string LessonName
        {
            get { return _lessonName; }
            set
            {
                if (_lessonName != value)
                {
                    _lessonName = value;
                    IsLessonDirty = true;
                    RaisePropertyChanged(() => LessonName);
                }
            }
        }
        #endregion

        #region Description
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
                    _description = value;
                    IsLessonDirty = true;
                    RaisePropertyChanged(() => Description);
                }
            }
        }
        #endregion

        #region BackSideCollection
        private ObservableCollection<BackSideModel> _backSideCollection;
        /// <summary>
        /// Gets or sets the BackSideCollection.
        /// </summary>
        public ObservableCollection<BackSideModel> BackSideCollection
        {
            get { return _backSideCollection; }
            set
            {
                if (_backSideCollection != value)
                {
                    _backSideCollection = value;
                    RaisePropertyChanged(() => BackSideCollection);
                }
            }
        }
        #endregion

        public bool IsLessonDirty { get; set; }

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

        #region"  CategoryCollection"
        private List<CategoryModel> _categoryCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public List<CategoryModel> CategoryCollection
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

        //SelectedCard Region
        #region "  SelectedCard"
        private CardModel _selectedCard;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CardModel SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                if (_selectedCard != value)
                {
                    _selectedCard = value;

                    RaisePropertyChanged(() => SelectedCard);
                }
            }
        }
        #endregion

        #region"  CategoryCollection"
        private ObservableCollection<CardModel> _cardCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CardModel> CardCollection
        {
            get { return _cardCollection; }
            set
            {
                if (_cardCollection != value)
                {
                    _cardCollection = value;
                    RaisePropertyChanged(() => CardCollection);
                }
            }
        }
        #endregion

        #region CategoryList
        private List<CardModel> _cardList;
        /// <summary>
        /// Gets or sets the CategoryList.
        /// </summary>
        public List<CardModel> CardList
        {
            get { return _cardList; }
            set
            {
                if (_cardList != value)
                {
                    _cardList = value;
                    RaisePropertyChanged(() => CardList);
                }
            }
        }
        #endregion
        
        public bool IsFromPopup { get; set; }

        #region"  IsCardHandle"
        private bool _isCardHandle;
        /// <summary>
        /// Gets or sets the IsCategoryHandle.
        /// </summary>
        public bool IsCardHandle
        {
            get { return _isCardHandle; }
            set
            {
                if (_isCardHandle != value)
                {
                    _isCardHandle = value;
                    RaisePropertyChanged(() => IsCardHandle);
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
            //var edit = LessonCollection.Count(x => x.IsDirty);
            //var ne = LessonCollection.Count(x => x.IsNew);

            return LessonCollection!=null &&  (LessonCollection.Count(x => x.IsDirty) == 0 || LessonCollection.Count(x => x.IsNew)==0) && (SelectedLesson!=null && !SelectedLesson.IsNew);
        }

        private void NewExecute(object param)
        {
            SelectedLesson = null;
            //!!!!  SelectedLesson.TypeID = LessonTypeCollection.First().TypeID;
            ///!!!!  SelectedLesson.BackSideCollection = new ObservableCollection<BackSideModel>();
            //SelectedLesson.Lesson.BackSides = new BackSideModel();
            //SelectedLesson.BackSideModel.IsCorrect = false;
            ///!!!! SelectedLesson.CategoryModel = CategoryCollection.First();
            ///!!!!  SelectedLesson.TypeModel = LessonTypeCollection.First();
            ///!!!! SelectedLesson.IsDirty = false;
            ///!!!! SelectedLesson.IsNew = true;
            ///!!!!   SelectedLesson.IsDelete = false;
            ///!!!!  SelectedLesson.IsDirtying = true;
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
            ///!!!!   return SelectedLesson.IsDirty && SelectedLesson.Errors.Count == 0 || (SelectedLesson.BackSideCollection != null && SelectedLesson.BackSideCollection.Count(x => x.IsDirty) > 0);
            return true;
        }

        private void SaveExecute(object param)
        {
            LessonDataAccess lessonDataAccess = new LessonDataAccess();
            ///!!!!
            //switch (SelectedLesson.TypeModel.TypeOf)
            //{
            //    case 1:// Is A list
            //        if (SelectedLesson.BackSideCollection == null)
            //            SelectedLesson.BackSideCollection = new ObservableCollection<BackSideModel>();
            //        SelectedLesson.BackSideCollection.Clear();
            //        SelectedLesson.BackSideModel.LessonID = SelectedLesson.LessonID;
            //        if (SelectedLesson.BackSideModel.BackSideID == 0)
            //            SelectedLesson.BackSideModel.IsNew = true;
            //        SelectedLesson.BackSideCollection.Add(SelectedLesson.BackSideModel);
            //        break;
            //}

            if (SelectedLesson.IsNew)
            {
                ///!!!! lessonDataAccess.Insert(SelectedLesson);
                LessonCollection.Add(SelectedLesson);
            }
            else
            {
                ///!!!!   lessonDataAccess.Update(SelectedLesson);
            }

            if (SelectedLesson.IsNewCate)
            {
                SelectedLesson.IsNewCate = false;
                ///!!!!     CategoryCollection.Add(SelectedLesson.CategoryModel);
            }

            ///!!!!     SelectedLesson.IsDirtying = false;
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
              if (BackSideCollection == null)
                  BackSideCollection = new ObservableCollection<BackSideModel>();
               BackSideCollection.Add(new BackSideModel()); 
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
                ///!!!!   SelectedLesson.TypeModel = new TypeModel();
                ///!!!!   SelectedLesson.TypeModel.IsNew = true;
                ///!!!!  SelectedLesson.TypeModel.TypeID = -1;
            }
            else
            {
                SelectedLesson.IsNewType = false;
                ///!!!!  SelectedLesson.TypeModel = LessonTypeCollection.First();
                ///!!!!  SelectedLesson.TypeModel.IsNew = false;
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
                //if (SelectedLesson != null && SelectedLesson.IsNew)
                //{
                //    LessonCollection.Remove(SelectedLesson);
                //}
                //else if (SelectedLesson != null && SelectedLesson.IsDirty)
                //{
                //    LessonDataAccess lessonDataAccess = new LessonDataAccess();
                //    ///!!!!       var lessonModel = lessonDataAccess.GetItem(SelectedLesson.LessonID);

                //    var lessonIndex = LessonCollection.IndexOf(SelectedLesson);
                //    if (lessonIndex > -1)
                //    {
                //        LessonCollection.RemoveAt(lessonIndex);
                //        ///!!!!   LessonCollection.Insert(lessonIndex, lessonModel);
                //    }
                //    RaisePropertyChanged(() => LessonCollection);
                //}

                SelectedLesson = param as LessonModel;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }



            //SelectedLesson = param as LessonModel;
            //if (SelectedLesson.BackSideModel == null)
            //{
            //    SelectedLesson.BackSideModel = new BackSideModel();
            //    RaisePropertyChanged(() => SelectedLesson);
            //}
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
                        ///!!!!
                        //if (lessonModel.LessonName.ToLower().Contains(keywordLesson.TrimStart().ToLower()) || lessonModel.CategoryModel.CategoryName.ToLower().Contains(keywordLesson.TrimStart().ToLower()))
                        //    return true;
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
                    ///!!!!  lessonDA.Delete(lessonModel);
                    LessonCollection.Remove(lessonModel);
                }
            }
        }
        #endregion

        //Category Command
        #region "  NewCardCommand"
        /// <summary>
        /// Gets the NewCategory Command.
        /// <summary>
        private ICommand _newCardCommand;
        public ICommand NewCardCommand
        {
            get
            {
                if (_newCardCommand == null)
                    _newCardCommand = new RelayCommand(this.OnNewCategoryExecute, this.OnNewCategoryCanExecute);
                return _newCardCommand;
            }
        }

        /// <summary>
        /// Method to check whether the NewCategory command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCategoryCanExecute(object param)
        {
            if (CardCollection == null)
                return true;
            return SelectedCard != null && !SelectedCard.IsNew;
        }

        /// <summary>
        /// Method to invoke when the NewCategory command is executed.
        /// </summary>
        private void OnNewCategoryExecute(object param)
        {
            SelectedCard = new CardModel();
            ///!!!!SelectedCategory.IsNew = true;
            //SelectedCategory.CategoryID = -2;
        }
        #endregion

        #region"  SaveCardCommand"
        private ICommand _saveCardCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand SaveCardCommand
        {
            get
            {
                if (_saveCardCommand == null)
                {
                    _saveCardCommand = new RelayCommand(this.SaveCategoryExecute, this.CanSaveCategoryExecute);
                }
                return _saveCardCommand;
            }
        }

        private bool CanSaveCategoryExecute(object param)
        {
            if (SelectedCard == null)
                return false;

            return SelectedCard.IsDirty;
        }

        private void SaveCategoryExecute(object param)
        {
            try
            {
                CategoryDataAccess categoryDataAccess = new CategoryDataAccess();
                if (SelectedCard.IsNew)
                {
                    ///!!!! categoryDataAccess.Insert(SelectedCategory);
                    CardCollection.Add(SelectedCard);
                    CardList.Add(SelectedCard);

                }
                else
                {
                    ///!!!!  categoryDataAccess.Update(SelectedCategory);
                }
                RaisePropertyChanged(() => CardList);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }

        }
        #endregion

        #region "  SelectionCardChangedCommand"
        /// <summary>
        /// Gets the SelectionChanged Command.
        /// <summary>
        private ICommand _selectionCardChangedCommand;
        public ICommand SelectionCardChangedCommand
        {
            get
            {
                if (_selectionCardChangedCommand == null)
                    _selectionCardChangedCommand = new RelayCommand(this.OnSelectionCategoryChangedExecute, this.OnSelectionCategoryChangedCanExecute);
                return _selectionCardChangedCommand;
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
                if (SelectedCard != null && SelectedCard.IsNew)
                {
                    CardCollection.Remove(SelectedCard);
                    CardList.Remove(SelectedCard);
                    RaisePropertyChanged(() => CardList);
                }
                else if (SelectedCard != null && SelectedCard.IsDirty)
                {
                    CategoryDataAccess categoryDataAccess = new CategoryDataAccess();
                    ///!!!!  var cateModel = categoryDataAccess.Get(SelectedCategory.CategoryID);
                    //reset data
                    ///!!!!   SelectedCategory = cateModel;
                }
                SelectedCard = param as CardModel;
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
            _categoryCollectionView = CollectionViewSource.GetDefaultView(this.CardCollection);
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
                CardModel cardModel;
                if (param != null)
                {
                    cardModel = param as CardModel;
                    CategoryDataAccess cateDataAccess = new CategoryDataAccess();
                    var resultDel = true;
                        ///!!!!cateDataAccess.DeleteWithRelation(cateModel);
                    if (resultDel)
                    {
                        foreach (var item in LessonCollection.Where(x => x.CategoryID.Equals(cardModel.CardID)).ToList())
                        {
                            if (item != null)
                            {
                                LessonCollection.Remove(item);
                                if (item == SelectedLesson)
                                    SelectedLesson = LessonCollection.First();
                            }

                        }
                        CardCollection.Remove(cardModel);
                        if (cardModel == SelectedCard)
                            SelectedCard = CardCollection.First();

                        CardList.Remove(cardModel);
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
                if (IsCardHandle)
                {
                    if (SelectedCard != null && SelectedCard.IsNew)
                    {
                        CardCollection.Remove(SelectedCard);
                    }
                    else if (SelectedCard != null && SelectedCard.IsDirty)
                    {
                        SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
                        CategoryDataAccess categoryDataAccess = new CategoryDataAccess();
                        var cardModel = flashCardEntity.Cards.Where(x=>x.CardID==SelectedCard.CardID).SingleOrDefault();
                        //!!!! reset data
                        ///SelectedCategory.Category = cateModel;
                    }
                }
                else
                {
                    if (SelectedLesson != null && SelectedLesson.IsNew)
                    {
                        LessonCollection.Remove(SelectedLesson);
                    }
                    else if (SelectedLesson != null && SelectedLesson.IsDirty)
                    {
                        LessonDataAccess lessonDataAccess = new LessonDataAccess();
                       ///!!!! var lessonModel = lessonDataAccess.GetItem(SelectedLesson.LessonID);

                        var lessonIndex = LessonCollection.IndexOf(SelectedLesson);
                        if (lessonIndex > -1)
                        {
                            LessonCollection.RemoveAt(lessonIndex);
                            ///!!!!  LessonCollection.Insert(lessonIndex, lessonModel);
                        }
                        RaisePropertyChanged(() => LessonCollection);
                    }
                }


                if (!ViewCore.grdControl.Children.Contains(_studyConfigView))
                {
                    _studyConfigView = new StudyConfigView();
                    var studyConfigViewModel = _studyConfigView.GetViewModel<StudyConfigViewModel>();
                    studyConfigViewModel.LessonCollection =  this.LessonCollection.ToList();
                    ///!!!! var cateWithHasLesson = this.CategoryCollection.Where(x => x.LessonNum > 0);
                    ///!!!!  studyConfigViewModel.CategoryCollection = cateWithHasLesson;
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
            if (IsCardHandle)
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
                if (IsCardHandle)
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
            if (IsCardHandle)
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
                if (IsCardHandle)
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
                SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
                LessonCollection = new ObservableCollection<LessonModel>(flashCardEntity.Lessons.ToList().Select(x => new LessonModel(x)));
                if (LessonCollection.Any())
                {
                    // SelectedLesson =LessonCollection.FirstOrDefault();
                }
                else
                {
                    LessonCollection = new ObservableCollection<LessonModel>();
                    NewExecute(null);
                }
                ///!!!! LessonTypeCollection = new List<KindModel>(typeDataAccess.GetAll());

                CardCollection = new ObservableCollection<CardModel>(flashCardEntity.Cards.ToList().Select(x=>new CardModel(x)));

                CardList = CardCollection.ToList();

                //if (CategoryCollection.Any())
                //    SelectedCategory = CategoryCollection.FirstOrDefault();
                //else
                //{
                //    CategoryCollection = new ObservableCollection<CategoryModel>();
                //    OnNewCategoryExecute(null);
                //}
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }
        #endregion
    }
}
