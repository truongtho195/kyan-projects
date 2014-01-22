using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using DPFP.Error;

namespace CPC.POS.ViewModel
{
    public class RecordFingerprintViewModel : ViewModelBase, DPFP.Capture.EventHandler
    {
        #region Fields
        public delegate void OnTemplateEventHandler(DPFP.Template template);
        public event OnTemplateEventHandler OnTemplate;
        private DPFP.Template Template;
        private DPFP.Capture.Capture Capturer;
        private DPFP.Processing.Enrollment Enroller;
        private FrameworkElement ViewCore;
        DispatcherTimer timer;

        #endregion

        #region Constructors
        public RecordFingerprintViewModel()
            : base()
        {
            _ownerViewModel = this;
        }
        public RecordFingerprintViewModel(FrameworkElement view)
        {

        }
        #endregion

        #region Properties
        private bool _isLeft = false;
        public bool IsLeft
        {
            get { return _isLeft; }
            set
            {
                if (_isLeft != value)
                {
                    _isLeft = value;
                    OnPropertyChanged(() => IsLeft);
                }
            }
        }

        private bool _isEdit = false;
        public bool IsEdit
        {
            get { return _isEdit; }
            set
            {
                if (_isEdit != value)
                {
                    _isEdit = value;
                    OnPropertyChanged(() => IsEdit);
                }
            }
        }

        private int _fingerId = 0;
        public int FingerID
        {
            get { return _fingerId; }
            set
            {
                if (_fingerId != value)
                {
                    _fingerId = value;
                    IsEdit = true;
                    OnPropertyChanged(() => FingerID);
                }
            }
        }

        private bool _matchone = false;
        public bool MatchOne
        {
            get { return _matchone; }
            set
            {
                if (_matchone != value)
                {
                    _matchone = value;
                    OnPropertyChanged(() => MatchOne);
                }
            }
        }

        private bool _matchtwo = false;
        public bool MatchTwo
        {
            get { return _matchtwo; }
            set
            {
                if (_matchtwo != value)
                {
                    _matchtwo = value;
                    OnPropertyChanged(() => MatchTwo);
                }
            }
        }

        private int _currentMatch = 0;
        public int CurrentMatch
        {
            get { return _currentMatch; }
            set
            {
                if (_currentMatch != value)
                {
                    _currentMatch = value;
                    OnPropertyChanged(() => CurrentMatch);
                }
            }
        }

        private string _statusToolTip = String.Empty;
        public string StatusToolTip
        {
            get { return _statusToolTip; }
            set
            {
                if (_statusToolTip != value)
                {
                    _statusToolTip = value;
                    OnPropertyChanged(() => StatusToolTip);
                }
            }
        }

        private bool _showToolTip = false;
        public bool ShowToolTip
        {
            get { return _showToolTip; }
            set
            {
                if (_showToolTip != value)
                {
                    _showToolTip = value;
                    OnPropertyChanged(() => ShowToolTip);
                }
            }
        }

        private byte[] _temp;
        public byte[] Temp
        {
            get { return _temp; }
            set
            {
                if (_temp != value)
                {
                    _temp = value;
                    OnPropertyChanged(() => Temp);
                }
            }
        }

        private bool _isError = false;
        public bool IsError
        {
            get { return _isError; }
            set
            {
                if (_isError != value)
                {
                    _isError = value;
                    OnPropertyChanged(() => IsError);
                }
            }
        }
        #endregion

        #region Command Properties

        #region OK Command

        private ICommand _okCommand;
        public ICommand OKCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand<object>(OKExecute, CanOKExecute);
                }
                return _okCommand;
            }
        }
        /// Save Execute Configuration
        /// </summary>
        private void OKExecute(object param)
        {
            System.Windows.Window window = FindOwnerWindow(this);
            window.DialogResult = true;

            //(ViewCore as RecordFingerprintView).Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            //{
            //    RecordFingerprintView view = (ViewCore as RecordFingerprintView);
            //    view.DialogResult = true;
            //    view.Close();
            //}));
        }

        private bool CanOKExecute(object param)
        {
            return (IsEdit && _fingerId == 0 && Temp!=null) || Template != null;
        }

        #endregion

        #region Load Command

        private ICommand _loadCommand;
        public ICommand LoadCommand
        {
            get
            {
                if (_loadCommand == null)
                {
                    _loadCommand = new RelayCommand<object>(LoadExecute, CanLoadExecute);
                }
                return _loadCommand;
            }
        }

        /// <summary>
        /// Save Execute Configuration
        /// </summary>
        private void LoadExecute(object param)
        {

            try
            {
                System.Windows.Window window = FindOwnerWindow(this);
                Enroller = new DPFP.Processing.Enrollment();			// Create an enrollment.
                DPFP.FeatureSet features = new DPFP.FeatureSet();
                //popupContainer = window as PopupContainer ;
                //recordFingerprintView = param as RecordFingerprintView;
                ViewCore = window;
                UpdateStatus();
                //Load data
                Init();
                Start();
                this.OnTemplate += new OnTemplateEventHandler(RecordFingerprintViewModel_OnTemplate);
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += new EventHandler(timer_Tick);
            }
            catch (DllNotFoundException ex)
            {
                _log4net.Error(ex);
                this.IsError = true;
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message.ToString(), "Warning");
                //recordFingerprintView.Close();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        private bool CanLoadExecute(object param)
        {
            return true;
        }


        #endregion

        #region Cancel Command

        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand<object>(CancelExecute);
                }
                return _cancelCommand;
            }
        }

        private void CancelExecute(object param)
        {
            try
            {
                System.Windows.Window window = FindOwnerWindow(this);
                Stop();
                window.DialogResult = false;
            }
            catch (DllNotFoundException ex)
            {
                _log4net.Error(ex);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }
        #endregion

        #endregion

        #region EventHandler Members

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            MakeReport("The fingerprint sample was captured.");
            SetPrompt("Scan the same fingerprint again.");
            Process(Sample);
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The finger was removed from the fingerprint reader.");
        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was touched.");
        }

        void timer_Tick(object sender, EventArgs e)
        {
            CurrentMatch = 0;
            timer.Stop();
        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was connected.");
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was disconnected.");
        }

        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback)
        {
            bool b = CaptureFeedback == DPFP.Capture.CaptureFeedback.Good;
            if (b)
            {
                MakeReport("The quality of the fingerprint sample is good.");
            }
            else
            {
                MakeReport("The quality of the fingerprint sample is poor.");
            }
        }

        #endregion

        #region Events
        void RecordFingerprintViewModel_OnTemplate(DPFP.Template template)
        {
            ViewCore.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
            Template = template;
            // VerifyButton.Enabled = SaveButton.Enabled = (Template != null);
            if (Template != null)
                base.ShowMessageBox("Successful", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                base.ShowMessageBox("Fail", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        void view_Closed(object sender, EventArgs e)
        {
            Stop();
        }

        void view_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            Start();
        }
        #endregion

        #region Methods
        protected virtual void Init()
        {
            try
            {
                Capturer = new DPFP.Capture.Capture();				// Create a capture operation.

                if (null != Capturer)
                    Capturer.EventHandler = this;					// Subscribe for capturing events.
                else
                    SetPrompt("Can't initiate capture operation!");
            }
            catch
            {
                base.ShowMessageBox("Can't initiate capture operation!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void Start()
        {
            if (null != Capturer)
            {
                try
                {
                    Capturer.StartCapture();
                    SetPrompt("Using the fingerprint reader, scan your fingerprint.");
                }
                catch
                {
                    SetPrompt("Can't initiate capture!");
                }
            }
        }

        protected void Stop()
        {
            try
            {
                if (null != Capturer)
                {
                    try
                    {
                        Capturer.StopCapture();
                    }
                    catch(Exception ex)
                    {
                        _log4net.Error(ex);
                        SetPrompt("Can't terminate capture!");
                    }
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message.ToString(), "Warning");
            }
        }

        protected void Process(DPFP.Sample Sample)
        {
            try
            {
                if (_fingerId == 0)
                {
                    ViewCore.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        base.ShowMessageBox("Please choose finger", "FingerPrint", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }));

                    return;
                }

                // DrawPicture(ConvertSampleToBitmap(Sample));

                // Process the sample and create a feature set for the enrollment purpose.
                DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Enrollment);

                // Check quality of the sample and add to enroller if it's good
                if (features != null)
                    try
                    {
                        MakeReport("The fingerprint feature set was created.");
                        Enroller.AddFeatures(features);		// Add feature set to template.
                    }
                    catch (SDKException ex)
                    {
                        _log4net.Error(ex);
                    }
                    finally
                    {
                        this.UpdateStatus();

                        // Check if template has been created.
                        switch (Enroller.TemplateStatus)
                        {
                            case DPFP.Processing.Enrollment.Status.Ready:	// report success and stop capturing
                                OnTemplate(Enroller.Template);
                                SetPrompt("Click Close, and then click Fingerprint Verification.");
                                Stop();
                                this.Temp = Enroller.Template.Bytes;
                                break;

                            case DPFP.Processing.Enrollment.Status.Failed:	// report failure and restart capturing
                                MatchOne = false;
                                MatchTwo = false;
                                CurrentMatch = 2;
                                Enroller.Clear();

                                Stop();
                                UpdateStatus();
                                OnTemplate(null);
                                Start();
                                break;
                        }
                    }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        private void SetPrompt(string p)
        {
         

            ViewCore.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                //(ViewCore as RecordFingerprintView).tblPrompt.Text = p;
            }));
        }

        private void UpdateStatus()
        {
            // Show number of samples needed.
            SetStatus(String.Format("Fingerprint samples needed: {0}", Enroller.FeaturesNeeded / 2));
        }

        protected void SetStatus(string status)
        {

            ViewCore.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                StatusToolTip = status;
                ShowToolTip = true;
                //(ViewCore as RecordFingerprintView).ToolTip = status;
            }));
        }

        private void MakeReport(string p)
        {
            
            ViewCore.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                StatusToolTip = p;
                ShowToolTip = true;
                //(ViewCore as RecordFingerprintView).tblStatusText.Text += Environment.NewLine + p;
            }));
        }

        //protected Bitmap ConvertSampleToBitmap(DPFP.Sample Sample)
        //{
        //    DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion();	// Create a sample convertor.
        //    Bitmap bitmap = null;												            // TODO: the size doesn't matter
        //    Convertor.ConvertToPicture(Sample, ref bitmap);									// TODO: return bitmap as a result
        //    return bitmap;
        //}

        //private void DrawPicture(Bitmap bitmap)
        //{
        //    (ViewCore as RecordFingerprintView).Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
        //    {
        //        //(ViewCore as RecordFingerprintView).img.Source = ConvertBitmap.ToBitmapSource(bitmap);	// fit the image into the picture box
        //    }));
        //}

        protected DPFP.FeatureSet ExtractFeatures(DPFP.Sample Sample, DPFP.Processing.DataPurpose Purpose)
        {

            DPFP.FeatureSet features = null;
            try
            {
                DPFP.Processing.FeatureExtraction Extractor = new DPFP.Processing.FeatureExtraction();	// Create a feature extractor
                DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
                features = new DPFP.FeatureSet();
                Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref features);			// TODO: return features as a result?
                Enroller.AddFeatures(features);

                uint i = Enroller.FeaturesNeeded / 2;
                bool b = (feedback == DPFP.Capture.CaptureFeedback.Good);

                if (i == 1)
                    MatchOne = b;
                else if (i == 0)
                    MatchTwo = b;

                if (!b)
                {
                    features = null;
                }
                else
                {
                    CurrentMatch = 1;
                    timer.Start();
                }

            }
            catch
            {
                CurrentMatch = 2;
                timer.Start();
            }
            finally
            {

            }
            return features;
        }
        #endregion
    }

    public static class ConvertBitmap
    {
        /// <summary>   
        /// Converts a 
        /// <see cref="System.Drawing.Bitmap"/> into a WPF 
        /// <see cref="BitmapSource"/>.    
        /// </summary>     
        /// <remarks>Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.</remarks>     
        /// <param name="source">The source bitmap.</param>    
        /// <returns>A BitmapSource</returns> 
        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc = null;
            var hBitmap = source.GetHbitmap();
            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }
            return bitSrc;
        }
    }

    /// <summary> 
    /// FxCop requires all Marshalled functions to be in a class called NativeMethods. 
    /// </summary> 
    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);
    }
}