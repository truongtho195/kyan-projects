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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Threading;
using System.Threading;
using Tims.Model;

namespace CPC.Control
{
    public class ComboBoxSalary : ComboBox
    {

        #region Constructor
        public ComboBoxSalary()
        {
            this.Loaded += new RoutedEventHandler(ComboBoxSalary_Loaded);
            this.ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator_StatusChanged);
        }
        #endregion

        #region Events of Control
        /// <summary>
        /// Event will execute when you click to ComboBox this first.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (this.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                foreach (var item in ItemsSource)
                {
                    SalaryModel itemModel = item as SalaryModel;
                    if (itemModel != null && (this.SelectedValue == null || int.Parse(this.SelectedValue.ToString()) != itemModel.Value) && itemModel.IsSelected)
                    {
                        ComboBoxItem comboBoxItem = this.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem;
                        comboBoxItem.IsEnabled = false;
                        comboBoxItem.Visibility = Visibility.Collapsed;
                        //Console.WriteLine("ItemContainerGenerator" + "\n");
                    }
                }
                this.ItemContainerGenerator.StatusChanged -= new EventHandler(ItemContainerGenerator_StatusChanged);
            }
        }

        /// <summary>
        /// Event will execute when the ComboBox loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxSalary_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded && this.ItemsSource != null && this.SelectedItem != null)
            {
                (this.SelectedItem as SalaryModel).IsSelected = true;
            }
        }

        /// <summary>
        /// Event will execute when the ComboBox's DropDown is opening.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnDropDownOpened(EventArgs e)
        {
            if (this.IsLoaded)
            {
                if (this.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    foreach (var item in ItemsSource)
                    {
                        SalaryModel itemModel = item as SalaryModel;
                        ComboBoxItem comboBoxItem = this.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem;
                        if (itemModel != null && (this.SelectedValue == null || int.Parse(this.SelectedValue.ToString()) != itemModel.Value) && itemModel.IsSelected)
                        {
                            comboBoxItem.IsEnabled = false;
                            comboBoxItem.Visibility = Visibility.Collapsed;
                            //Console.WriteLine("OnDropDownOpened" + "\n");
                        }
                        else
                        {
                            comboBoxItem.IsEnabled = true;
                            comboBoxItem.Visibility = Visibility.Visible;
                            //Console.WriteLine("Reload Data" + "\n");
                        }

                    }
                }
            }
            base.OnDropDownOpened(e);
        }

        /// <summary>
        /// Event will execute when the ComboBox's Selection is changing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (this.IsLoaded && !this.IsDeleted)
            {
                if (e.RemovedItems.Count > 0)
                {
                    (e.RemovedItems[0] as SalaryModel).IsSelected = false;
                }
                if (e.AddedItems.Count > 0)
                {
                    (e.AddedItems[0] as SalaryModel).IsSelected = true;
                }
            }
            base.OnSelectionChanged(e);
        }
        #endregion

        #region DependencyProperty
        /// <summary>
        /// Get or set value for IsDeleted when you delete an item in DataGrid.
        /// </summary>
        public bool IsDeleted
        {
            get { return (bool)GetValue(IsDeletedProperty); }
            set { SetValue(IsDeletedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDeleted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDeletedProperty =
            DependencyProperty.Register("IsDeleted", typeof(bool), typeof(ComboBoxSalary), new PropertyMetadata(false, OnValueChanged));
        #endregion

        #region Methods
        protected static void OnValueChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && bool.Parse(e.NewValue.ToString()))
            {
                (source as ComboBoxSalary).ReloadSelectedItem();
            }
        }

        /// <summary>
        /// This function will progress when value of IsDeleted change.
        /// </summary>
        public void ReloadSelectedItem()
        {
            if (this.SelectedItem != null)
                (this.SelectedItem as SalaryModel).IsSelected = false;
        }
        #endregion

    }
}
