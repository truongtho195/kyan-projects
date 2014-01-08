using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using BioCodeAnalyzerUltimate.Service;

//http://blog.vuscode.com/malovicn/archive/2010/11/07/naked-mvvm-simplest-possible-mvvm-approach.aspx
namespace Tims.Toolkit.Behavior
{
    //public class AutoWireUpViewModelBehavior : Behavior<UIElement> 
    //{ 
    //    protected override void OnAttached() 
    //    { 
    //        base.OnAttached(); 
    //        var view = (FrameworkElement)this.AssociatedObject; 
    //        var viewModelName = string.Format("{0}Model", view.GetType().FullName); 
    //        var viewModel = ServiceLocator.IoC.Resolve<object>(viewModelName); 
    //        view.DataContext = viewModel; 
    //    } 
    //} 
}
