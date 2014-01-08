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
            //mark index that cursor will be;
            string specialChar = "{$*^*$}";
            string currentDate = string.Empty;

            currentDate = " " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine + specialChar;

            //Using clipboard paste text cause insert text not correct current position if in textbox has break line
            string clipboardText = Clipboard.GetText();
            Clipboard.SetText(currentDate);
            remarkTextBox.Paste();

            int indexSpecialChar = remarkTextBox.Text.IndexOf(specialChar);

            //Remove Special Char
            remarkTextBox.Text = remarkTextBox.Text.Replace(specialChar, "");
            remarkTextBox.Focus();
            //Set Cursor
            remarkTextBox.SelectionStart = indexSpecialChar+1;
            int currentLine = remarkTextBox.GetLineIndexFromCharacterIndex(remarkTextBox.CaretIndex);
            remarkTextBox.ScrollToLine(currentLine);

            //Store Clipboard & Set again
            if (!string.IsNullOrWhiteSpace(clipboardText))
                Clipboard.SetText(clipboardText);
            else
                Clipboard.SetText(string.Empty);
        }
    }
}
