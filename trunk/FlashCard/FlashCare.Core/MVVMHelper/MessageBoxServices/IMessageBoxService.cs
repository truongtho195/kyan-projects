using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMHelper.Services
{
    public interface IMessageBoxService
    {

        // Methods can return a value to the caller
        
        bool? ShowQuestion(string message, params string[] param);

        bool ShowYesNoQuestion(string message, params string[] param);

        bool ShowOKCancelQuestion(string message, params string[] param);

        // Methods parameters

        void Show(string message, params string[] param);

        void ShowAlert(string message, params string[] param);

        void ShowWarning(string message, params string[] param);

        void ShowSuccessful(string message, params string[] param);

        void ShowExclamation(string message, params string[] param);

    }
}
