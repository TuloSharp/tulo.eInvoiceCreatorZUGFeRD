namespace tulo.CoreLib.Date
{
    public interface IBavariaHollidayCalculator
    {
        bool IsWeekendOrHoliday(DateTime date);
    }
}