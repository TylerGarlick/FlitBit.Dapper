using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Dapper
{
    public static class StringExtensions
    {
        /// <summary>
        /// Fixes the specified name to be camel cased. No singularization.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string ToCamelCase(this string name)
        {
            if (string.IsNullOrEmpty(name)) return "a" + Guid.NewGuid().ToString("N");

            var endsWithId = Regex.IsMatch(name, "[a-z0-9 _]{1}(?<Id>ID)$");

            name = MakePascalCase(name); //reuse this

            if (endsWithId)
            {
                //ends with a capital "ID" in an otherwise non-capitalized word
                name = name.Substring(0, name.Length - 2) + "Id";
            }

            //remove all spaces
            name = Regex.Replace(name, @"[^\w]+", string.Empty);

            if (Char.IsUpper(name[0]))
            {
                name = char.ToLowerInvariant(name[0]) +
                    (name.Length > 1 ? name.Substring(1) : string.Empty);
            }

            return name;
        }

        private static string MakePascalCase(string name)
        {
            //make underscores into spaces, plus other odd punctuation
            name = name.Replace('_', ' ').Replace('$', ' ').Replace('#', ' ');

            //if it's all uppercase
            if (Regex.IsMatch(name, @"^[A-Z0-9 ]+$"))
            {
                //lowercase it
                name = CultureInfo.InvariantCulture.TextInfo.ToLower(name);
            }

            //if it's mixed case with no spaces, it's already pascal case
            if (name.IndexOf(' ') == -1 && !Regex.IsMatch(name, @"^[a-z0-9]+$"))
            {
                return name;
            }

            //titlecase it (words that are uppered are preserved)
            name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name);

            return name;
        }
    }
}
