using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace LiteUI.Common.Extensions
{
    [PublicAPI]
    public static class StringExtension
    {
        public static string Truncate(this string value, int maxLength, string? replace = null)
        {
            if (value.Length <= maxLength) {
                return value;
            }
            if (string.IsNullOrEmpty(replace)) {
                return value[..maxLength];
            }
            return value[..(maxLength - replace.Length)] + replace;
        }

        public static string Capitalize(this string self)
        {
            if (self.Length == 0) {
                return self;
            }
            return char.ToUpperInvariant(self[0]) + (self.Length > 1 ? self[1..] : "");
        }

        public static string Uncapitalize(this string self)
        {
            if (self.Length == 0) {
                return self;
            }
            return char.ToLowerInvariant(self[0]) + (self.Length > 1 ? self[1..] : "");
        }

        public static string CamelCaseToUnderscore(this string self)
        {
            return string.Concat(self.Select((x, i) => i > 0 && char.IsUpper(x)
                                                               ? "_" + char.ToUpperInvariant(x)
                                                               : char.ToUpperInvariant(x).ToString())
                                     .ToArray());
        }

        public static string UnderscoreToCamelCase(this string self)
        {
            StringBuilder builder = new();
            string str = self.ToLowerInvariant();
            for (int i = 0; i < str.Length; i++) {
                char c = str[i];
                if (i == 0 && c != '_') {
                    builder.Append(char.ToUpperInvariant(c));
                } else if (c == '_' && i != str.Length - 1) {
                    builder.Append(char.ToUpperInvariant(str[i + 1]));
                    i++;
                } else {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }
    }
}
