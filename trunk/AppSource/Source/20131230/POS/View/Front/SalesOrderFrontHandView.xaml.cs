using System;
using System.Collections.Generic;
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
using CPC.POS.Model;
using System.Collections.ObjectModel;

namespace CPC.POS.View
{
    /// <summary>
    /// Interaction logic for SalesOrderFrontHandView.xaml
    /// </summary>
    public partial class SalesOrderFrontHandView
    {
        public SalesOrderFrontHandView()
		{
            
			this.InitializeComponent();
            //ObservableCollection<base_SaleOrderDetailModel> list = new ObservableCollection<base_SaleOrderDetailModel>();
            //for (int i = 0; i < 1000; i++)
            //{
            //    list.Add(new base_SaleOrderDetailModel()
            //    {
            //        ItemCode =i.ToString("000000#"),
            //        ItemName ="Item Name"+ i.ToString(),
            //        Quantity=1,
            //        SalePrice =0,
            //        SubTotal =1
            //    });
            //}
            //dtgrdOrder.ItemsSource = list;
            //btnTop.Click += (sender, e) =>
            //{
            //    OnScrollUp(dtgrdOrder);
            //};

            //btnBottom.Click += (sender, e) =>
            //{
            //    OnScrollDown(dtgrdOrder);
            //};
            
		}





        #region Ultility

        /// <summary>
        /// Get Scroll Viewer of Element
        /// </summary>
        /// <param name="frameworkElement"></param>
        /// <returns></returns>
        private ScrollViewer ScrollViewerFromFrameworkElement(FrameworkElement frameworkElement)
        {
            if (VisualTreeHelper.GetChildrenCount(frameworkElement) == 0) return null;

            FrameworkElement child = VisualTreeHelper.GetChild(frameworkElement, 0) as FrameworkElement;

            if (child == null) return null;

            if (child is ScrollViewer)
            {
                return (ScrollViewer)child;
            }

            return ScrollViewerFromFrameworkElement(child);
        }

        /// <summary>
        /// Scroll Up
        /// </summary>
        /// <param name="sender"></param>
        private void OnScrollUp(object sender)
        {
            var scrollViwer = ScrollViewerFromFrameworkElement(sender as FrameworkElement) as ScrollViewer;

            if (scrollViwer != null)
            {
                // Physical Scrolling by Offset
                scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset - 3);
            }
        }

        /// <summary>
        /// Scroll Down
        /// </summary>
        /// <param name="sender"></param>
        private void OnScrollDown(object sender)
        {
            var scrollViwer = ScrollViewerFromFrameworkElement(sender as FrameworkElement) as ScrollViewer;

            if (scrollViwer != null)
            {
                scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset + 3);
            }
        }


        #endregion
    }
}