﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using FlashCard.Database;
using FlashCard.Database.Repository;
using FlashCard.Views;
using log4net;
using MVVMHelper.Commands;

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
        public LessonViewModel(LessonManageView view,List<LessonModel> lessonCollection):this(view)
        {
            try
            {
                //handle Statup for file
                if (App.SetupModel != null && (App.SetupModel.IsRunStatup || App.SetupModel.IsOpenLastStudy == true))
                {
                    MainWindow mainWindow = new MainWindow();
                    var mainViewModel = mainWindow.GetViewModel<MainViewModel>();
                    mainViewModel.GetLesson(lessonCollection.ToList());
                    mainViewModel.ExcuteMainForm();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        #endregion

        #region Variables
        private StudyConfigView _studyConfigView;
        private ICollectionView _lessonCollectionView;
        private ICollectionView _cardCollectionView;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsFromPopup { get; set; }
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
                if (value != null && !value.IsNew)
                {
                    _selectedLesson = value;
                    _selectedLesson.ToModel();
                    //_selectedLesson.CategoryID = _selectedLesson.Lesson.CategoryID;
                    //_selectedLesson.CardID = _selectedLesson.Lesson.CardID;
                    //_selectedLesson.LessonName = _selectedLesson.Lesson.LessonName;
                    //_selectedLesson.Description = _selectedLesson.Lesson.Description;

                    _selectedLesson.BackSideCollection = new ObservableCollection<BackSideModel>(_selectedLesson.Lesson.BackSides.ToList().Select(x => new BackSideModel(x)));
                    _selectedLesson.IsDirty = false;
                }
                else if (value != null)
                {
                    _selectedLesson = value;
                    _selectedLesson.CategoryID = "1";
                    _selectedLesson.CardID = "1";
                    _selectedLesson.LessonName = string.Empty;
                    _selectedLesson.Description = string.Empty;
                    _selectedLesson.BackSideCollection = new ObservableCollection<BackSideModel>();
                    _selectedLesson.IsDirty = false;
                }
                RaisePropertyChanged(() => SelectedLesson);
            }
        }

        #endregion

        #region"  SelectedBackSide"

        private BackSideModel _selectedBackSide;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public BackSideModel SelectedBackSide
        {
            get { return _selectedBackSide; }
            set
            {
                if (_selectedBackSide != value)
                {
                    _selectedBackSide = value;
                    RaisePropertyChanged(() => SelectedBackSide);
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

                if (value != null && !value.IsNew)
                {
                    _selectedCard = value;
                    _selectedCard.CardID = value.Card.CardID;
                    _selectedCard.CardName = value.Card.CardName;
                    _selectedCard.Remark = value.Card.Remark;
                    _selectedCard.EndUpdate();
                    RaisePropertyChanged(() => SelectedCard);
                }
                else if (value != null)
                {
                    _selectedCard = value;
                    _selectedCard.CardID = string.Empty;
                    _selectedCard.CardName = string.Empty;
                    _selectedCard.Remark = string.Empty;
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

        #region"  CardList"
        private List<Card> _cardList;
        /// <summary>
        /// Gets or sets the CategoryList.
        /// </summary>
        public List<Card> CardList
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

        #region"  IsCardHandle"
        private bool _isCardHandle = false;
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
            if (SelectedLesson == null)
                return true;

            return SelectedLesson != null && !SelectedLesson.IsDirty;
        }

        private void NewExecute(object param)
        {
            SelectedLesson = new LessonModel();
            SelectedLesson.BackSideCollection = new ObservableCollection<BackSideModel>();
            SelectedLesson.BackSideCollection.Add(new BackSideModel() { BackSideName = "Main Back Side", IsMain = 1 });
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
            return (SelectedLesson.IsDirty || SelectedLesson.BackSideCollection.Count(x => x.IsDirty) > 0) &&
                    SelectedLesson.Errors.Count() == 0 &&
                    SelectedLesson.BackSideCollection.Count(x => x.Errors.Count > 0) == 0;
        }

        private void SaveExecute(object param)
        {
            try
            {
                LessonRepository lessonRepository = new LessonRepository();
                if (SelectedLesson.IsNew)
                    SelectedLesson.LessonID = AutoGeneration.NewSeqGuid().ToString();
                //Mapping
                SelectedLesson.ToEntity();

                BackSideRepository backSideRepository = new BackSideRepository();
                if (SelectedLesson.IsNew)
                {
                    foreach (var backSideModel in SelectedLesson.BackSideCollection)
                    {
                        backSideModel.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                        backSideModel.LessonID = SelectedLesson.LessonID;
                        backSideModel.ToEntity();
                        SelectedLesson.Lesson.BackSides.Add(backSideModel.BackSide);
                        backSideModel.EndUpdate();
                    }
                    lessonRepository.Add(SelectedLesson.Lesson);
                    lessonRepository.Commit();
                    LessonCollection.Add(SelectedLesson);
                }
                else
                {
                    foreach (var backSideModel in SelectedLesson.BackSideCollection.ToList())
                    {
                        if (backSideModel.IsDeleted)
                        {
                            var chk = SelectedLesson.Lesson.BackSides.Remove(backSideModel.BackSide);
                            BackSide deleteBackSide = backSideRepository.GetSingle<BackSide>(x => x.BackSideID.Equals(backSideModel.BackSide.BackSideID));
                            backSideRepository.Delete<BackSide>(deleteBackSide);
                            SelectedLesson.BackSideCollection.Remove(backSideModel);
                            backSideRepository.Commit();
                        }
                        else if (backSideModel.IsNew)
                        {
                            backSideModel.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                            backSideModel.LessonID = SelectedLesson.LessonID;
                            SelectedLesson.Lesson.BackSides.Add(backSideModel.BackSide);
                        }
                        backSideModel.ToEntity();
                        backSideModel.EndUpdate();
                    }
                    lessonRepository.Update<Lesson>(SelectedLesson.Lesson);
                    lessonRepository.Commit();
                }
                SelectedLesson.EndUpdate();

                RaisePropertyChanged(() => SelectedLesson);
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
            try
            {
                MessageBoxResult result = MessageBox.Show(ViewCore as Window, "Do you want to remove this Lesson", "Remove Lesson", MessageBoxButton.YesNo);
                if (result.Equals(MessageBoxResult.Yes))
                {
                    LessonModel lessonModel;
                    if (param != null)
                    {
                        lessonModel = param as LessonModel;
                        LessonRepository lessonRepository = new LessonRepository();
                        lessonRepository.Delete(lessonModel.Lesson);
                        lessonRepository.Commit();
                        if (SelectedLesson == lessonModel)
                        {
                            var indexLesson = LessonCollection.IndexOf(SelectedLesson);
                            ViewCore.lstLesson.SelectedIndex = indexLesson - 1;

                        }
                        LessonCollection.Remove(lessonModel);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
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
                    SelectedLesson.Dispose();
                }
                SelectedLesson = param as LessonModel;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        #endregion

        #region "  AddBackSideCommand"
        private ICommand _addBackSideCommand;
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
            if (SelectedLesson == null || SelectedLesson.BackSideCollection == null)
                return false;
            return SelectedLesson.BackSideCollection != null && SelectedLesson.BackSideCollection.Count(x => x.Errors.Count > 0) == 0;
        }

        private void AddBackSideExecute(object param)
        {
            if (SelectedLesson.BackSideCollection == null)
                SelectedLesson.BackSideCollection = new ObservableCollection<BackSideModel>();
            SelectedLesson.BackSideCollection.Add(new BackSideModel() { IsMain = 0 });
        }
        #endregion

        #region"  DeleteBackSideCommand"

        private ICommand _deleteBackSideCommand;
        /// <summary>
        /// Gets the DeleteBackSide Command.
        /// <summary>
        public ICommand DeleteBackSideCommand
        {
            get
            {
                if (_deleteBackSideCommand == null)
                    _deleteBackSideCommand = new RelayCommand(this.OnDeleteBackSideExecute, this.OnDeleteBackSideCanExecute);
                return _deleteBackSideCommand;
            }
        }

        /// <summary>
        /// Method to check whether the DeleteBackSide command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteBackSideCanExecute(object param)
        {
            var backSideModel = param as BackSideModel;
            if (backSideModel == null)
                return false;
            return backSideModel.IsMain == 0;
        }

        /// <summary>
        /// Method to invoke when the DeleteBackSide command is executed.
        /// </summary>
        private void OnDeleteBackSideExecute(object param)
        {
            try
            {
                var backSideModel = param as BackSideModel;
                if (backSideModel.IsNew)
                {
                    SelectedLesson.BackSideCollection.Remove(backSideModel);
                }
                else
                {
                    if (backSideModel.Errors.Count > 0)
                    {
                        backSideModel.BackSideName = "BackSideName";
                        backSideModel.Content = "Content";
                    }
                    backSideModel.IsDeleted = true;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
            try
            {
                _lessonCollectionView = CollectionViewSource.GetDefaultView(this.LessonCollection);
                string keywordLesson = string.Empty;
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
                        if (lessonModel.Lesson.LessonName.ToLower().Contains(keywordLesson.TrimStart().ToLower()) || lessonModel.Lesson.Card.CardName.ToLower().Contains(keywordLesson.TrimStart().ToLower()))
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
                    _newCardCommand = new RelayCommand(this.OnNewCardExecute, this.OnNewCardCanExecute);
                return _newCardCommand;
            }
        }

        /// <summary>
        /// Method to check whether the NewCategory command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCardCanExecute(object param)
        {
            if (CardCollection == null)
                return true;
            return SelectedCard != null && !SelectedCard.IsNew;
        }

        /// <summary>
        /// Method to invoke when the NewCategory command is executed.
        /// </summary>
        private void OnNewCardExecute(object param)
        {
            SelectedCard = new CardModel();
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
                    _saveCardCommand = new RelayCommand(this.SaveCardExecute, this.CanSaveCardExecute);
                }
                return _saveCardCommand;
            }
        }

        private bool CanSaveCardExecute(object param)
        {
            if (SelectedCard == null)
                return false;

            return SelectedCard.IsDirty && SelectedCard.Errors.Count() == 0;
        }

        private void SaveCardExecute(object param)
        {
            SaveCard(SelectedCard);
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
                    SelectedCard.Dispose();
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

        #region"  SearchCard"

        /// <summary>
        /// Gets the SearchCategory Command.
        /// <summary>
        private ICommand _searchCardCommand;
        public ICommand SearchCardCommand
        {
            get
            {
                if (_searchCardCommand == null)
                    _searchCardCommand = new RelayCommand(this.OnSearchCardExecute, this.OnSearchCardCanExecute);
                return _searchCardCommand;
            }
        }

        /// <summary>
        /// Method to check whether the SearchCategory command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchCardCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SearchCategory command is executed.
        /// </summary>
        private void OnSearchCardExecute(object param)
        {
            _cardCollectionView = CollectionViewSource.GetDefaultView(this.CardCollection);
            string keywordCard = string.Empty;

            try
            {
                this._cardCollectionView.Filter = (item) =>
                {
                    var cardModel = item as CardModel;
                    if (cardModel == null)
                        return false;

                    if (param == null)
                        return false;
                    else
                        keywordCard = param.ToString();

                    if (string.IsNullOrWhiteSpace(keywordCard))
                        return true;
                    else
                    {
                        if (cardModel.Card.CardName.ToLower().Contains(keywordCard.TrimStart().ToLower()))
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

        #region"  DeleteCardCommand"

        /// <summary>
        /// Gets the DeleteCategory Command.
        /// <summary>
        private ICommand _deleteCardCommand;
        public ICommand DeleteCardCommand
        {
            get
            {
                if (_deleteCardCommand == null)
                    _deleteCardCommand = new RelayCommand(this.OnDeleteCardExecute, this.OnDeleteCardCanExecute);
                return _deleteCardCommand;
            }
        }

        /// <summary>
        /// Method to check whether the DeleteCategory command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCardCanExecute(object param)
        {
            if (param == null)
                return false;
            return !(param as CardModel).Card.CardID.Equals("1");
        }

        /// <summary>
        /// Method to invoke when the DeleteCategory command is executed.
        /// </summary>
        private void OnDeleteCardExecute(object param)
        {
            MessageBoxResult result = MessageBox.Show(ViewCore as Window, "Do you want to remove this Category & Lesson relation", "Remove Category", MessageBoxButton.YesNo);
            if (result.Equals(MessageBoxResult.Yes))
            {
                CardModel cardModel;
                if (param != null)
                {
                    cardModel = param as CardModel;
                    var resultDel = true;
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
                        CardRepository cardRepository = new CardRepository();
                        cardRepository.Delete<Card>(cardModel.Card);
                        cardRepository.Commit();
                        CardCollection.Remove(cardModel);
                        if (cardModel == SelectedCard)
                            SelectedCard = CardCollection.First();

                        CardList.Remove(cardModel.Card);

                        var CardLesson = LessonCollection.Where(x => x.CardID.Equals(cardModel.Card.CardID));
                        foreach (var item in CardLesson.ToList())
                        {
                            LessonCollection.Remove(item);
                        }

                    }
                }
            }
        }
        #endregion

        #region"  ExportCardCommand"
        private ICommand _exportCardCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand ExportCardCommand
        {
            get
            {
                if (_exportCardCommand == null)
                {
                    _exportCardCommand = new RelayCommand(this.ExportCardExecute, this.CanExportCardExecute);
                }
                return _exportCardCommand;
            }
        }

        private bool CanExportCardExecute(object param)
        {
            if (SelectedCard == null)
                return false;
            return SelectedCard.Card.Lessons.Count > 0;
        }

        private void ExportCardExecute(object param)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Filter = "Flash Card File (*.fcard) |*.fcard";
                dialog.FileName = string.Format("{0}.fcard", SelectedCard.Card.CardName);
                var result = dialog.ShowDialog(ViewCore);
                if (!string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    Serializer<Card>.Serialize(SelectedCard.Card, dialog.FileName);
                }

                MessageBox.Show(ViewCore as Window, "Flash Card Export success !","Information" ,MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            
        }
        #endregion

        #region"  ImportCommand"

        /// <summary>
        /// Gets the Import Command.
        /// <summary>
        private ICommand _importCardCommand;
        public ICommand ImportCardCommand
        {
            get
            {
                if (_importCardCommand == null)
                    _importCardCommand = new RelayCommand(this.OnImportCardExecute, this.OnImportCardCanExecute);
                return _importCardCommand;
            }
        }

        /// <summary>
        /// Method to check whether the Import command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnImportCardCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the Import command is executed.
        /// </summary>
        private void OnImportCardExecute(object param)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
                openDialog.Filter = "Flash Card File (*.fcard) |*.fcard";
                openDialog.Multiselect = false;
                openDialog.ShowDialog();
                List<LessonModel> listLesson = new List<LessonModel>();
                List<BackSideModel> listBackSide;

                if (!string.IsNullOrWhiteSpace(openDialog.FileName))
                {
                    var card = Serializer<Card>.Deserialize(openDialog.FileName);

                    CardModel cardModel = new CardModel();
                    if (card != null)
                    {
                        cardModel.CardID = AutoGeneration.NewSeqGuid().ToString();
                        cardModel.CardName = card.CardName;
                        cardModel.Remark = card.Remark;
                        cardModel.ToEntity();
                        //Handle Card.Lessons
                        if (card.Lessons != null && card.Lessons.Count > 0)
                        {
                            listLesson = new List<LessonModel>();
                            foreach (var lesson in card.Lessons)
                            {
                                if (lesson != null)
                                {
                                    LessonModel lessonModel = new LessonModel();
                                    lessonModel.LessonID = AutoGeneration.NewSeqGuid().ToString();
                                    lessonModel.CardID = cardModel.CardID;
                                    lessonModel.LessonName = lesson.LessonName;
                                    lessonModel.Description = lesson.Description;
                                    lessonModel.CategoryID = lesson.CategoryID;
                                    lessonModel.ToEntity();
                                    //Handle Lesson.BackSides
                                    if (lesson.BackSides != null && lesson.BackSides.Count > 0)
                                    {
                                        listBackSide = new List<BackSideModel>();
                                        foreach (var backSide in lesson.BackSides)
                                        {
                                            if (backSide != null)
                                            {
                                                BackSideModel backSideModel = new BackSideModel();
                                                backSideModel.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                                                backSideModel.LessonID = backSide.LessonID;
                                                backSideModel.BackSideName = backSide.BackSideName;
                                                backSideModel.Content = backSide.Content;
                                                backSideModel.IsMain = backSide.IsMain;
                                                backSideModel.ToEntity();
                                                //add BackSide to Lessons
                                                lessonModel.Lesson.BackSides.Add(backSideModel.BackSide);
                                                backSideModel.EndUpdate();
                                                listBackSide.Add(backSideModel);
                                            }
                                        }
                                    }//End handle BackSide

                                    //Add Lesson To Card
                                    lessonModel.EndUpdate();
                                    cardModel.Card.Lessons.Add(lessonModel.Lesson);
                                    listLesson.Add(lessonModel);
                                }
                            }
                        }//End Handle Lesson

                        CardRepository cardRepository = new CardRepository();
                        cardRepository.Add<Card>(cardModel.Card);
                        cardRepository.Commit();

                        // Add To UI

                        cardModel.EndUpdate();
                        CardCollection.Add(cardModel);
                        CardList.Add(cardModel.Card);

                        foreach (var lessonModel in listLesson)
                        {
                            LessonCollection.Add(lessonModel);
                        }
                        string msg = string.Format("Card {0} & {1} Lesson(s) import success!", cardModel.CardName, listLesson.Count());
                        MessageBox.Show(ViewCore as Window, msg, "Information", MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
                }
                else
                {
                    if (SelectedLesson != null && SelectedLesson.IsNew)
                    {
                        LessonCollection.Remove(SelectedLesson);
                    }
                }


                if (!ViewCore.grdControl.Children.Contains(_studyConfigView))
                {
                    _studyConfigView = new StudyConfigView();
                    var studyConfigViewModel = _studyConfigView.GetViewModel<StudyConfigViewModel>();
                    studyConfigViewModel.LessonCollection = this.LessonCollection.ToList();
                    var cateWithHasLesson = this.CardCollection.Where(x => x.Card.Lessons.Count > 0).ToList();
                    studyConfigViewModel.CardCollection = cateWithHasLesson;
                    ViewCore.grdUserControl.Visibility = System.Windows.Visibility.Visible;
                    studyConfigViewModel.ButtonClickHandler += new StudyConfigViewModel.handlerControl(LessonViewModel_DoNow);
                    ViewCore.grdControl.Children.Add(_studyConfigView);
                }
                else
                {
                    ViewCore.grdUserControl.Visibility = System.Windows.Visibility.Hidden;
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

        private ICommand _shortcutKeyNewItemCommand;
        /// <summary>
        /// Gets the ShortcutKeyNewItem Command.
        /// <summary>
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
                return OnNewCardCanExecute(param);
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
                    if (OnNewCardCanExecute(null))
                    {
                        OnNewCardExecute(null);
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
        private ICommand _shortcutKeySaveItemCommand;
        /// <summary>
        /// Gets the ShortCutKeySaveItem Command.
        /// <summary>
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
                return CanSaveCardExecute(param);
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
                    if (CanSaveCardExecute(param))
                    {
                        SaveCardExecute(param);
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
                LessonRepository lessonRepository = new LessonRepository();
                CategoryRepository categoryRepository = new CategoryRepository();
                CardRepository cardRepository = new CardRepository();
                SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
                LessonCollection = new ObservableCollection<LessonModel>(lessonRepository.GetAll<Lesson>().Select(x => new LessonModel(x)));
                if (LessonCollection.Any())
                {
                    //OnSelectionChangedExecute(LessonCollection.FirstOrDefault());
                    SelectedLesson = LessonCollection.FirstOrDefault();
                }
                else
                {
                    LessonCollection = new ObservableCollection<LessonModel>();
                    NewExecute(null);
                }
                CategoryCollection = new List<CategoryModel>(categoryRepository.GetAll<Category>().Select(x => new CategoryModel(x)));

                CardCollection = new ObservableCollection<CardModel>(cardRepository.GetAll<Card>().Select(x => new CardModel(x)));

                CardList = CardCollection.Select(x => x.Card).ToList();
                RaisePropertyChanged(() => CardList);

                if (CardCollection.Any())
                {
                    //ViewCore.lstCategories.SelectedItem = CardCollection.FirstOrDefault();
                    SelectedCard = CardCollection.FirstOrDefault();
                }
                else
                {
                    CardCollection = new ObservableCollection<CardModel>();
                    OnNewCardExecute(null);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

        /// <summary>
        /// Update or Save new card
        /// </summary>
        /// <param name="cardModel"></param>
        private void SaveCard(CardModel cardModel)
        {
            try
            {
                CardRepository cardRepository = new CardRepository();
                if (cardModel.IsNew)
                {
                    cardModel.CardID = AutoGeneration.NewSeqGuid().ToString();
                    cardModel.ToEntity();
                    cardRepository.Add<Card>(SelectedCard.Card);
                    cardRepository.Commit();
                    cardModel.EndUpdate();
                    CardCollection.Add(cardModel);
                    CardList.Add(cardModel.Card);
                }
                else
                {
                    cardModel.ToEntity();
                    cardRepository.Update(cardModel.Card);
                    cardRepository.Commit();
                    cardModel.EndUpdate();
                }
                RaisePropertyChanged(() => CardList);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

       
        #endregion
    }
}