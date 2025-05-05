using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValid(string value, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return true;
            return Regex.IsMatch(value ?? string.Empty, pattern);
        }
    }
}