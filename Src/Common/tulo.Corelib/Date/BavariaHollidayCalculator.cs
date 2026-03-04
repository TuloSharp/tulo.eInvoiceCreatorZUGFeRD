namespace tulo.CoreLib.Date
{
    public class BavariaHollidayCalculator : IBavariaHollidayCalculator
    {
        public bool IsWeekendOrHoliday(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return true;

            if (IsHoliday(date))
                return true;

            return false;
        }

        private bool IsHoliday(DateTime date)
        {
            // Fix holliday
            var fixedHolidays = new List<(int Month, int Day)> {
                                                     (1, 1), // "New Year's Day",
                                                     (1, 6), // "Epiphany",
                                                     (5, 1), // "Labour Day",
                                                     (8, 15),// "Assumption Day",
                                                     (10, 3), // "German Unity Day",
                                                     (11, 1), // "All Saints' Day",
                                                     (12, 25), // "Christmas Day",
                                                     (12, 26) // "St. Stephen's Day"
                };

            if (fixedHolidays.Contains((date.Month, date.Day)))
                return true;

            // Variable holliday
            DateTime easter = CalculateEaster(date.Year);

            var variableHolidays = new List<DateTime> {
                                                easter.AddDays(-2),  // Good Friday
                                                easter.AddDays(1),   // Easter Monday
                                                easter.AddDays(39),  // Ascension Day
                                                easter.AddDays(50),  // Whit Monday
                                                //easter.AddDays(60)   // Corpus Christi 
                };

            if (variableHolidays.Contains(date))
                return true;

            return false;
        }

        private DateTime CalculateEaster(int year)
        {
            // (Gaussian Easter formula)
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int month = (h + l - 7 * m + 114) / 31;
            int day = (h + l - 7 * m + 114) % 31 + 1;

            return new DateTime(year, month, day);
        }
    }
}