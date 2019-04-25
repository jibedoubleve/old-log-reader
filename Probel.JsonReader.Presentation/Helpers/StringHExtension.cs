using System.Linq;
using System.Text.RegularExpressions;

namespace Probel.JsonReader.Presentation.Helpers
{
    public static class StringExtension
    {
        public static string ToReadableCString(this string input)
        {
            var pattern = @"\(?.HOST?.=.*?\)";

            var result = Regex.Matches(input, pattern)
                              .Cast<Match>()
                              .FirstOrDefault()?.ToString() ?? "No connection string";

            return result.Replace("(", "").Replace(")", "");
        }
    }
}
