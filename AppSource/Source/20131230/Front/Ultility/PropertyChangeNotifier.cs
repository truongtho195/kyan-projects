using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;

namespace CPC.Utility
{
    public sealed class PropertyChangeNotifier : DependencyObject, IDisposable
    {
        #region Class level variables

        private WeakReference mPropertySource;

        #endregion

        #region Constructors

        public PropertyChangeNotifier(DependencyObject propertySource, string path)
            : this(propertySource, new PropertyPath(path))
        {
        }

        public PropertyChangeNotifier(DependencyObject propertySource, DependencyProperty property)
            : this(propertySource, new PropertyPath(property))
        {
        }

        public PropertyChangeNotifier(DependencyObject propertySource, PropertyPath property)
        {
            if (null == propertySource)
                throw new ArgumentNullException("propertySource");
            if (null == property)
                throw new ArgumentNullException("property");
            this.mPropertySource = new WeakReference(propertySource);
            Binding binding = new Binding();
            binding.Path = property;
            binding.Mode = BindingMode.OneWay;
            binding.Source = propertySource;
            BindingOperations.SetBinding(this, ValueProperty, binding);
        }

        #endregion

        #region PropertySource

        public DependencyObject PropertySource
        {
            get
            {
                try
                {
                    return this.mPropertySource.IsAlive
                    ? this.mPropertySource.Target as DependencyObject
                    : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        #endregion

        #region Value

        /// <summary> 
        /// Identifies the <see cref="Value"/> dependency property 
        /// </summary> 
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
            typeof(object), typeof(PropertyChangeNotifier), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PropertyChangeNotifier notifier = (PropertyChangeNotifier)d;
            if (null != notifier.ValueChanged)
                notifier.ValueChanged(notifier, EventArgs.Empty);
        }

        /// <summary> 
        /// Returns/sets the value of the property 
        /// </summary> 
        /// <seealso cref="ValueProperty"/> 
        [Description("Returns/sets the value of the property")]
        [Category("Behavior")]
        [Bindable(true)]
        public object Value
        {
            get
            {
                return (object)this.GetValue(PropertyChangeNotifier.ValueProperty);
            }
            set
            {
                this.SetValue(PropertyChangeNotifier.ValueProperty, value);
            }
        }

        #endregion

        #region Events

        public event EventHandler ValueChanged;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            BindingOperations.ClearBinding(this, ValueProperty);
        }

        #endregion
    }
}
