using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CustomTextBox;

namespace CPC.POS.Dictionary
{
    partial class TextBoxControlStyle
    {
        private void TextBoxLoaded(object sender, RoutedEventArgs e)
        {
            TextAlignment textAlignmentDefine = TextAlignment.Left;
            TextBox textBox = sender as TextBox;

            if (Enum.IsDefined(typeof(TextAlignment), Convert.ToInt32(Define.CONFIGURATION.TextNumberAlign)))
                textAlignmentDefine = (TextAlignment)Define.CONFIGURATION.TextNumberAlign;

            if (textBox != null)
            {
                textBox.TextAlignment = textAlignmentDefine;
            }

            //TextAlignment textAlign= Enum.Parse(TextAlignment, Define.CONFIGURATION.TextNumberAlign);
            // if (Define.CONFIGURATION.TextNumberAlign == 0)
            //     textBox.TextAlignment = TextAlignment.Right;
            // else if (Define.CONFIGURATION.TextNumberAlign == 1)
            //     textBox.TextAlignment = TextAlignment.Left;
            // else
            //     textBox.TextAlignment = TextAlignment.Center;
        }
    }
}
