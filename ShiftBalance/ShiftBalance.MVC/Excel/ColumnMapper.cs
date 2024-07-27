namespace ShiftBalance.MVC.Excel
{
    public static class ColumnMapper
    {
        private const int ALPHABET_CHARS_NUMBER = 26;

        /// <summary>
        /// Returns the spreadsheet column name based on the index passed as parameter (example: 1=A, 26=Z, 27=AA, 28=AB)
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static string GetColumnNameFromNumber(int index)
        {
            if (index <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(index));

            if (index <= ALPHABET_CHARS_NUMBER)
            {
                // Single-letter representation (A-Z)
                int code = index - 1 + 'A';
                return char.ConvertFromUtf32(code);
            }
            else
            {
                // Multi-letter representation (AA, AB, etc.)
                int mod = index % ALPHABET_CHARS_NUMBER;
                int columnIndex = index / ALPHABET_CHARS_NUMBER;

                // If mod is 0, we reached the end of one combination (e.g., AZ)
                if (mod == 0)
                {
                    // Reduce the column index as index / alphabetsCount will give the next value
                    columnIndex -= 1;
                    // Mod should be the alphabets count to get the last character in the alphabet (e.g., 'Z')
                    mod = ALPHABET_CHARS_NUMBER;
                }
                // Recursively construct the multi-letter representation
                return GetColumnNameFromNumber(columnIndex) + GetColumnNameFromNumber(mod);
            }
        }
    }
}
