using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Crystal Report Utils
/// </summary>
static class ReportUtils
{
    public static void HideTabControl(this CrystalDecisions.Windows.Forms.CrystalReportViewer reportViewer)
    {
        foreach (System.Windows.Forms.Control control in reportViewer.Controls)
        {
            if (control is CrystalDecisions.Windows.Forms.PageView)
            {
                System.Windows.Forms.TabControl tab = (System.Windows.Forms.TabControl)((CrystalDecisions.Windows.Forms.PageView)control).Controls[0];
                tab.ItemSize = new System.Drawing.Size(0, 1);
                tab.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
                tab.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            }
        }
    }
}