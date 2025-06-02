using System.Globalization;
using System.Text;
using SerializerLib.Enums;

namespace SerializerLib;

public static class Utils
{
    public static string Caseify(string original, CasePolicy casePolicy)
    {
        return casePolicy switch
        {
            CasePolicy.CamelCase => CamelCaseify(original),
            CasePolicy.SnakeCase => SnakeCaseify(original),
            _ => throw new ArgumentOutOfRangeException(nameof(casePolicy), casePolicy,
                $"Unexpected value of CasePolicy. Expected values: {string.Join(", ", Enum.GetValues<CasePolicy>())}")
        };
    }

    private static string CamelCaseify(string original)
    {
        // Assumes that property names are PascalCase. If not then well... Unlucky...
        return string.IsNullOrEmpty(original) ? original : char.ToLower(original[0]) + original[1..];
    }

    private static string SnakeCaseify(string original)
    {
        if (string.IsNullOrEmpty(original))
        {
            return original;
        }

        var builder = new StringBuilder(original.Length + Math.Min(2, original.Length / 5));
        var previousCategory = default(UnicodeCategory?);

        for (var currentIndex = 0; currentIndex < original.Length; currentIndex++)
        {
            var currentChar = original[currentIndex];
            if (currentChar == '_')
            {
                builder.Append('_');
                previousCategory = null;
                continue;
            }

            var currentCategory = char.GetUnicodeCategory(currentChar);
            switch (currentCategory)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                    if (previousCategory == UnicodeCategory.SpaceSeparator ||
                        previousCategory == UnicodeCategory.LowercaseLetter ||
                        previousCategory != UnicodeCategory.DecimalDigitNumber &&
                        previousCategory != null &&
                        currentIndex > 0 &&
                        currentIndex + 1 < original.Length &&
                        char.IsLower(original[currentIndex + 1]))
                    {
                        builder.Append('_');
                    }

                    currentChar = char.ToLower(currentChar);
                    break;

                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.DecimalDigitNumber:
                    if (previousCategory == UnicodeCategory.SpaceSeparator)
                    {
                        builder.Append('_');
                    }

                    break;

                default:
                    if (previousCategory != null)
                    {
                        previousCategory = UnicodeCategory.SpaceSeparator;
                    }

                    continue;
            }

            builder.Append(currentChar);
            previousCategory = currentCategory;
        }

        return builder.ToString();
    }
}