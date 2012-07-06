using System;
using MessageBoxControl;
using MVVMHelper.Properties;

namespace MVVMHelper.Services
{
    public class MessageBoxService : IMessageBoxService
    {

        #region Methods can return a value to the caller

        public bool? ShowQuestion(string message, params string[] param)
        {
            MessageBoxResultCustom result = MessageBoxCustom.Show(String.Format(message, param), Resources.QuestionCaption, MessageBoxButtonCustom.YesNoCancel,
                MessageBoxImageCustom.Question);

            if (result == MessageBoxResultCustom.Yes) { return true; }
            else if (result == MessageBoxResultCustom.No) { return false; }

            return null;
        }

        public bool ShowYesNoQuestion(string message, params string[] param)
        {
            MessageBoxResultCustom result = MessageBoxCustom.Show(String.Format(message, param), Resources.QuestionCaption, MessageBoxButtonCustom.YesNo,
                MessageBoxImageCustom.Question);

            return result == MessageBoxResultCustom.Yes;
        }

        public bool ShowOKCancelQuestion(string message, params string[] param)
        {
            MessageBoxResultCustom result = MessageBoxCustom.Show(String.Format(message, param), Resources.QuestionCaption, MessageBoxButtonCustom.OKCancel,
                MessageBoxImageCustom.Question);

            return result == MessageBoxResultCustom.OK;
        }

        #endregion

        #region Method parameters

        public void Show(string message, params string[] param)
        {
            MessageBoxResultCustom result = MessageBoxCustom.Show(String.Format(message, param), Resources.WarningCaption, MessageBoxButtonCustom.OK,
                MessageBoxImageCustom.Warning);
        }

        public void ShowAlert(string message, params string[] param)
        {
            MessageBoxResultCustom result = MessageBoxCustom.Show(String.Format(message, param), Resources.AlertCaption, MessageBoxButtonCustom.OK,
                MessageBoxImageCustom.Error);
        }

        public void ShowWarning(string message, params string[] param)
        {
            MessageBoxResultCustom result = MessageBoxCustom.Show(String.Format(message, param), Resources.WarningCaption, MessageBoxButtonCustom.OK,
                MessageBoxImageCustom.Warning);
        }

        public void ShowSuccessful(string message, params string[] param)
        {
            MessageBoxResultCustom result = MessageBoxCustom.Show(String.Format(message, param), Resources.SuccessfulCaption, MessageBoxButtonCustom.OK,
                MessageBoxImageCustom.Information);
        }

        public void ShowExclamation(string message, params string[] param)
        {
            MessageBoxResultCustom result = MessageBoxCustom.Show(String.Format(message, param), Resources.AlertCaption, MessageBoxButtonCustom.OK,
                MessageBoxImageCustom.Exclamation);
        }

        #endregion

    }
}
