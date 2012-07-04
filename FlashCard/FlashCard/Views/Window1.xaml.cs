using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FlashCard.Model;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using FlashCard.DataAccess;

namespace FlashCard.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window, INotifyPropertyChanged
    {
        public Window1()
        {
            InitializeComponent();
            LessonDataAccess lessonDataAccess = new LessonDataAccess();
            LessonCollection = new List<LessonModel>(lessonDataAccess.GetAllWithRelation());
            SelectedLesson = LessonCollection.FirstOrDefault();
            this.btnNext.Click += new RoutedEventHandler(btnNext_Click);
            this.btnChanged.Click += new RoutedEventHandler(btnChanged_Click);
            this.DataContext = this;
        }

        void btnChanged_Click(object sender, RoutedEventArgs e)
        {
            if (tblWordBackSide.Visibility.Equals(Visibility.Hidden))
            {
                tblWordBackSide.Visibility = Visibility.Visible;
                tbWords.Visibility = Visibility.Hidden;
            }
            else
            {
                tblWordBackSide.Visibility = Visibility.Hidden;
                tbWords.Visibility = Visibility.Visible;
            }
        }
        int i = -1;
        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            i++;
            SelectedLesson = LessonCollection[i];
        }


        #region LessonCollection
        private List<LessonModel> _lessonCollection;
        /// <summary>
        /// Gets or sets the LessonCollection.
        /// </summary>
        public List<LessonModel> LessonCollection
        {
            get { return _lessonCollection; }
            set
            {
                if (_lessonCollection != value)
                {
                    _lessonCollection = value;
                }
            }
        }


        #endregion



        #region SelectedLesson
        private LessonModel _selectedLesson;
        /// <summary>
        /// Gets or sets the SelectedLesson.
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


        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("propertyExpression must represent a valid Member Expression");

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("propertyExpression must represent a valid Property on the object");

            handler(this, new PropertyChangedEventArgs(propertyInfo.Name));
        }

      



    }
}
