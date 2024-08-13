using System.Globalization;
using System.Text.RegularExpressions;

namespace StarLight_Core.Utilities
{
    public class TextUtil
    {
        public static string ToTitleCase(string input)
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(input);
        }
        
        public static bool IsValidMinecraftId(string input)
        {
            string pattern = @"^[a-zA-Z0-9_]{1,24}$";
            
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }
    }
}