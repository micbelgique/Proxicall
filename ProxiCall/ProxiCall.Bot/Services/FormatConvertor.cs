using System;
using System.Globalization;

namespace ProxiCall.Bot.Services
{
    public class FormatConvertor
    {
        public static DateTime TimexToDateTime(string timex)
        {
            if(string.IsNullOrEmpty(timex) || timex.Length < 9)
            {
                return DateTime.MinValue;
            }

            var year = timex.Substring(0, 4);
            var month = timex.Substring(5, 2);
            var day = timex.Substring(8, 2);

            if (year.Contains('X'))
            {
                var beforeEndOfYear = int.Parse(month) <= 12 && int.Parse(day) <= 31;
                var betweenNowAndEndOfYear = beforeEndOfYear && (int.Parse(month) > DateTime.Now.Month || int.Parse(month) == DateTime.Now.Month && int.Parse(day) >= DateTime.Now.Day);
                if (betweenNowAndEndOfYear)
                {
                    year = DateTime.Now.Year.ToString();
                }
                else
                {
                    var nextyear = DateTime.Now.Year + 1;
                    year = nextyear.ToString();
                }
            }

            if (!(month.Contains('X') || day.Contains('X')))
            {
                var format = "yyyy-MM-dd";
                var input = $"{year}-{month}-{day}";
                if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
                {
                    return parsed;
                }
            }

            return DateTime.MinValue;
        }
    }
}
