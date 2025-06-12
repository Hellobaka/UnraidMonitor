using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Models
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;
            if (string.IsNullOrWhiteSpace(input))
            {
                return new ValidationResult(false, "内容不能为空");
            }
            return ValidationResult.ValidResult;
        }
    }
}
