using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Input;

namespace CPC.Toolkit.Behavior
{
    public class OneAutoFocusBehavior : Behavior<FrameworkElement>
    {
        #region Fields

        private static bool _hasFocused = false;

        #endregion

        #region Contructors

        public OneAutoFocusBehavior()
        {
            FocusBehaviorType = FocusBehaviorType.Attached | FocusBehaviorType.VisibleChanged | FocusBehaviorType.EnabledChanged | FocusBehaviorType.Loaded;
        }

        #endregion

        #region Properties

        #region FocusBehaviorType

        public FocusBehaviorType FocusBehaviorType
        {
            get;
            set;
        }

        #endregion

        #endregion

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();

            if ((FocusBehaviorType & FocusBehaviorType.Attached) == FocusBehaviorType.Attached)
            {
                Focus(AssociatedObject);
            }

            if ((FocusBehaviorType & FocusBehaviorType.VisibleChanged) == FocusBehaviorType.VisibleChanged)
            {
                AssociatedObject.IsVisibleChanged += AssociatedObjectIsVisibleChanged;
            }

            if ((FocusBehaviorType & FocusBehaviorType.EnabledChanged) == FocusBehaviorType.EnabledChanged)
            {
                AssociatedObject.IsEnabledChanged += AssociatedObjectIsEnabledChanged;
            }

            if ((FocusBehaviorType & FocusBehaviorType.Loaded) == FocusBehaviorType.Loaded)
            {
                AssociatedObject.Loaded += AssociatedObjectLoaded;
            }

            AssociatedObject.Unloaded += AssociatedObjectUnloaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if ((FocusBehaviorType & FocusBehaviorType.VisibleChanged) == FocusBehaviorType.VisibleChanged)
            {
                AssociatedObject.IsVisibleChanged -= AssociatedObjectIsVisibleChanged;
            }

            if ((FocusBehaviorType & FocusBehaviorType.EnabledChanged) == FocusBehaviorType.EnabledChanged)
            {
                AssociatedObject.IsEnabledChanged -= AssociatedObjectIsEnabledChanged;
            }

            if ((FocusBehaviorType & FocusBehaviorType.Loaded) == FocusBehaviorType.Loaded)
            {
                AssociatedObject.Loaded -= AssociatedObjectLoaded;
            }

            AssociatedObject.Unloaded -= AssociatedObjectUnloaded;
        }

        #endregion

        #region Methods

        private void Focus(UIElement element)
        {
            if (element.Focusable && !_hasFocused)
            {
                _hasFocused = true;
                Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Background,
                        new Action(delegate()
                        {
                            Keyboard.Focus(element);
                        }));
            }
        }

        #endregion

        #region Events

        private void AssociatedObjectIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = sender as UIElement;
            if (element != null && element.IsVisible)
            {
                Focus(element);
            }
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            UIElement element = sender as UIElement;
            if (element != null && element.IsVisible)
            {
                Focus(element);
            }
        }

        private void AssociatedObjectIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = sender as UIElement;
            if (element != null && element.IsEnabled)
            {
                Focus(element);
            }
        }

        private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = d as FrameworkElement;
            if (fe != null && e.NewValue != null && (bool)e.NewValue && !_hasFocused)
            {
                Keyboard.Focus(fe);
                _hasFocused = true;
            }
        }


        private void AssociatedObjectUnloaded(object sender, RoutedEventArgs e)
        {
            _hasFocused = false;
        }

        #endregion
    }
}
