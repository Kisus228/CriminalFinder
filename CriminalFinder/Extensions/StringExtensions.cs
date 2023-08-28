using System.Text;
using Spectre.Console;

namespace CriminalChecker.Extensions;

public static class StringExtensions
{
    public static string WithMarkup(this string? value, Color foregroundColor, string? link = null)
    {
        if (string.IsNullOrEmpty(value))
            return "";
        
        var markup = new StringBuilder(foregroundColor.ToMarkup());

        if (link is not null)
            markup.Append($" link={link}");
        return $"[{markup}]{value}[/]";
    }
}