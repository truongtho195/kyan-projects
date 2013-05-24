using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Windows;
using CPC.POS.Database;
using CPC.POS.View;
using CPC.Toolkit.Base;

namespace CPC.POS.ViewModel
{
    class SplashScreenViewModel : ViewModelBase
    {
        #region Defines

        private BackgroundWorker _bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
        private SplashScreenView _splashScreenView;

        #endregion

        #region Properties

        private bool _canConnectDB;
        /// <summary>
        /// Gets or sets the CanConnectDB.
        /// </summary>
        public bool CanConnectDB
        {
            get { return _canConnectDB; }
            set
            {
                if (_canConnectDB != value)
                {
                    _canConnectDB = value;
                    OnPropertyChanged(() => CanConnectDB);
                }
            }
        }

        private string _statusMessage;
        /// <summary>
        /// Gets or sets the StatusMessage.
        /// </summary>
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(() => StatusMessage);
                }
            }
        }

        #endregion

        #region Constructors

        public SplashScreenViewModel(SplashScreenView splashScreenView)
        {
            _bgWorker.DoWork += (sender, e) =>
            {
                _splashScreenView = splashScreenView;

                StatusMessage = "Checking connection...";

                CanConnectDB = CheckConnectionDB();
                _bgWorker.ReportProgress(100);
            };
            _bgWorker.ProgressChanged += (sender, e) =>
            {
                if (!CanConnectDB)
                {
                    StatusMessage = "Connection failed!";
                    MessageBoxResult msgResult = MessageBox.Show(_splashScreenView, StatusMessage, "POS", MessageBoxButton.OK);
                }
                _splashScreenView.DialogResult = CanConnectDB;
            };
            _bgWorker.RunWorkerAsync();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check connection to database
        /// </summary>
        /// <returns></returns>
        private static bool CheckConnectionDB()
        {
            bool result = false;

            POSEntities objectContext = new POSEntities(ConfigurationManager.ConnectionStrings["POSDBEntities"].ConnectionString);

            try
            {
                // Check connection
                objectContext.Connection.Open();

                // Connection completed
                result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                // Enforce close connnection
                if (objectContext.Connection.State.Equals(ConnectionState.Open))
                    objectContext.Connection.Close();
            }

            return result;
        }

        #endregion
    }
}
