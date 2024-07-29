namespace ShiftBalance.MVC.Models
{
    public static class CalendarFunctions
    {
        public static bool IsNationalHoliday(DateTime day)
        {
            // 1 o 6 gennaio
            if (day.Month == 1 && (day.Day == 1 || day.Day == 6))
            {
                return true;
            }
            //Pasquetta
            if (day == EasterSunday(day.Year).AddDays(1))
            {
                return true;
            }
            // Ferragosto
            if (day.Month == 8 && day.Day == 15)
            {
                return true;
            }
            // 1 Novembre
            if (day.Month == 11 && day.Day == 1)
            {
                return true;
            }
            // 25 e 26 Dicembre
            if( day.Month == 12 && (day.Day == 8 || day.Day == 25 || day.Day == 26))
            {
                return true;
            }
            return false;
        }

        public static DateTime EasterSunday(int year)
        {
            EasterSunday(year, out int month, out int day);

            return new DateTime(year, month, day);
        }

        private static void EasterSunday(int year, out int month, out int day)
        {
            int g = year % 19;
            int c = year / 100;
            int h = h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25)
                                                + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) *
                        (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) +
                          i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }
        }
    }
}
