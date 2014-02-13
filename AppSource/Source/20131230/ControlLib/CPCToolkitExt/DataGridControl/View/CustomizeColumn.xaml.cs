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
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Windows.Markup;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;

namespace CPCToolkitExt.DataGridControl.View
{
    /// <summary>
    /// Interaction logic for ScheduleManagement.xaml
    /// </summary>
    public partial class CustomizeColumnView : Window
    {
        #region Ctor
        public CustomizeColumnView()
        {
            this.InitializeComponent();
            this.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive == true);
            this.btnRight.Click += new RoutedEventHandler(BtnRight_Click);
            this.btnLeft.Click += new RoutedEventHandler(BtnLeft_Click);
            this.btnDown.Click += new RoutedEventHandler(BtnDown_Click);
            this.btnUP.Click += new RoutedEventHandler(BtnUP_Click);
            this.btnOK.Click += new RoutedEventHandler(BtnOK_Click);
            this.btnCancel.Click += new RoutedEventHandler(BtnCancel_Click);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(TopBarLine_MouseLeftButtonDown);
        }
        #endregion

        #region Fields
        public List<KeyValuePair<object, RowModel>> AvailableColumns = new List<KeyValuePair<object, RowModel>>();
        public List<KeyValuePair<object, RowModel>> ChosenColumns = new List<KeyValuePair<object, RowModel>>();
        private List<KeyValuePair<object, RowModel>> AvailableColumnsClone;
        private List<KeyValuePair<object, RowModel>> ChosenColumnsClone;
        public bool IsColumnsAddition { get; set; }
        public bool IsChangePossitionColumn { get; set; }
        public bool IsCancel { get; set; }
        #endregion

        #region Methods
        public void SetItemsSource()
        {
            this.lbAvailableColumn.ItemsSource = this.AvailableColumns;
            this.lbChosenColumn.ItemsSource = this.ChosenColumns;
            this.CloneData();
        }

        private void CloneData()
        {
            this.AvailableColumnsClone = new List<KeyValuePair<object, RowModel>>();
            foreach (var item in this.AvailableColumns)
                this.AvailableColumnsClone.Add(item);
            this.ChosenColumnsClone = new List<KeyValuePair<object, RowModel>>();
            foreach (var item in this.ChosenColumns)
                this.ChosenColumnsClone.Add(item);
        }

        public void ChangeClose()
        {
            this.IsChangePossitionColumn = false;
            this.IsColumnsAddition = false;
        }
        #endregion

        #region Override Events
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            this.Hide();
        }
        #endregion

        #region Events
        public void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.IsChangePossitionColumn || this.IsColumnsAddition)
                {
                    this.AvailableColumns = new List<KeyValuePair<object, RowModel>>();
                    foreach (var item in this.AvailableColumnsClone)
                        this.AvailableColumns.Add(item);
                    this.ChosenColumns = new List<KeyValuePair<object, RowModel>>();
                    foreach (var item in this.ChosenColumnsClone)
                        this.ChosenColumns.Add(item);
                    this.lbAvailableColumn.ItemsSource = this.AvailableColumns;
                    this.lbChosenColumn.ItemsSource = this.ChosenColumns;
                }
                this.IsChangePossitionColumn = false;
                this.IsColumnsAddition = false;
                this.IsCancel = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), " Customize Column");
            }

        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.CloneData();
                this.IsCancel = false;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), " Customize Column");
            }

        }

        private void BtnUP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.lbChosenColumn.SelectedIndex > 0 && this.lbChosenColumn.SelectedValue != null)
                {
                    this.IsChangePossitionColumn = true;
                    KeyValuePair<object, RowModel> CurrentSelected = this.ChosenColumns.SingleOrDefault(x => x.Key == this.lbChosenColumn.SelectedValue);
                    var CurrentIndex = this.lbChosenColumn.SelectedIndex;
                    KeyValuePair<object, RowModel> NextSelected = this.ChosenColumns.ElementAt(CurrentIndex - 1);
                    if (NextSelected.Key != null)
                    {
                        this.lbChosenColumn.ItemsSource = null;
                        this.ChosenColumns.Remove(CurrentSelected);
                        this.ChosenColumns.Insert(CurrentIndex - 1, CurrentSelected);
                        this.lbChosenColumn.ItemsSource = this.ChosenColumns;
                        this.lbChosenColumn.SelectedValue = CurrentSelected.Key;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), " Customize Column");
            }

        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.lbChosenColumn.SelectedIndex >= 0 && this.lbChosenColumn.SelectedValue != null && this.lbChosenColumn.SelectedIndex < this.ChosenColumns.Count - 1)
                {
                    this.IsChangePossitionColumn = true;
                    KeyValuePair<object, RowModel> CurrentSelected = this.ChosenColumns.SingleOrDefault(x => x.Key == this.lbChosenColumn.SelectedValue);
                    var CurrentIndex = this.lbChosenColumn.SelectedIndex;
                    this.lbChosenColumn.ItemsSource = null;
                    this.ChosenColumns.Remove(CurrentSelected);
                    this.ChosenColumns.Insert(CurrentIndex + 1, CurrentSelected);
                    this.lbChosenColumn.ItemsSource = this.ChosenColumns;
                    this.lbChosenColumn.SelectedValue = CurrentSelected.Key;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), " Customize Column");
            }

        }

        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.lbChosenColumn.SelectedIndex >= 0 && this.lbChosenColumn.SelectedValue != null)
                {
                    this.IsColumnsAddition = true;
                    int CurrentIndex = 0;
                    if ((this.lbChosenColumn.SelectedIndex == 0 && this.lbChosenColumn.Items.Count > 1))
                        CurrentIndex = 0;
                    else
                        CurrentIndex = this.lbChosenColumn.SelectedIndex - 1;
                    //To remove item in Available List.
                    KeyValuePair<object, RowModel> CurrentSelected = this.ChosenColumns.SingleOrDefault(x => x.Key == this.lbChosenColumn.SelectedValue);
                    this.ChosenColumns.Remove(CurrentSelected);
                    this.lbChosenColumn.ItemsSource = null;
                    this.lbChosenColumn.ItemsSource = this.ChosenColumns;
                    //To add item to Chosen List.
                    this.lbAvailableColumn.ItemsSource = null;
                    this.AvailableColumns.Add(CurrentSelected);
                    this.lbAvailableColumn.ItemsSource = this.AvailableColumns;
                    //To set index of ListBox.
                    this.lbChosenColumn.SelectedIndex = CurrentIndex;
                    if (this.lbChosenColumn.SelectedItem != null && this.lbChosenColumn.SelectedValue == null)
                        this.lbChosenColumn.SelectedIndex += 1;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), " Customize Column");
            }

        }

        private void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.lbAvailableColumn.SelectedIndex >= 0 && this.lbAvailableColumn.SelectedValue != null)
                {
                    this.IsColumnsAddition = true;
                    int CurrentIndex = 0;
                    if (this.lbAvailableColumn.SelectedIndex == 0 && this.lbAvailableColumn.Items.Count > 1)
                        CurrentIndex = 0;
                    else
                        CurrentIndex = this.lbAvailableColumn.SelectedIndex - 1;
                    //To remove item in Available List.
                    KeyValuePair<object, RowModel> CurrentSelected = this.AvailableColumns.SingleOrDefault(x => x.Key == this.lbAvailableColumn.SelectedValue);
                    this.AvailableColumns.Remove(CurrentSelected);
                    this.lbAvailableColumn.ItemsSource = null;
                    this.lbAvailableColumn.ItemsSource = this.AvailableColumns;
                    //To add item to Chosen List.
                    this.lbChosenColumn.ItemsSource = null;
                    this.ChosenColumns.Add(CurrentSelected);
                    this.lbChosenColumn.ItemsSource = this.ChosenColumns;
                    this.lbAvailableColumn.SelectedIndex = CurrentIndex;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), " Customize Column");
            }

        }

        private void TopBarLine_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        #endregion
    }
}