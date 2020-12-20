using System;
using System.Collections.Generic;
using System.Linq;

namespace Challenge
{
    public enum WeekendRule
    {
        Fixed,
        NextMonday
    }

    public class PublicHolidayRule
    {
        public int? Year { get; set; }
        public int Month { get; set; }
        public int? OrdinalDay { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public int? WeekOfMonth { get; set; }
        public WeekendRule WeekendRule { get; set; };
    }

    public class BusinessDayCounter
    {
        public int WeekdaysBetweenTwoDates(DateTime firstDate, DateTime secondDate)
        {
            if (secondDate <= firstDate)
                return 0;

            int weekdays = (int)((secondDate.Date - firstDate.Date).TotalDays - 1);
            weekdays = weekdays - weekdays / 7 * 2;

            if ((firstDate.AddDays(1).DayOfWeek == DayOfWeek.Sunday && secondDate.AddDays(-1).DayOfWeek < DayOfWeek.Saturday)
               ||
               (secondDate.AddDays(-1).DayOfWeek == DayOfWeek.Saturday && firstDate.AddDays(1).DayOfWeek != DayOfWeek.Sunday))
            {
                weekdays--;
            }

            return weekdays;
        }

        public int BusinessDaysBetweenTwoDates(DateTime firstDate, DateTime secondDate, IList<DateTime> publicHolidays)
        {
            if (secondDate <= firstDate)
                return 0;

            int holidayCount = publicHolidays
                .Where(x => x > firstDate && x < secondDate
                      && x.DayOfWeek != DayOfWeek.Saturday && x.DayOfWeek != DayOfWeek.Sunday)
                .Count();

            return WeekdaysBetweenTwoDates(firstDate, secondDate) - holidayCount;
        }

        //TODO Does not handle multiple holidays on same day (e.g. 25/12, 26/12 on starting on the Weekend with WeekendRule.NextMonday)        
        public int BusinessDaysBetweenTwoDates(DateTime firstDate, DateTime secondDate, IList<PublicHolidayRule> publicHolidays)
        {
            if (secondDate <= firstDate)
                return 0;

            int firstYear = firstDate.AddDays(1).Year;
            int secondYear = secondDate.AddDays(-1).Year;

            int holidayCount = 0;

            for (int year = firstYear; year <= secondYear; year++)
            {
                foreach (PublicHolidayRule rule in publicHolidays)
                {
                    DateTime date = date.MinValue;

                    try
                    {
                        if (rule.Year > 0)
                        {
                            if (rule.Year == year && rule.Month > 0 && rule.OrdinalDay > 0)
                                date = new DateTime(year, rule.Month, rule.OrdinalDay.Value);
                        }
                        else if (rule.OrdinalDay > 0)
                        {
                            date = new DateTime(year, rule.Month, rule.OrdinalDay.Value);
                        }
                        else if (rule.DayOfWeek > 0 && rule.DayOfWeek > 0)
                        {
                            date = new DateTime(year, rule.Month, 1);
                            while (date.DayOfWeek != rule.DayOfWeek) date = date.AddDays(1);
                            date = date.AddDays(7 * (rule.WeekOfMonth.Value - 1));
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Ignore invalid dates.
                    }

                    if (date != DateTime.MinValue)
                    {
                        if (IsHoliday(date, firstDate, secondDate, rule.WeekendRule))
                            holidayCount++;

                        // Check weekend holidays that may bleed into the first year from the previous year
                        if (year == firstYear && date.Month == 12 && date.Day >= 30
                            && IsHoliday(date.AddYears(-1), firstDate, secondDate, rule.WeekendRule))
                            holidayCount++;
                    }
                }
            }

            return WeekdaysBetweenTwoDates(firstDate, secondDate) - holidayCount;
        }

        private bool IsHoliday(DateTime date, DateTime firstDate, DateTime secondDate, WeekendRule weekendRule)
        {
            if (weekendRule == WeekendRule.NextMonday)
            {
                if (date.DayOfWeek == DayOfWeek.Saturday)
                    date.AddDays(2);
                else if (date.DayOfWeek == DayOfWeek.Sunday)
                    date.AddDays(1);
            }

            return date > firstDate && date < secondDate 
                && date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }
    }   
}