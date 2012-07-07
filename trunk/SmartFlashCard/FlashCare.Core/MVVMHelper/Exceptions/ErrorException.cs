using System;
using MVVMHelper.Properties;

namespace MVVMHelper.Common
{
    public enum ErrorType
    {
        Required,
        MustChoose,
        NotAllowNull,
        GreaterThan,
        SmallerThan,
        Invalid,
        Custom,
        CorrectFormat,
        Equal,
        AbsoluteValue
    }

    public static class ErrorException
    {
        public static string Message(ErrorType errorType, string value)
        {
            string message = string.Empty;
            switch (errorType)
            {
                case ErrorType.Required:
                    message = String.Format(Resources.Validate_RequiredInputText, value);
                    break;
                case ErrorType.MustChoose:
                    message = String.Format(Resources.Validate_RequiredChooseItem, value);
                    break;
                case ErrorType.NotAllowNull:
                    message = String.Format(Resources.Validate_NotNullField, value);
                    break;
                case ErrorType.Invalid:
                    message = String.Format(Resources.Validate_InvalidField, value);
                    break;
                default:
                    message = value;
                    break;
            }

            return message;
        }

        public static string Message(ErrorType errorType, string value1, string value2)
        {
            string message = string.Empty;
            switch (errorType)
            {
                case ErrorType.GreaterThan:
                    message = String.Format(Resources.Validate_GreaterThanField, value1, value2);
                    break;
                case ErrorType.SmallerThan:
                    message = String.Format(Resources.Validate_SmallerThan, value1, value2);
                    break;
                case ErrorType.Equal:
                    message = string.Format(Resources.Validate_Equal, value1, value2);
                    break;
                case ErrorType.AbsoluteValue:
                    message = string.Format(Resources.Validate_AbsoluteField, value1, value2);
                    break;
                case ErrorType.CorrectFormat:
                    message = string.Format(Resources.Validate_CorrectFormatField, value1, value2);
                    break;
                case ErrorType.Custom:
                    message = String.Format(value1, value2);
                    break;
            }

            return message;
        }

        public static string Message(ErrorType errorType, string value1, string value2, string value3)
        {
            string message = string.Empty;
            switch (errorType)
            {
                case ErrorType.Custom:
                    message = String.Format(value1, value2, value3);
                    break;
                default:
                    break;
            }
            return message;
        }
    }

    //public class MessageErrors
    //{
    //    public static string SpecialCharacter = LanguageDictionary.Current.Translate<string>("exceptSpecialCharacter", "Message");
    //    public static string FormatEmail = LanguageDictionary.Current.Translate<string>("formatEmail", "Message");
    //    public static string FormatWebsite = LanguageDictionary.Current.Translate<string>("formatWebsite", "Message");
    //}
}
