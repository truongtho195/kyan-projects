using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CPC.Helper
{
    class SetValueControlHelper
    {
        public static void InsertTimeStamp(CPCToolkitExt.TextBoxControl.TextBox remarkTextBox)
        {
            int currentCursor = remarkTextBox.SelectionStart;
            string currentDate = " " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
            remarkTextBox.Text = remarkTextBox.Text.Insert(currentCursor, currentDate);
            remarkTextBox.SelectionStart = currentCursor + currentDate.Length;
            remarkTextBox.Focus();
        }
    }
}
