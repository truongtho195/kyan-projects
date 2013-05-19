using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.Control;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class NoteViewModel : ViewModelBase
    {
        #region Defines

        private base_ResourceNoteRepository _noteRepository = new base_ResourceNoteRepository();

        #endregion

        #region Properties

        private base_ResourceNoteModel _selectedNote;
        /// <summary>
        /// Gets or sets the SelectedNote.
        /// </summary>
        public base_ResourceNoteModel SelectedNote
        {
            get { return _selectedNote; }
            set
            {
                if (_selectedNote != value)
                {
                    _selectedNote = value;
                    OnPropertyChanged(() => SelectedNote);
                }
            }
        }

        private ObservableCollection<PopupContainer> _notePopupCollection = new ObservableCollection<PopupContainer>();
        /// <summary>
        /// Gets or sets the NotePopupCollection.
        /// </summary>
        public ObservableCollection<PopupContainer> NotePopupCollection
        {
            get { return _notePopupCollection; }
            set
            {
                if (_notePopupCollection != value)
                {
                    _notePopupCollection = value;
                    OnPropertyChanged(() => NotePopupCollection);
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
                }
            }
        }

        #endregion

        #region Constructors

        public NoteViewModel()
        {
            InitialCommands();
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
            if (ResourceNoteCollection.Count == Define.CONFIGURATION.DefaultMaximumSticky)
                return;

            // Create a new note
            base_ResourceNoteModel noteModel = new base_ResourceNoteModel
            {
                Resource = SelectedNote.Resource,
                Color = Define.DefaultColorNote,
                DateCreated = DateTimeExt.Now
            };

            // Set position for note
            Point position = ResourceNoteCollection.LastOrDefault().Position;
            noteModel.Position = new Point(position.X + 10, position.Y + 10);

            // Add new note to collection
            ResourceNoteCollection.Add(noteModel);

            // Show new note
            PopupContainer popupContainer = CreatePopupNote(noteModel);
            popupContainer.Show();
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
            // Get popup note
            PopupContainer popupContainer = NotePopupCollection.FirstOrDefault(x => x.DataContext.Equals(this));
            if (popupContainer != null)
            {
                popupContainer.Owner.Activate();

                // Close note view
                popupContainer.Close();

                // Delete note
                NoteViewModel noteViewModel = popupContainer.DataContext as NoteViewModel;
                if (!noteViewModel.SelectedNote.IsNew)
                {
                    _noteRepository.Delete(noteViewModel.SelectedNote.base_ResourceNote);
                    _noteRepository.Commit();
                }

                // Remove popup note
                ResourceNoteCollection.Remove(noteViewModel.SelectedNote);
                NotePopupCollection.Remove(popupContainer);
            }
        }

        #endregion

        #endregion

        #region Methods

        private void InitialCommands()
        {
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);

        }

        /// <summary>
        /// Create popup note
        /// </summary>
        /// <param name="noteModel"></param>
        /// <returns></returns>
        private PopupContainer CreatePopupNote(base_ResourceNoteModel noteModel)
        {
            NoteViewModel noteViewModel = new NoteViewModel();
            noteViewModel.SelectedNote = noteModel;
            noteViewModel.NotePopupCollection = NotePopupCollection;
            noteViewModel.ResourceNoteCollection = ResourceNoteCollection;
            CPC.POS.View.NoteView noteView = new View.NoteView();

            noteViewModel.SaveNote();

            PopupContainer popupContainer = new PopupContainer(noteView,true);
            popupContainer.WindowStartupLocation = WindowStartupLocation.Manual;
            popupContainer.DataContext = noteViewModel;
            NotePopupCollection.Add(popupContainer);
            popupContainer.Width = 150;
            popupContainer.Height = 120;
            popupContainer.MinWidth = 150;
            popupContainer.MinHeight = 120;
            popupContainer.FormBorderStyle = PopupContainer.BorderStyle.None;
            popupContainer.Deactivated += (sender, e) => { noteViewModel.SaveNote(); };
            popupContainer.Loaded += (sender, e) =>
            {
                popupContainer.Left = noteModel.Position.X;
                popupContainer.Top = noteModel.Position.Y;
            };
            return popupContainer;
        }

        private void SaveNote()
        {
            SelectedNote.ToEntity();
            if (SelectedNote.IsNew)
                _noteRepository.Add(SelectedNote.base_ResourceNote);
            _noteRepository.Commit();
            SelectedNote.EndUpdate();
        }

        #endregion
    }
}
