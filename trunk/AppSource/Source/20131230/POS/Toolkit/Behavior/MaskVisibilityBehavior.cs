using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using Xceed.Wpf.Toolkit;
using System.Windows.Controls;

namespace CPC.Toolkit.Behavior
{
    public class MaskVisibilityBehavior : Behavior<MaskedTextBox>
    {
        private FrameworkElement _contentPresenter;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += (sender, args) =>
            {
                _contentPresenter = AssociatedObject.Template.FindName("PART_ContentHost", AssociatedObject) as FrameworkElement;
                if (_contentPresenter == null)
                    return;
                AssociatedObject.TextChanged += OnTextChanged;
                AssociatedObject.GotFocus += OnGotFocus;
                AssociatedObject.LostFocus += OnLostFocus;
                UpdateMaskVisibility();
            };
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= OnTextChanged;
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
            base.OnDetaching();
        }

        private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            UpdateMaskVisibility();
        }

        private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            UpdateMaskVisibility();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            UpdateMaskVisibility();
        }

        private void UpdateMaskVisibility()
        {
            _contentPresenter.Visibility = AssociatedObject.MaskedTextProvider.AssignedEditPositionCount > 0 ||
                                            AssociatedObject.IsFocused
                                              ? Visibility.Visible
                                              : Visibility.Hidden;
        }
    }
}
