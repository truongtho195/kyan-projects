using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;
using System.Windows.Data;

namespace MVVMHelper.Commands
{

    // This handy code snippet comes from Josh Smith's blog. 
    // http://blogs.infragistics.com/blogs/joshs/archive/2008/06/26/data-binding-the-isvisible-property-of-contextualtabgroup.aspx
     
    public class DataContextSpy
    : Freezable // Enable ElementName and DataContext bindings
    {
        public DataContextSpy()
        {
            // This binding allows the spy to inherit a DataContext.
            BindingOperations.SetBinding(this, DataContextProperty, new Binding());
        }

        public object DataContext
        {
            get { return (object)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }

        // Borrow the DataContext dependency property from FrameworkElement.
        public static readonly DependencyProperty DataContextProperty =
            FrameworkElement.DataContextProperty.AddOwner(typeof(DataContextSpy));

        protected override Freezable CreateInstanceCore()
        {
            // We are required to override this abstract method.
            throw new NotImplementedException();
        }
    }



    public class NonRoutedRibbonCommandDelegator : DependencyObject, ICommand , INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        ///   Cached value for the CommandBinding associated with this RibbonCommand.
        /// </summary>
        // private CommandBinding _commandBinding;

        /// <summary>
        ///   Backing store for the LargeImageSource property.
        /// </summary>
        private ImageSource _largeImageSource;

        /// <summary>
        ///   Backing store for the SmallImageSource property.
        /// </summary>
        private ImageSource _smallImageSource;

        /// <summary>
        ///   Backing store for the LabelTitle property.
        /// </summary>
        private string _labelTitle;

        /// <summary>
        ///   Backing store for the LabelDescription property.
        /// </summary>
        private string _labelDescription;

        /// <summary>
        ///   Backing store for the ToolTipTitle property.
        /// </summary>
        private string _toolTipTitle;

        /// <summary>
        ///   Backing store for the ToolTipDescription property.
        /// </summary>
        private string _toolTipDescription;

        /// <summary>
        ///   Backing store for the ToolTipImageSource property.
        /// </summary>
        private ImageSource _toolTipImageSource;

        /// <summary>
        ///   Backing store for the ToolTipFooterTitle property.
        /// </summary>
        private string _toolTipFooterTitle;

        /// <summary>
        ///   Backing store for the ToolTipFooterDescription property.
        /// </summary>
        private string _toolTipFooterDescription;

        /// <summary>
        ///   Backing store for the ToolTipFooterImageSource property.
        /// </summary>
        private ImageSource _toolTipFooterImageSource;

        
        #endregion

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the RibbonCommand class.
        /// </summary>
        public NonRoutedRibbonCommandDelegator()               
        {
        }
  
        #endregion

        #region Public Events




        public ICommand ActualCommand
        {
            get { return (ICommand)GetValue(ActualCommandProperty); }
            set { SetValue(ActualCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualCommandProperty =
            DependencyProperty.Register("ActualCommand", typeof(ICommand), typeof(NonRoutedRibbonCommandDelegator),
            new UIPropertyMetadata(new PropertyChangedCallback(OnActualCommandChanged)));

        protected ICommand _actualCommandCache = null; 

        static void OnActualCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NonRoutedRibbonCommandDelegator instance = d as NonRoutedRibbonCommandDelegator;
            System.Diagnostics.Debug.Assert(instance != null);
            if (instance != null)
            {
                instance._actualCommandCache = e.NewValue as ICommand ; 
            }
            CommandManager.InvalidateRequerySuggested(); 
        } 


        /// <summary>
        ///   This event is raised when a property of RibbonCommand changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets the LargeImageSource for this RibbonCommand.  At 96dpi this
        ///   is normally a 32x32 icon.
        /// </summary>
        public ImageSource LargeImageSource
        {
            get
            {
                return _largeImageSource;
            }

            set
            {
                if (value != _largeImageSource)
                {
                    _largeImageSource = value;
                    this.NotifyPropertyChanged("LargeImageSource");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the SmallImageSource for this RibbonCommand.  At 96dpi this
        ///   is normally a 16x16 icon.
        /// </summary>
        public ImageSource SmallImageSource
        {
            get
            {
                return _smallImageSource;
            }

            set
            {
                if (value != _smallImageSource)
                {
                    _smallImageSource = value;
                    this.NotifyPropertyChanged("SmallImageSource");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the LabelTitle for this RibbonCommand.  This is the primary
        ///   label that will be used on a bound Ribbon control.
        /// </summary>
        public string LabelTitle
        {
            get
            {
                return _labelTitle;
            }

            set
            {
                if (value != _labelTitle)
                {
                    _labelTitle = value;
                    this.NotifyPropertyChanged("LabelTitle");
                }
                //SetValue(LabelTitleProperty, value);
            }
        }

        //private static readonly DependencyProperty LabelTitleProperty =
        //    DependencyProperty.Register("LabelTitle"
        //    , typeof(string)
        //    , typeof(NonRoutedRibbonCommandDelegator)
        //    , new PropertyMetadata(string.Empty));

        /// <summary>
        ///   Gets or sets the LabelDescription for this RibbonCommand.
        /// </summary>
        public string LabelDescription
        {
            get
            {
                return _labelDescription;
            }

            set
            {
                if (value != _labelDescription)
                {
                    _labelDescription = value;
                    this.NotifyPropertyChanged("LabelDescription");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipTitle for this RibbonCommand.  This is
        ///   used as the main header of the ToolTip for a Ribbon control bound
        ///   to this RibbonCommand.
        /// </summary>
        public string ToolTipTitle
        {
            get
            {
                return _toolTipTitle;
            }

            set
            {
                if (value != _toolTipTitle)
                {
                    _toolTipTitle = value;
                    this.NotifyPropertyChanged("ToolTipTitle");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipDescription for this RibbonCommand.  This is
        ///   used as the main body description of the ToolTip for a Ribbon control
        ///   bound to this RibbonCommand.
        /// </summary>
        public string ToolTipDescription
        {
            get
            {
                return _toolTipDescription;
            }

            set
            {
                if (value != _toolTipDescription)
                {
                    _toolTipDescription = value;
                    this.NotifyPropertyChanged("ToolTipDescription");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipImageSource for this RibbonCommand.  This is
        ///   the main image in the body of the ToolTip for a Ribbon control bound
        ///   to this RibbonCommand.
        /// </summary>
        public ImageSource ToolTipImageSource
        {
            get
            {
                return _toolTipImageSource;
            }

            set
            {
                if (value != _toolTipImageSource)
                {
                    _toolTipImageSource = value;
                    this.NotifyPropertyChanged("ToolTipImageSource");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipFooterTitle for this RibbonCommand.  This is
        ///   the title of the footer of the ToolTip for a Ribbon control bound
        ///   to this RibbonCommand.
        /// </summary>
        public string ToolTipFooterTitle
        {
            get
            {
                return _toolTipFooterTitle;
            }

            set
            {
                if (value != _toolTipFooterTitle)
                {
                    _toolTipFooterTitle = value;
                    this.NotifyPropertyChanged("ToolTipFooterTitle");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipFooterDescription for this RibbonCommand.  This is
        ///   the main description in the footer of the ToolTip for a Ribbon control bound
        ///   to this RibbonCommand.
        /// </summary>
        public string ToolTipFooterDescription
        {
            get
            {
                return _toolTipFooterDescription;
            }

            set
            {
                if (value != _toolTipFooterDescription)
                {
                    _toolTipFooterDescription = value;
                    this.NotifyPropertyChanged("ToolTipFooterDescription");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipFooterImageSource for this RibbonCommand.  This is
        ///   the image used in the footer of the ToolTip for a Ribbon control bound to
        ///   this RibbonCommand.
        /// </summary>
        public ImageSource ToolTipFooterImageSource
        {
            get
            {
                return _toolTipFooterImageSource;
            }

            set
            {
                if (value != _toolTipFooterImageSource)
                {
                    _toolTipFooterImageSource = value;
                    this.NotifyPropertyChanged("ToolTipFooterImageSource");
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///   Called to indicate that the indicated property has changed.
        /// </summary>
        /// <param name="info">The name of the property that changed.</param>
        private void NotifyPropertyChanged(string info)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region ICommand Members

        public bool  CanExecute(object parameter)
        {
            if (_actualCommandCache != null)
                return _actualCommandCache.CanExecute(parameter);

            return false ; 
        }

        public bool registerOnce;
        public event EventHandler CanExecuteChanged = null; 

        public void Execute(object parameter)
        {

            if (_actualCommandCache != null)
                _actualCommandCache.Execute (parameter);
         }

        #endregion
    }


    public class NonRoutedRibbonCommandDelegator2 : DependencyObject, ICommand, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        ///   Cached value for the CommandBinding associated with this RibbonCommand.
        /// </summary>
        // private CommandBinding _commandBinding;

        /// <summary>
        ///   Backing store for the LargeImageSource property.
        /// </summary>
        private ImageSource _largeImageSource;

        /// <summary>
        ///   Backing store for the SmallImageSource property.
        /// </summary>
        private ImageSource _smallImageSource;

        /// <summary>
        ///   Backing store for the LabelTitle property.
        /// </summary>
        private string _labelTitle = String.Empty;

        /// <summary>
        ///   Backing store for the LabelDescription property.
        /// </summary>
        private string _labelDescription;

        /// <summary>
        ///   Backing store for the ToolTipTitle property.
        /// </summary>
        private string _toolTipTitle;

        /// <summary>
        ///   Backing store for the ToolTipDescription property.
        /// </summary>
        private string _toolTipDescription;

        /// <summary>
        ///   Backing store for the ToolTipImageSource property.
        /// </summary>
        private ImageSource _toolTipImageSource;

        /// <summary>
        ///   Backing store for the ToolTipFooterTitle property.
        /// </summary>
        private string _toolTipFooterTitle;

        /// <summary>
        ///   Backing store for the ToolTipFooterDescription property.
        /// </summary>
        private string _toolTipFooterDescription;

        /// <summary>
        ///   Backing store for the ToolTipFooterImageSource property.
        /// </summary>
        private ImageSource _toolTipFooterImageSource;


        #endregion

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the RibbonCommand class.
        /// </summary>
        public NonRoutedRibbonCommandDelegator2 ()
        {
        }

        #endregion

        #region Public Events




        public ICommand ActualCommand
        {
            get { return (ICommand)GetValue(ActualCommandProperty); }
            set { SetValue(ActualCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualCommandProperty =
            DependencyProperty.Register("ActualCommand", typeof(ICommand), typeof(NonRoutedRibbonCommandDelegator2),
            new UIPropertyMetadata(new PropertyChangedCallback(OnActualCommandChanged)));

        protected ICommand _actualCommandCache = null;

        static void OnActualCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NonRoutedRibbonCommandDelegator2 instance = d as NonRoutedRibbonCommandDelegator2;
            System.Diagnostics.Debug.Assert(instance != null);
            if (instance != null)
            {
                instance._actualCommandCache = e.NewValue as ICommand;
            }
            CommandManager.InvalidateRequerySuggested();
        }


        /// <summary>
        ///   This event is raised when a property of RibbonCommand changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets the LargeImageSource for this RibbonCommand.  At 96dpi this
        ///   is normally a 32x32 icon.
        /// </summary>
        public ImageSource LargeImageSource
        {
            get
            {
                return _largeImageSource;
            }

            set
            {
                if (value != _largeImageSource)
                {
                    _largeImageSource = value;
                    this.NotifyPropertyChanged("LargeImageSource");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the SmallImageSource for this RibbonCommand.  At 96dpi this
        ///   is normally a 16x16 icon.
        /// </summary>
        public ImageSource SmallImageSource
        {
            get
            {
                return _smallImageSource;
            }

            set
            {
                if (value != _smallImageSource)
                {
                    _smallImageSource = value;
                    this.NotifyPropertyChanged("SmallImageSource");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the LabelTitle for this RibbonCommand.  This is the primary
        ///   label that will be used on a bound Ribbon control.
        /// </summary>
        public string LabelTitle
        {
            get
            {
                return GetValue(LabelTitleProperty) as string;
            }
            set
            {
                //if (value != _labelTitle)
                //{
                //    _labelTitle = value;
                //    this.NotifyPropertyChanged("LabelTitle");
                //}

                SetValue(LabelTitleProperty, value);
            }
        }

        private static readonly DependencyProperty LabelTitleProperty =
            DependencyProperty.Register("LabelTitle", 
            typeof(string), 
            typeof(NonRoutedRibbonCommandDelegator2), 
            new PropertyMetadata(string.Empty));

        /// <summary>0

        ///   Gets or sets the LabelDescription for this RibbonCommand.
        /// </summary>
        public string LabelDescription
        {
            get
            {
                return _labelDescription;
            }

            set
            {
                if (value != _labelDescription)
                {
                    _labelDescription = value;
                    this.NotifyPropertyChanged("LabelDescription");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipTitle for this RibbonCommand.  This is
        ///   used as the main header of the ToolTip for a Ribbon control bound
        ///   to this RibbonCommand.
        /// </summary>
        public string ToolTipTitle
        {
            get
            {
                return _toolTipTitle;
            }

            set
            {
                if (value != _toolTipTitle)
                {
                    _toolTipTitle = value;
                    this.NotifyPropertyChanged("ToolTipTitle");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipDescription for this RibbonCommand.  This is
        ///   used as the main body description of the ToolTip for a Ribbon control
        ///   bound to this RibbonCommand.
        /// </summary>
        public string ToolTipDescription
        {
            get
            {
                return _toolTipDescription;
            }

            set
            {
                if (value != _toolTipDescription)
                {
                    _toolTipDescription = value;
                    this.NotifyPropertyChanged("ToolTipDescription");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipImageSource for this RibbonCommand.  This is
        ///   the main image in the body of the ToolTip for a Ribbon control bound
        ///   to this RibbonCommand.
        /// </summary>
        public ImageSource ToolTipImageSource
        {
            get
            {
                return _toolTipImageSource;
            }

            set
            {
                if (value != _toolTipImageSource)
                {
                    _toolTipImageSource = value;
                    this.NotifyPropertyChanged("ToolTipImageSource");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipFooterTitle for this RibbonCommand.  This is
        ///   the title of the footer of the ToolTip for a Ribbon control bound
        ///   to this RibbonCommand.
        /// </summary>
        public string ToolTipFooterTitle
        {
            get
            {
                return _toolTipFooterTitle;
            }

            set
            {
                if (value != _toolTipFooterTitle)
                {
                    _toolTipFooterTitle = value;
                    this.NotifyPropertyChanged("ToolTipFooterTitle");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipFooterDescription for this RibbonCommand.  This is
        ///   the main description in the footer of the ToolTip for a Ribbon control bound
        ///   to this RibbonCommand.
        /// </summary>
        public string ToolTipFooterDescription
        {
            get
            {
                return _toolTipFooterDescription;
            }

            set
            {
                if (value != _toolTipFooterDescription)
                {
                    _toolTipFooterDescription = value;
                    this.NotifyPropertyChanged("ToolTipFooterDescription");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the ToolTipFooterImageSource for this RibbonCommand.  This is
        ///   the image used in the footer of the ToolTip for a Ribbon control bound to
        ///   this RibbonCommand.
        /// </summary>
        public ImageSource ToolTipFooterImageSource
        {
            get
            {
                return _toolTipFooterImageSource;
            }

            set
            {
                if (value != _toolTipFooterImageSource)
                {
                    _toolTipFooterImageSource = value;
                    this.NotifyPropertyChanged("ToolTipFooterImageSource");
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///   Called to indicate that the indicated property has changed.
        /// </summary>
        /// <param name="info">The name of the property that changed.</param>
        private void NotifyPropertyChanged(string info)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (_actualCommandCache != null)
                return _actualCommandCache.CanExecute(parameter);

            return false;
        }

        public bool registerOnce;
        public event EventHandler CanExecuteChanged          
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    

        public void Execute(object parameter)
        {

            if (_actualCommandCache != null)
                _actualCommandCache.Execute(parameter);
        }

        #endregion
    }

}
