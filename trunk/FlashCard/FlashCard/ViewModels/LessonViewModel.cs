using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using FlashCard.Model;
using System.Windows.Input;
using MVVMHelper.Commands;
using FlashCard.DataAccess;
using System.Collections.ObjectModel;

namespace FlashCard.ViewModels
{
    public class LessonViewModel : ViewModel<LessonManageView>
    {
        #region Constructors
        public LessonViewModel(LessonManageView view)
            : base(view)
        {
            Initialize();
        }

        public LessonViewModel(LessonManageView view, bool isFromPopup)
            : this(view)
        {
            IsFromPopup = isFromPopup;

        }
        #endregion

        #region Properties
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

      
        private List<LessonModel> _lessonCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public List<LessonModel> LessonCollection
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


        private BackSideModel _selectedBackSideModel;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public BackSideModel SelectedBackSideModel
        {
            get { return _selectedBackSideModel; }
            set
            {
                if (_selectedBackSideModel != value)
                {
                    _selectedBackSideModel = value;
                    RaisePropertyChanged(() => SelectedBackSideModel);
                }
            }
        }

        public bool IsFromPopup { get; set; }
        #endregion

        #region Commands
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
            return SelectedLesson != null && !SelectedLesson.IsEditing;
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
            SelectedLesson.Description = null;
            SelectedLesson.IsEdit = false;
            SelectedLesson.IsNew = true;
            SelectedLesson.IsDelete = false;
            SelectedLesson.IsEditing = true;
        }
        #endregion

        #region "  EditCommand"
        private ICommand _editCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(this.EditExecute, this.CanEditExecute);
                }
                return _editCommand;
            }
        }

        private bool CanEditExecute(object param)
        {
            if (SelectedLesson == null)
                return false;
            return !SelectedLesson.IsEditing;
        }

        private void EditExecute(object param)
        {
            if (SelectedLesson.BackSideModel == null)
            {
                SelectedLesson.BackSideModel = new BackSideModel();
                RaisePropertyChanged(() => SelectedLesson);
            }

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
            return SelectedLesson.IsEdit || (SelectedLesson.BackSideCollection != null && SelectedLesson.BackSideCollection.Count(x => x.IsEdit) > 0);
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

        #region "  DeleteCommand"
        private ICommand _deleteCommand;
        //Relay Command In viewModel
        //Gets or sets the property value
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(this.DeleteExecute, this.CanDeleteExecute);
                }
                return _deleteCommand;
            }
        }

        private bool CanDeleteExecute(object param)
        {
            if (SelectedLesson == null)
                return false;
            return true;
        }

        private void DeleteExecute(object param)
        {

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
            ViewCore.DialogResult = true;
            //if (this.IsFromPopup)
            //{
            //    MainWindow mainView = new MainWindow();
            //}
            ViewCore.Close();
        } 
        #endregion

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
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCategory command is executed.
        /// </summary>
        private void OnNewCategoryExecute(object param)
        {
            if ("Add".Equals(param.ToString()))
            {
                SelectedLesson.IsNewCate = true;
                SelectedLesson.CategoryModel = new CategoryModel();
                SelectedLesson.CategoryModel.IsNew = true;
                SelectedLesson.CategoryModel.CategoryID = -2;
            }
            else
            {
                SelectedLesson.IsNewCate = false;
                SelectedLesson.CategoryModel = CategoryCollection.First();
            }
            //RaisePropertyChanged(() => SelectedLesson);
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
            SelectedLesson = param as LessonModel;
            SelectedLesson.IsEditing = false;
        }

        #endregion
        #endregion

        #region Methods
        private void Initialize()
        {
            TypeDataAccess typeDataAccess = new TypeDataAccess();
            LessonTypeCollection = new List<TypeModel>(typeDataAccess.GetAll());

            CategoryDataAccess categoryDataAccess = new CategoryDataAccess();
            CategoryCollection = new List<CategoryModel>(categoryDataAccess.GetAll());

            LessonDataAccess lessonDataAccess = new LessonDataAccess();
            LessonCollection = new List<LessonModel>(lessonDataAccess.GetAllWithRelation());

            if (LessonCollection.Any())
                SelectedLesson = LessonCollection.FirstOrDefault();
            else
            {
                LessonCollection = new List<LessonModel>();
                NewExecute(null);
            }
        }
        #endregion
    }
}
