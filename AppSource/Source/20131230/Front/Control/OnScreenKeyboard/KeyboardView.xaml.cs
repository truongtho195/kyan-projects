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
using Wosk;
using CPC.POS;

namespace CPC.Control
{
    /// <summary>
    /// Interaction logic for LaptopKeyboardView.xaml
    /// </summary>
    public partial class KeyboardView : Window
    {

		WoskViewModel viewModel;

        private HookProc cbtCallbackDelegate;
        private IntPtr hook;


        //protected VirtualKeyboard virtualKeyboard;
        public KeyboardView()
        {
            InitializeComponent();
			viewModel = (WoskViewModel)App.Current.FindResource("virtualKeyboard");
            this.cbtCallbackDelegate = new HookProc(CbtCallbackFunction);
            hook = NativeWin32.SetWindowsHookEx(5 /* wh_cbt */, this.cbtCallbackDelegate, IntPtr.Zero, AppDomain.GetCurrentThreadId());

            try
			{
				//ComboBoxItem cbi = (ComboBoxItem)(lstKeyboardSkins.Items[0]);
				//LoadKeyboardLayout(cbi.Content.ToString());
			}
			catch 
			{ //TODO: log ex
			}
        }


        private void TransparentWindow_Loaded(object sender, RoutedEventArgs e)
        {
			SetWindowStyle();
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
			source.AddHook(new HwndSourceHook(WndProc));

            LoadKeyboardLayout("IPhoneKeyboard.xaml");


        }

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
			if (msg == UnsafeNativeMethods.WM_MOVING)
			{
				System.Windows.Forms.Message m = new System.Windows.Forms.Message();
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

		private void SetWindowStyle()
		{
			//HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
			//source.AddHook(new HwndSourceHook(WndProc));
			// Get this window's handle
			IntPtr HWND = new WindowInteropHelper(this).Handle;

			//int WS_EX_TOOLWINDOW = 0x00000080;
			//int GWL_ID = (-12);
			//int GWL_STYLE = (-16);
            int GWL_EXSTYLE = (-20); 
			int WS_EX_NOACTIVATE = 0x08000000;

			//int WS_EX_DLGMODALFRAME = 0x0001;
			//uint SWP_NOSIZE = 0x0001;
			//uint SWP_NOMOVE = 0x0002;
			//uint SWP_NOZORDER = 0x0004;
			//uint SWP_FRAMECHANGED = 0x0020;
			//uint WM_SETICON = 0x0080;
			//long OLD = 
		//	NativeWin32.GetWindowLong(HWND, GWL_EXSTYLE);
			//            MessageBox.Show(OLD.ToString());
			// SetWindowLong(HWND, GWL_EXSTYLE, (IntPtr)(OLD | WS_EX_TOOLWINDOW));
            NativeWin32.SetWindowLong(HWND, GWL_EXSTYLE, (IntPtr)WS_EX_NOACTIVATE);//SetWindowPos(HWND, IntPtr.Zero, 0, 0, 0, 0, (uint)SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
			//    MessageBox.Show((OLD | 0x8000000).ToString());



		}

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
			this.DragMove();
		}


        private void Button_Hide(object sender, RoutedEventArgs e)
        {
            // it is better to have custom handler (like this) for hiding, 
            // instead of just mapping regular button/key (default is NumLock)
            // because hotkey can be sometimes broken.
            viewModel.KeyboardModel.ReleaseStickyKeys();
            //WindowState = WindowState.Minimized;
            viewModel.Hide();
        }

        private void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
            //Application.Current.Shutdown();
            //Environment.Exit(0); //even after shutdown there was application button visible in taskbar
        }

        private void TransparentWindow_Closed(object sender, EventArgs e)
        {
            viewModel.Dispose();
        }

		public void LoadKeyboardLayout(string filename)
		{
			////Load control from XML file
			System.IO.FileStream fileStream = new System.IO.FileStream(filename, System.IO.FileMode.Open);
			UserControl dependencyObject = System.Windows.Markup.XamlReader.Load(fileStream) as UserControl;
			pnlMain.Content = dependencyObject;
			pnlMain.DataContext = viewModel;
			this.Width = dependencyObject.Width; //.DesiredSize.Width;
			this.Height = dependencyObject.Height;
		}

		private void lstKeyboardSkins_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBoxItem cbi = (ComboBoxItem)(((ComboBox)sender).SelectedItem);
			LoadKeyboardLayout(cbi.Content.ToString());
		}

		//private void lstHotkeys_SelectionChanged(object sender, SelectionChangedEventArgs e)
		//{
		//    System.Windows.Forms.Keys cbi = (System.Windows.Forms.Keys)(((ComboBox)sender).SelectedItem);
		//    RegisterHotkey(cbi);
		//    //System.Windows.Forms.Keys key = (System.Windows.Forms.Keys)Enum.Parse(typeof(System.Windows.Forms.Keys), cbi.Content.ToString());

		//}
        public void Dock(Dock dock)
        {
            WindowState = WindowState.Normal;
            if (dock != System.Windows.Controls.Dock.Bottom) throw new NotImplementedException("Only Dock.Bottom is supported currently");
            //dock to bottom
            Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            if (Height > System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height) Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left;
            Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top + System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - Height;
        }

        public void Maximize()
        {
            WindowState = WindowState.Maximized;
        }

        public void Minimize()
        {
            WindowState = WindowState.Minimized;
        }

		private void btnHide_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}


        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            Maximize();
        }

        private void btnDock_Click(object sender, RoutedEventArgs e)
        {
            Dock(System.Windows.Controls.Dock.Bottom);
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            Minimize();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = e.Source as MenuItem;
            if (null != menu) switch (menu.Header.ToString())
                {
                    case "Exit": this.Close(); break;
                    case "Hide Panel":
                        HideInputPanel();
                        menu.Header = "Show Panel";
                        break;
                    case "Show Panel":
                        ShowInputPanel();
                        menu.Header = "Hide Panel";
                        break;
                }
        }

        private void HideInputPanel()
        {
         //   this.Hide();
        }

        private void ShowInputPanel()
        {
       //     this.Show();
        }

        private void LayoutMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = e.Source as MenuItem;
            if (null != menu)
            {
                LoadKeyboardLayout(menu.Header.ToString());
                ShowInputPanel();
            }
        }

    }
}
