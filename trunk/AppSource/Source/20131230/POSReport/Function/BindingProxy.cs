using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CPC.POSReport.Function
{
    public class BindingProxy : Freezable
    {
        #region -Overide of Freezable-
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
        #endregion

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
}
