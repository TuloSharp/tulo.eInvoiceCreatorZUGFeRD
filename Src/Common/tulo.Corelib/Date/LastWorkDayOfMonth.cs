namespace tulo.CoreLib.Date
{
    public class LastWorkDayOfMonth : ILastWorkDayOfMonth
    {
        private readonly IBavariaHollidayCalculator _holidayCalculator;

        public LastWorkDayOfMonth(IBavariaHollidayCalculator holidayCalculator)
        {
            _holidayCalculator = holidayCalculator ?? throw new ArgumentNullException(nameof(holidayCalculator));
        }

        public DateTime GetLastWorkingDayOfMonth(DateTime date)
        {
            // Determine the last day of the month
            int lastDay = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime candidate = new DateTime(date.Year, date.Month, lastDay);

            // iterate backwards until a valid working day is found
            while (_holidayCalculator.IsWeekendOrHoliday(candidate))
            {
                candidate = candidate.AddDays(-1);
            }

            return candidate.Date;
        }
    }
}
