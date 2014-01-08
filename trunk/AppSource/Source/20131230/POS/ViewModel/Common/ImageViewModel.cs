using CPC.Toolkit.Base;

namespace CPC.POS.ViewModel
{
    public class ImageViewModel : ViewModelBase
    {
        #region Contructor

        public ImageViewModel()
        {

        }

        public ImageViewModel(string filePath)
        {
            _filePath = filePath;
        }

        #endregion

        #region Properties

        #region FilePath

        private string _filePath;
        /// <summary>
        /// Gets or sets file path.
        /// </summary>
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged(() => FilePath);
                }
            }
        }

        #endregion

        #endregion
    }
}