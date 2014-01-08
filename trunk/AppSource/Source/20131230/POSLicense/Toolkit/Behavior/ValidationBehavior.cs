using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace CPC.Toolkit.Behavior
{
    /// <summary>
    /// A simple behavior that can transfer the number of validation error with exceptions
    /// to a ViewModel which supports the INotifyValidationException interface
    /// </summary>
    public class ValidationBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(OnValidationError));
        }

        private void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            //ViewModelBase viewModelBase = AssociatedObject.DataContext as ViewModelBase;
            //if (viewModelBase == null)
            //    return;

            //if (e.Action == ValidationErrorEventAction.Added)
            //    viewModelBase.Errors.Add(e.Error);
            //else
            //    viewModelBase.Errors.Remove(e.Error);

            //viewModelBase.IsValid = !viewModelBase.Errors.Any();
        }
    }
}
