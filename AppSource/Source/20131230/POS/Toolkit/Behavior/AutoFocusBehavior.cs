using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Input;

namespace CPC.Toolkit.Behavior
{
    [Flags]
    public enum FocusBehaviorType
    {
        Attached = 1,
        VisibleChanged = 2,
        EnabledChanged = 4,
        Loaded = 8
    }

    public class AutoFocusBehavior : Behavior<FrameworkElement>
    {
        #region Contructors

        public AutoFocusBehavior()
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

        #region IsFocused

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached(
            "IsFocused", typeof(bool), typeof(AutoFocusBehavior), new FrameworkPropertyMetadata(IsFocusedChanged));

        public static bool GetIsFocused(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (bool)element.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(IsFocusedProperty, value);
        }

        #endregion

        #region UseConditionFocus

        private bool _useConditionFocus;
        public bool UseConditionFocus
        {
            get
            {
                return _useConditionFocus;
            }
            set
            {
                _useConditionFocus = value;
            }
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
        }

        #endregion

        #region Methods

        private void Focus(UIElement element)
        {
            if (element.Focusable)
            {
                if (_useConditionFocus)
                {
                    if (GetIsFocused(this))
                    {
                        //element.Focus();
                        Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Background,
                        new Action(delegate()
                        {
                            Keyboard.Focus(element);
                        }));
                    }
                }
                else
                {

                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Background,
                        new Action(delegate()
                        {
                            Keyboard.Focus(element);
                        }));

                }
            }
        }

        private void FocusNonCondition(UIElement element)
        {
            if (element.Focusable)
            {
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
                FocusNonCondition(element);
            }
        }

        private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = d as FrameworkElement;
            if (fe != null && e.NewValue != null && (bool)e.NewValue)
            {
                //fe.Focus();
                Keyboard.Focus(fe);
            }
        }

        #endregion
    }
}
