using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;

namespace Wosk
{
    public class KeyboardModel : System.ComponentModel.INotifyPropertyChanged
    {
        public void PressAndRelease(string key)
        {
            VK keyCode;
            try
            {
                keyCode = (VK)Enum.Parse(typeof(VK), key);
                SendKey(keyCode);
            }
            catch  
			{
			// TODO: log exception
			}

            ReleaseStickyKeys();
        }

        public void ReleaseStickyKeys()
        { 
            //TODO: any key can be configured to be "sticky". check the "pressedKeys" collection/dictionary
            Shift = false;
            Alt = false;
            Ctrl = false;
            RightShift = false;
            RightAlt = false;
            RightCtrl = false;
            Win = false;
        }

        public void PressAndHold(string key)
        {
            PressAndHold((VK)Enum.Parse(typeof(VK), key));
        }

		/// <summary>
		/// For "sticky" keys, that need to be pressed for long time, typically until another key is pressed and released.
		/// </summary>
		/// <param name="keyCode"></param>
        public void PressAndHold(VK keyCode)
        {
            switch (keyCode)
            { 
                case VK.LSHIFT:
                case VK.RSHIFT:
                case VK.SHIFT: this.Shift = !this.Shift; break;

                case VK.MENU: Alt= !Alt; break;
                    
                case VK.CONTROL:
                case VK.RCONTROL:
                case VK.LCONTROL: this.Ctrl = !this.Ctrl; break;

                case VK.LWIN: this.Win = !this.Win; break;
            }

            //TODO: consider implementing collection of pressed keys
            // instead of handling all cases manually

            //VK keyCode = (VK)Enum.Parse(typeof(VK), key);
            //if (!pressedKeys.Keys.Contains(keyCode)) pressedKeys.Add(keyCode, false);
            //if (null==pressedKeys[keyCode]) ReleaseSticky(keyCode); else PressSticky(keyCode);
            //pressedKeys[keyCode] = !pressedKeys[keyCode];
        }

        public void PressKey(VK keyCode)
        {
            uint intReturn = 0;
            INPUT structInput;
            structInput = new INPUT();
            structInput.type = (uint)1;
            structInput.ki.wScan = 0;
            structInput.ki.time = 0;
            structInput.ki.dwFlags = 0;
            structInput.ki.dwExtraInfo = 0;
            // Key down the actual key-code

            structInput.ki.wVk = (ushort)keyCode; //VK.SHIFT etc.
			intReturn = NativeWin32.SendInput(1, ref structInput, Marshal.SizeOf(structInput));
        }

        public void ReleaseKey(VK keyCode)
        {
            uint intReturn = 0;
            INPUT structInput;
            structInput = new INPUT();
            structInput.type = (uint)1;
            structInput.ki.wScan = 0;
            structInput.ki.time = 0;
            structInput.ki.dwFlags = 0;
            structInput.ki.dwExtraInfo = 0;

            // Key up the actual key-code
            structInput.ki.dwFlags = NativeWin32.KEYEVENTF_KEYUP;
            structInput.ki.wVk = (ushort)keyCode;// (ushort)NativeWin32.VK.SNAPSHOT;//vk;
            intReturn = NativeWin32.SendInput((uint)1, ref structInput, Marshal.SizeOf(structInput));
        }

        public void SendKey(VK keyCode)
        {
			PressKey(keyCode);
			ReleaseKey(keyCode);
        }

		private bool _LeftShift;
		private bool _RightShift;

		public bool LeftShift
		{
			get { return _LeftShift; }
			set
			{
				if (_LeftShift != value)
				{
					_LeftShift = value;
					if (_LeftShift) PressKey(VK.LSHIFT); else ReleaseKey(VK.LSHIFT);
					NotifyPropertyChanged("LeftShift");
				}
			}
		}

		public bool RightShift
		{
			get { return _RightShift; }
			set
			{
				if (_RightShift != value)
				{
					_RightShift = value;
					if (_RightShift) PressKey(VK.RSHIFT); else ReleaseKey(VK.RSHIFT);
					NotifyPropertyChanged("RightShift");
				}
			}
		}
		
        public bool Shift
        {
            get { return (LeftShift|| RightShift); }
            set
            {
                if (LeftShift != value) // since we don't know which Shift user wants, we have to assume one.
                {
                    LeftShift = value;
                    if (Shift) PressKey(VK.SHIFT); else ReleaseKey(VK.SHIFT);
                    NotifyPropertyChanged("Shift");
                }
            }
        }

        //public bool CapsLock { get; set; }
		private bool _LeftAlt;
		private bool _RightAlt;

		public bool LeftAlt
		{
			get { return _LeftAlt; }
			set
			{
				if (_LeftAlt != value)
				{
					_LeftAlt = value;
					if (_LeftAlt) PressKey(VK.MENU); else ReleaseKey(VK.MENU);
					NotifyPropertyChanged("LeftAlt");
				}
			}
		}

		public bool RightAlt
		{
			get { return _RightAlt; }
			set
			{
				if (_RightAlt != value)
				{
					_RightAlt = value;
					if (_RightAlt) PressKey(VK.RMENU); else ReleaseKey(VK.RMENU);
					NotifyPropertyChanged("RightAlt");
				}
			}
		}


        public bool Alt
        {
            get { return (LeftAlt || RightAlt); }
            set
            {
                if (LeftAlt != value)
                {
                    LeftAlt = value;
                    if (LeftAlt) PressKey(VK.MENU); else ReleaseKey(VK.MENU);
                    NotifyPropertyChanged("Alt");
                }
            }
        }
        private bool LeftCtrl { get; set; }
        private bool RightCtrl { get; set; }
        public bool Ctrl
        {
            get { return (LeftCtrl || RightCtrl); }
            set
            {
                if (LeftCtrl != value)
                {
                    LeftCtrl = value;
                    if (LeftCtrl) PressKey(VK.CONTROL); else ReleaseKey(VK.CONTROL);
                    NotifyPropertyChanged("Ctrl");

                }
            }
        }
        private bool LeftWin { get; set; }
        public bool Win
        {
            get { return LeftWin; }
            set
            {
                if (LeftWin != value)
                {
                    LeftWin = value;
                    if (LeftWin) PressKey(VK.LWIN); else ReleaseKey(VK.LWIN);
                    NotifyPropertyChanged("Win");
                }
            }
        }

        //public bool Menu { get; set; }
        //public bool NumLock { get; set; }
        //public bool FLock { get; set; }
        //public bool Fn { get; set; }
        //public bool ScrollLock { get; set; }

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
