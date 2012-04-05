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
                    SelectedLessonChanged();
                }
            }
        }

        private void SelectedLessonChanged()
        {
            if (SelectedLesson != null)
            {
                if(SelectedLesson.BackSideCollection!=null && SelectedLesson.BackSideCollection.Any())
                    this.SelectedLesson.BackSideModel = SelectedLesson.BackSideCollection.FirstOrDefault();
                SelectedLesson.IsEditing = false;
            }

        }


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

        
        


        #endregion

        #region Commands

        #region NewCommand
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
            return SelectedLesson!=null && !SelectedLesson.IsEditing ;
        }

        private void NewExecute(object param)
        {
            SelectedLesson = new LessonModel();
            SelectedLesson.TypeID = LessonTypeCollection.First().TypeID;
            SelectedLesson.CategoryID = CategoryCollection.First().CategoryID;
            SelectedLesson.BackSideCollection = new ObservableCollection<BackSideModel>();
            SelectedLesson.BackSideModel = new BackSideModel();
            SelectedLesson.BackSideModel.IsCorrect = false;
            SelectedLesson.IsEdit = false;
            SelectedLesson.IsNew = true;
            SelectedLesson.IsDelete = false;
            SelectedLesson.IsEditing = true;

        } 
        #endregion

        #region EditCommand
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
            return !SelectedLesson.IsEditing ;
        }

        private void EditExecute(object param)
        {
            SelectedLesson.IsEditing = true;
        }
        #endregion

        #region SaveCommand
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
            return SelectedLesson.IsEdit || (SelectedLesson.BackSideCollection!=null && SelectedLesson.BackSideCollection.Count(x=>x.IsEdit)>0);
        }

        private void SaveExecute(object param)
        {
            LessonDataAccess lessonDataAccess = new LessonDataAccess();
            switch (SelectedLesson.TypeID)
            {
                case 1:
                case 3:
                    if (SelectedLesson.BackSideCollection == null)
                        SelectedLesson.BackSideCollection = new ObservableCollection<BackSideModel>();
                    SelectedLesson.BackSideCollection.Clear();
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
            SelectedLesson.IsEditing = false;
        }
        #endregion

        #region AddBackSideCommand
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


        #endregion

        #region Methods
        private void Initialize()
        {
            TypeDataAccess typeDataAccess = new TypeDataAccess();
            LessonTypeCollection = new List<TypeModel>(typeDataAccess.GetAll());

            CategoryDataAccess categoryDataAccess = new CategoryDataAccess();
            CategoryCollection = new List<CategoryModel>(categoryDataAccess.GetAll());

            LessonDataAccess lessonDataAccess = new LessonDataAccess();
            LessonCollection = new ObservableCollection<LessonModel>(lessonDataAccess.GetAllWithRelation());

            if (LessonCollection.Any())
                SelectedLesson = LessonCollection.FirstOrDefault();
            else
                NewExecute(null);
        }
        #endregion
    }
}
