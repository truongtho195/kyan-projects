using System;
using System.Windows;
using System.Windows.Interop;

namespace CPC.POS
{
    /// <summary>
    /// Interaction logic for LockScreenView.xaml
    /// </summary>
    public partial class LockScreenView : Window
    {
        public LockScreenView()
        {
            this.InitializeComponent();

            // Insert code required on object creation below this point.
            this.SourceInitialized += new System.EventHandler(LockScreenView_SourceInitialized);
        }

        #region Prevent move window
        
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;

        protected IntPtr WndProcess(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        protected void LockScreenView_SourceInitialized(object sender, System.EventArgs e)
        {
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(this);
            HwndSource hwndSource = HwndSource.FromHwnd(windowInteropHelper.Handle);
            hwndSource.AddHook(WndProcess);
        }

        #endregion
    }
}