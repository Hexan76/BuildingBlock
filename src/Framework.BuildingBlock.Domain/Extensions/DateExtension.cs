using System.Globalization;

namespace System
{
    public static class DateExtension
    {
        /// <summary>
        /// Converts a Gregorian DateTime to its equivalent Persian (Shamsi) date string.
        /// Format: yyyy/MM/dd
        /// </summary>
        /// <param name="date">The Gregorian DateTime to convert.</param>
        /// <returns>A string representing the Persian date in yyyy/MM/dd format.</returns>
        public static string ToPersianDate(this DateTime dateTime)
        {
            PersianCalendar pc = new PersianCalendar();
            if (dateTime < pc.MinSupportedDateTime)
            {
                dateTime = pc.MinSupportedDateTime;
            }
            int year = pc.GetYear(dateTime);
            int month = pc.GetMonth(dateTime);
            int day = pc.GetDayOfMonth(dateTime);

            return $"{year:0000}/{month:00}/{day:00}";
        }

        /// <summary>
        /// Converts a Gregorian DateTime to its equivalent Persian (Shamsi) date and time string.
        /// Format: yyyy/MM/dd HH:mm:ss
        /// </summary>
        /// <param name="dateTime">The Gregorian DateTime to convert.</param>
        /// <returns>A string representing the Persian date and time in yyyy/MM/dd HH:mm:ss format.</returns>
        public static string ToPersianDateTime(this DateTime dateTime)
        {
            PersianCalendar pc = new PersianCalendar();
            if (dateTime < pc.MinSupportedDateTime)
            {
                dateTime = pc.MinSupportedDateTime;
            }
            int year = pc.GetYear(dateTime);
            int month = pc.GetMonth(dateTime);
            int day = pc.GetDayOfMonth(dateTime);
            int hour = pc.GetHour(dateTime);
            int minute = pc.GetMinute(dateTime);
            int second = pc.GetSecond(dateTime);

            return $"{year:0000}/{month:00}/{day:00} {hour:00}:{minute:00}:{second:00}";
        }

        /// <summary>
        /// Converts a Persian (Shamsi) date string to its equivalent Gregorian DateTime.
        /// Accepts formats: "yyyy/MM/dd" or "yyyy/MM/dd HH:mm:ss"
        /// </summary>
        /// <param name="persianDateTime">The Persian date string to convert.</param>
        /// <returns>A DateTime object representing the Gregorian date and time.</returns>
        public static DateTime ToGregorianDateTime(this string persianDateTime)
        {
            PersianCalendar pc = new PersianCalendar();

            string[] parts = persianDateTime.Split(' ');
            string[] dateParts = parts[0].Split('/');
            int year = int.Parse(dateParts[0]);
            int month = int.Parse(dateParts[1]);
            int day = int.Parse(dateParts[2]);

            int hour = 0, minute = 0, second = 0;
            if (parts.Length > 1)
            {
                string[] timeParts = parts[1].Split(':');
                hour = int.Parse(timeParts[0]);
                minute = int.Parse(timeParts[1]);
                second = int.Parse(timeParts[2]);
            }

            return pc.ToDateTime(year, month, day, hour, minute, second, 0);
        }

        /// <summary>
        /// Converts a Gregorian DateTime to Persian (Shamsi) date string with time.
        /// Example output: 24 آبان 1404 12:40
        /// </summary>
        /// <param name="date">Gregorian DateTime</param>
        /// <param name="showYear">Show Year In Date</param>
        /// <returns>Persian formatted string: d MMMM yyyy HH:mm</returns>
        public static string ToPersianTextDateTime(this DateTime date, bool showYear = true)
        {
            var pc = new PersianCalendar();

            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);
            int hour = pc.GetHour(date);
            int minute = pc.GetMinute(date);

            string[] persianMonths =
            {
                "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
                "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
            };

            string monthName = persianMonths[month - 1];

            string datePart = showYear
                ? $"{day} {monthName} {year}"
                : $"{day} {monthName}";

            return $"{datePart} {hour:00}:{minute:00}";
        }
        public static (DateTime FirstDay, DateTime LastDay) GetYearRange(this CultureInfo culture, int? year = null)
        {
            if (culture == null) throw new ArgumentNullException(nameof(culture));

            var calendar = culture.Calendar;
            int currentYear = year ?? calendar.GetYear(DateTime.Now);

            // First day: 1st month, 1st day
            var firstDay = calendar.ToDateTime(currentYear, 1, 1, 0, 0, 0, 0);

            // Last month and last day of that month
            int lastMonth = calendar.GetMonthsInYear(currentYear);
            int lastDayOfMonth = calendar.GetDaysInMonth(currentYear, lastMonth);
            var lastDay = calendar.ToDateTime(currentYear, lastMonth, lastDayOfMonth, 23, 59, 59, 0);

            return (firstDay, lastDay);
        }

        public static (DateTime Start, DateTime End) GetQuarterRange(
            this DateTime dateTime,
            int quarter,
            CultureInfo? culture = null)
        {
            if (culture == null)
                culture = CultureInfo.CurrentCulture;

            var calendar = culture.Calendar;

            int targetYear = calendar.GetYear(dateTime);

            int startMonth = quarter switch
            {
                1 => 1,
                2 => 4,
                3 => 7,
                4 => 10,
                _ => throw new ArgumentOutOfRangeException(nameof(quarter))
            };

            int endMonth = startMonth + 2;

            var start = calendar.ToDateTime(
                targetYear,
                startMonth,
                1,
                0, 0, 0, 0);

            int endDay = calendar.GetDaysInMonth(targetYear, endMonth);

            var end = calendar.ToDateTime(
                targetYear,
                endMonth,
                endDay,
                23, 59, 59, 0);

            return (start, end);
        }

        public static int GetQuarter(
            this DateTime dateTime,
            CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;

            var calendar = culture.Calendar;

            int month = calendar.GetMonth(dateTime);

            return (month - 1) / 3 + 1;
        }
    }
}
