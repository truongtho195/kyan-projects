using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Threading;

namespace CPCToolkitExt.OtherControl
{
    public class TabItemStatusControl : TabItem
    {
        #region Constructor
        public TabItemStatusControl()
        {
            try
            {
                this.IsFocusing = false;
                ///Find resource
                ResourceDictionary dictionary = new ResourceDictionary();
                dictionary.Source = new Uri(@"pack://application:,,,/CPCToolkitExt;component/Theme/Dictionary.xaml");
                this.Resources = dictionary;
                //Set Style for TabItem
                this.Style = this.FindResource("TabItemStatusControlStyle") as Style;
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<<<<TabItemStatusControl>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
            this.Loaded += new RoutedEventHandler(TabItemStatusControl_Loaded);
        }

        #endregion

        #region DependencyProperty

        #region Status
        //
        // Summary:
        //     Gets or sets the status to this TabItem is pressed. This is a
        //     dependency property.
        //
        public int Status
        {
            get { return (int)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Status.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(int), typeof(TabItemStatusControl));

        #endregion

        #region Command
        //
        // Summary:
        //     Gets or sets the command to invoke when this button is pressed. This is a
        //     dependency property.
        //
        // Returns:
        //     A command to invoke when this button is pressed. The default value is null.
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TabItemStatusControl));

        [Category("Common Properties")]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        #endregion

        #region CommandParamater
        //
        // Summary:
        //     Gets or sets the parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property. This is a dependency property.
        //
        // Returns:
        //     Parameter to pass to the System.Windows.Controls.Primitives.ButtonBase.Command
        //     property.
        public static readonly DependencyProperty CommandParamaterProperty =
            DependencyProperty.Register("CommandParamater", typeof(object), typeof(TabItemStatusControl));

        [Category("Common Properties")]
        public object CommandParamater
        {
            get { return (object)GetValue(CommandParamaterProperty); }
            set { SetValue(CommandParamaterProperty, value); }
        }
        #endregion

        #region BackgroundWhenSelected
        // Summary:
        //     Gets or sets a brush that describes the background of a control.
        //
        // Returns:
        //     The brush that is used to fill the background of the control. The default
        //     is System.Windows.Media.Brushes.Transparent.
        public Brush BackgroundWhenSelected
        {
            get { return (Brush)GetValue(BackgroundWhenSelectedProperty); }
            set { SetValue(BackgroundWhenSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundWhenSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundWhenSelectedProperty =
            DependencyProperty.Register("BackgroundWhenSelected", typeof(Brush), typeof(TabItemStatusControl));

        #endregion

        #region VisibilityTabItemByData
        public Visibility VisibilityTabItemByData
        {
            get { return (Visibility)GetValue(VisibilityTabItemByDataProperty); }
            set { SetValue(VisibilityTabItemByDataProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityTabItemByDataProperty =
            DependencyProperty.Register("VisibilityTabItemByData", typeof(Visibility), typeof(TabItemStatusControl), new PropertyMetadata(Visibility.Visible, OnValueVisibilityChanged));

        protected static void OnValueVisibilityChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (source as TabItemStatusControl).SetTabItemControl((Visibility)e.NewValue);
        }
        #endregion

        #endregion

        #region Properties
        protected bool IsFocusing { get; set; }
        #endregion

        #region Event

        protected void TabItemStatusControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ///Set Focus
                if (this.IsSelected)
                {
                    this.IsFocusing = true;
                    this.Focus();
                }

            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<<<<Load TabItemStatusControl>>>>>>>>>>>>>>>" + ex.ToString());
            }

        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            try
            {
                ///Execute Command
                if ((e.Source is TabItemStatusControl)
                   && (e.Source as TabItemStatusControl) != null)
                {
                    this.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    (ThreadStart)delegate
                    {
                        TabItemStatusControl TabItem = (e.Source as TabItemStatusControl);
                        if (TabItem.Command != null)
                            TabItem.Command.Execute(TabItem.CommandParamater);
                    });
                }
                base.OnSelected(e);
            }
            catch (Exception ex)
            {
                Debug.Write("<<<<<<<<<<<<<<<<TabItemStatusControl OnSelected>>>>>>>>>>>>>>>>>>>>>" + ex.ToString());
            }
        }

        #endregion

        #region Method

        #region SetTabItemControl
        protected void SetTabItemControl(Visibility value)
        {
            this.Dispatcher.BeginInvoke(
                            DispatcherPriority.Input,
                            (ThreadStart)delegate
                            {
                                try
                                {
                                    //Set Visibility
                                    this.Visibility = (Visibility)value;
                                    //Set selecteditem for TabControl
                                    //if (this.Visibility == Visibility.Collapsed && this.IsSelected)
                                    //{
                                    var item = (this.Parent as TabControl).Items.OfType<TabItem>().Where(x => (x as TabItem).Visibility == Visibility.Visible);
                                    if (item != null && !item.ToList()[0].IsSelected)
                                        item.ToList()[0].IsSelected = true;
                                    //}
                                }
                                catch (Exception ex)
                                {
                                    Debug.Write("SetTabItemControl" + ex.ToString());
                                }

                            });
        }
        #endregion

        #endregion
    }
}