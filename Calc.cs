
namespace SchoolDB
{
    // Class for doing some calculations
    public static class Calc
    {
        // Calculates the difference between a span of years,
        // Mainly for displaying how many years an employee been at the school
        public static int CalcYears(DateOnly start, DateOnly end) 
        {
            return end.Year - start.Year - 1 +
                (((end.Month > start.Month) ||
                ((end.Month == start.Month) && (end.Day >= start.Day))) ? 1 : 0);
        }

        // Calculates the median of a sequence of numbers
        public static double Median(IEnumerable<int?> source)
        {
            // Adds all grades to a new list and sorts them
            var sortedList = source.Where(value => value.HasValue).OrderBy(value => value).ToList();
            int count = sortedList.Count; // Counts the grades in the list

            if (count == 0)
            {
                throw new InvalidOperationException("Det finns inga betyg att räkna på.");
            }

            // Calculates the mid
            int mid = count / 2; 

            if (count % 2 == 0)
            {
                // Even number of elements, average the middle two
                return (sortedList[mid - 1].Value + sortedList[mid].Value) / 2.0;
            }
            else
            {
                // Odd number of elements, return the middle one
                return sortedList[mid].Value;
            }
        }
    }
}
