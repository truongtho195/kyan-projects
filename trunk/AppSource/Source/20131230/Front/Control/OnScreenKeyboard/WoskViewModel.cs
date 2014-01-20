using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;

namespace Wosk
{
    public class WoskViewModel : System.ComponentModel.INotifyPropertyChanged
	{

		private KeyboardModel _KeyboardModel;
		public KeyboardModel KeyboardModel
		{
			get
			{
				if (null == _KeyboardModel) _KeyboardModel = new KeyboardModel();
				return _KeyboardModel;
			}
		}

        private ManagedWinapi.Hotkey hotkey;
        public ManagedWinapi.Hotkey Hotkey
        {
            get
            {
                if (null == hotkey) hotkey = new ManagedWinapi.Hotkey();
                return hotkey;
            }
        }



		private DelegateCommand<string> _PressAndHold;

		private DelegateCommand<string> _PressAndRelease;

		public ICommand PressAndHold
		{
			get
			{
				if (_PressAndHold == null)
				{
					_PressAndHold = new DelegateCommand<string>(delegate(string key)
					{
						KeyboardModel.PressAndHold(key);
					});
				}
				return _PressAndHold;
			}
		}

		public ICommand PressAndRelease
		{
			get
			{
				if (_PressAndRelease == null)
				{
					_PressAndRelease = new DelegateCommand<string>(delegate(string key)
					{
                        KeyboardModel.PressAndRelease(key);
                     //   if (null != player) { player.Play(); }
					});
				}
				return _PressAndRelease;
			}
		}

        public void RegisterHotkey(System.Windows.Forms.Keys keyCode)
        {
            Hotkey.HotkeyPressed += new EventHandler(Hotkey_HotkeyPressed);
            System.Windows.Forms.Keys oldKey = hotkey.KeyCode;
            if (keyCode != oldKey) //prevent infinite recursive loop
            {
                try
                {
                    hotkey.Enabled = false;
                    hotkey.KeyCode = keyCode;
                    hotkey.Enabled = true;
                }
                catch (ManagedWinapi.HotkeyAlreadyInUseException)
                {
                    MessageBox.Show("Could not register hotkey (already in use).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    RegisterHotkey(oldKey);
                }
            }
        }

        void Hotkey_HotkeyPressed(object sender, EventArgs e)
        {
            if (this.IsVisible) //(WindowState != WindowState.Minimized)
            {
                KeyboardModel.ReleaseStickyKeys();    //sticky keys cause confusion when they are hidden but still active
                this.Hide();
            }
            else
            {
                this.Show();
            }
        }

        public void Hide()
        {
            this.IsVisible = false;
        }

        public void Show()
        {
            this.IsVisible = true;
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    NotifyPropertyChanged("IsVisible");
                    _isVisible = value;
                }
            }
        }

		public WoskViewModel()
		{

            
		}

        public void Dispose()
        {
            KeyboardModel.ReleaseStickyKeys();
            if (null != Hotkey) Hotkey.Dispose();
        }



        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

	}
}
