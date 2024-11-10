using System.Globalization;
using System.Text.RegularExpressions;

namespace StarLight_Core.Utilities;

public class TextUtil
{
    public static string ToTitleCase(string input)
    {
        var cultureInfo = CultureInfo.CurrentCulture;
        var textInfo = cultureInfo.TextInfo;
        return textInfo.ToTitleCase(input);
    }

    public static bool IsValidMinecraftId(string input)
    {
        var pattern = @"^[a-zA-Z0-9_]{1,24}$";

        var regex = new Regex(pattern);
        return regex.IsMatch(input);
    }
}