using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace CPC.Helper
{
    public class RowDataInfoValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            List<string> errors = new List<string>();

            foreach (var item in bindingGroup.Items)
            {
                // Aggregate errors.
                IDataErrorInfo dataErrorInfo = item as IDataErrorInfo;
                if (dataErrorInfo != null)
                {
                    string msg = dataErrorInfo.Error;
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        errors.Add(msg);
                    }
                }
            }

            if (errors.Count > 0)
            {
                return new ValidationResult(false, string.Join(Environment.NewLine, errors));
            }

            return ValidationResult.ValidResult;
        }
    }
}
