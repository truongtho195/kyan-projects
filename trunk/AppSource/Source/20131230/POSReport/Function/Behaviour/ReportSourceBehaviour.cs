using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SAPBusinessObjects.WPF.Viewer;

namespace CPC.POSReport.Function
{
    public static class ReportSourceBehaviour
    {
        public static readonly DependencyProperty ReportSourceProperty =
            DependencyProperty.RegisterAttached(
            "ReportSource",
            typeof(object),
            typeof(ReportSourceBehaviour),
            new PropertyMetadata(ReportSourceChanged)
        );

        private static void ReportSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var crviewer = d as CrystalReportsViewer;
            if (crviewer != null)
            {
                crviewer.ViewerCore.ReportSource = e.NewValue;
            }
        }

        public static void SetReportSource(DependencyObject target, object value)
        {
            target.SetValue(ReportSourceProperty, value);
        }

        public static object GetReportSource(DependencyObject target)
        {
            return target.GetValue(ReportSourceProperty);
        }
    }    
}
