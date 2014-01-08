using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace CPC.POS.ViewModel.Synchronization
{
    public class ClientNotifiedEventArgs : EventArgs, INotifyPropertyChanged
    {
        private readonly object message;
        private readonly object content;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message from server.</param>
        public ClientNotifiedEventArgs(string message, object content)
        {
            this.message = message;
            this.content = content;
        }

        public ClientNotifiedEventArgs(string message, object content, bool result)
        {
            this.message = message;
            this.content = content;
            this.Result = result;
        }

        public ClientNotifiedEventArgs(string message, object content, bool result, int intResult)
        {
            this.message = message;
            this.content = content;
            this.Result = result;
            this.IntResult = intResult;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public object Message { get { return message; } }
        public object Content { get { return content; } }

        private bool result;
        /// <summary>
        /// Gets or sets the StoreCollection.
        /// </summary>
        public bool Result
        {
            get { return result; }
            set
            {
                if (result != value)
                {
                    result = value;
                    OnPropertyChanged("Result");
                }
            }
        }

        private int intResult;
        /// <summary>
        /// Gets or sets the IntResult.
        /// </summary>
        public int IntResult
        {
            get { return intResult; }
            set
            {
                if (intResult != value)
                {
                    intResult = value;
                    OnPropertyChanged("IntResult");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
