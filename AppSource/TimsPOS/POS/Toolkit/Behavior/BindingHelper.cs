﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CPC.Toolkit.Behavior
{
   public class BindingHelper : Freezable
    {
       protected override Freezable CreateInstanceCore()
       {
           return new BindingHelper();
       }
       public object Data
       {
           get { return (object)GetValue(DataProperty); }
           set { SetValue(DataProperty, value); }
       }

       // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
       public static readonly DependencyProperty DataProperty =
           DependencyProperty.Register("Data", typeof(object), typeof(BindingHelper), new UIPropertyMetadata(null));
    }
}
