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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestCenter.Database;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace TestCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ObservableCollection<Question> question = GetLessonForTest();
            this.lstQuestion.ItemsSource = question;
        }


        private ObservableCollection<Question> GetLessonForTest()
        {
            WPFlashCardDBEntities entities = new WPFlashCardDBEntities();
            ObservableCollection<Question> QuestionCollection = new ObservableCollection<Question>();
            var AllLessonStudy = entities.StudyDetails.ToList().Take(20);
            var questionID = 1;
            foreach (var item in AllLessonStudy)
            {
                //Check Vietnamese mean
                if (item.Lesson.BackSides.Any(x => x.BackSideTypeID == 1))
                {
                    Question question = new Question();
                    question.QuestionID = questionID;
                    List<Answer> listAnswer = new List<Answer>();

                    question.Detail = string.Format("Vietnamese of \"{0}\"", item.Lesson.LessonName.Trim());
                    //Get Correct Answer
                    var backsideQuestion = item.Lesson.BackSides.SingleOrDefault(x => x.BackSideTypeID == 1);
                    Answer correctAnswer = new Answer() { AnswerID = 1, AnswerText = backsideQuestion.Content, IsCorrect = true, Choose = null };
                    listAnswer.Add(correctAnswer);

                    //Get another Anwser
                    var backSideRamdom = ShuffleList.Randomize<BackSide>(entities.BackSides.Where(x => x.BackSideTypeID == 1 && x.BackSideID != backsideQuestion.BackSideID).ToList());
                    
                    foreach (var backSideItem in backSideRamdom.Take(3))
                    {
                        var id = listAnswer.Max(x => x.AnswerID) + 1;
                        Answer answer = new Answer() { AnswerID = id, AnswerText = backSideItem.Content, IsCorrect = false, Choose = null };
                        listAnswer.Add(answer);
                    }
                   
                    question.AnswerCollection = new ObservableCollection<Answer>( ShuffleList.Randomize<Answer>(listAnswer));
                    QuestionCollection.Add(question);
                    questionID = QuestionCollection.Max(x => x.QuestionID) + 1;
                }
            }

            return QuestionCollection;
        }



    }

    public static class ShuffleList
    {
        public static IList<T> Randomize<T>(IList<T> list)
        {
            List<T> randomizedList = new List<T>();
            Random rnd = new Random();
            while (list.Count > 0)
            {
                int index = rnd.Next(0, list.Count); //pick a random item from the master list
                randomizedList.Add(list[index]); //place it at the end of the randomized list
                list.RemoveAt(index);
            }
            return randomizedList;
        }
    }

    public class Question : NotifyPropertyChangedBase
    {

        #region QuestionID
        private int _questionID;
        /// <summary>
        /// Gets or sets the QuestionID.
        /// </summary>
        public int QuestionID
        {
            get { return _questionID; }
            set
            {
                if (_questionID != value)
                {
                    _questionID = value;
                    RaisePropertyChanged(() => QuestionID);
                }
            }
        }
        #endregion

        #region Detail
        private string _detail;
        /// <summary>
        /// Gets or sets the Detail.
        /// </summary>
        public string Detail
        {
            get { return _detail; }
            set
            {
                if (_detail != value)
                {
                    _detail = value;
                    RaisePropertyChanged(() => Detail);
                }
            }
        }
        #endregion

        #region AnswerCollection
        private ObservableCollection<Answer> _answerCollection;
        /// <summary>
        /// Gets or sets the AnswerCollection.
        /// </summary>
        public ObservableCollection<Answer> AnswerCollection
        {
            get { return _answerCollection; }
            set
            {
                if (_answerCollection != value)
                {
                    _answerCollection = value;
                    RaisePropertyChanged(() => AnswerCollection);
                }
            }
        }
        #endregion
    }

    public class Answer : NotifyPropertyChangedBase
    {

        #region AnswerID
        private int _answerID;
        /// <summary>
        /// Gets or sets the AnswerID.
        /// </summary>
        public int AnswerID
        {
            get { return _answerID; }
            set
            {
                if (_answerID != value)
                {
                    _answerID = value;
                    RaisePropertyChanged(() => AnswerID);
                }
            }
        }
        #endregion

        #region AnswerText
        private string _answerText;
        /// <summary>
        /// Gets or sets the AnswerText.
        /// </summary>
        public string AnswerText
        {
            get { return _answerText; }
            set
            {
                if (_answerText != value)
                {
                    _answerText = value;
                    RaisePropertyChanged(() => AnswerText);
                }
            }
        }
        #endregion

        #region IsCorrect
        private bool _isCorrect;
        /// <summary>
        /// Gets or sets the IsCorrect.
        /// </summary>
        public bool IsCorrect
        {
            get { return _isCorrect; }
            set
            {
                if (_isCorrect != value)
                {
                    _isCorrect = value;
                    RaisePropertyChanged(() => IsCorrect);
                }
            }
        }
        #endregion

        #region Choose
        private bool? _choose;
        /// <summary>
        /// Gets or sets the Choose.
        /// </summary>
        public bool? Choose
        {
            get { return _choose; }
            set
            {
                if (_choose != value)
                {
                    _choose = value;
                    RaisePropertyChanged(() => Choose);
                    RaisePropertyChanged(() => CorrectAnswer);
                }
            }
        }
        #endregion


        #region CorrectAnswer
        /// <summary>
        /// Gets or sets the CorrectAnswer.
        /// </summary>
        public bool? CorrectAnswer
        {
            get
            {
                if (Choose.HasValue)
                    if (IsCorrect == Choose)
                        return true;
                    else
                        return false;
                return null;
            }

        }
        #endregion

    }

    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

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



        #endregion
    }

}
