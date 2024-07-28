using System.Diagnostics;

namespace ShiftBalance.MVC.Excel
{
    public static class ExcelProcess
    {
        public static void OpenFile(string filePath)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true // This allows the system to use the default application for the file type
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open the Excel file: {ex.Message}");
            }
        }
    }
}
