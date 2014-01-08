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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace CPC.Control
{
    public enum ButtonType
    {
        None = 0,
        Add = 1,
        Remove = 2
    }

    public class ButtonItem : Button
    {

        #region Dependency Properties

        public ButtonType Key
        {
            get { return (ButtonType)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(ButtonType), typeof(ButtonItem), new UIPropertyMetadata(ButtonType.None));

        #endregion

    }
}
