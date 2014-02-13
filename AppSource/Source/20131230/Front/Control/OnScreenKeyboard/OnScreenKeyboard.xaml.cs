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
using System.Windows.Interop;
using CPC.POS;
using System.Runtime.InteropServices;

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for OnScreenKeyboard.xaml
    /// </summary>
    public partial class OnScreenKeyboard : Window
    {

        public OnScreenKeyboard()
        {
            this.Focusable = false;
            InitializeComponent();
            this.cbtCallbackDelegate = new HookProc(CbtCallbackFunction);
            hook = NativeWin32.SetWindowsHookEx(5 /* wh_cbt */, this.cbtCallbackDelegate, IntPtr.Zero, AppDomain.GetCurrentThreadId());
            //viewModel = new TouchKeyboardViewModel();
            
        }

        void OnScreenKeyboard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //this.DragMove();
        }


        private HookProc cbtCallbackDelegate;
        private IntPtr hook;
        TouchKeyboardViewModel viewModel;


        private int CbtCallbackFunction(int code, IntPtr wParam, IntPtr lParam)
        {
            switch (code)
            {
                case 5: /* HCBT_ACTIVATE */
                    NativeWin32.UnhookWindowsHookEx(hook);
                    return 1; /* prevent windows from handling activate */
            }
            //return the value returned by CallNextHookEx
            return NativeWin32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int MA_NOACTIVATE = 0x0003;
        /// <summary>
        /// We have to handle WM_MOVING messages manually, because we use special type of window 
        /// that ignores them. Our window is not typical (regarding focus, visibility, etc.)
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            System.Windows.Forms.Message m = new System.Windows.Forms.Message();
            if (msg == UnsafeNativeMethods.WM_MOVING)
            {
                m.HWnd = hwnd;
                m.Msg = msg;
                m.WParam = wParam;
                m.LParam = lParam;
                m.Result = IntPtr.Zero;
                UnsafeNativeMethods.ReDrawWindow(m);
                handled = true;
            }
            return IntPtr.Zero;
        }

        //private void SetWindowStyle()
        //{
        //    // Get this window's handle
        //    IntPtr HWND = new WindowInteropHelper(this).Handle;

        //    int GWL_EXSTYLE = (-20);
        //    int WS_EX_NOACTIVATE = 0x08000000;

        //    NativeWin32.SetWindowLong(HWND, GWL_EXSTYLE, (IntPtr)WS_EX_NOACTIVATE);//SetWindowPos(HWND, IntPtr.Zero, 0, 0, 0, 0, (uint)SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        //    //    MessageBox.Show((OLD | 0x8000000).ToString());
        //}

    }


}
