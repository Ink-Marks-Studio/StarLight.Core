using System.Globalization;

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
    }
}