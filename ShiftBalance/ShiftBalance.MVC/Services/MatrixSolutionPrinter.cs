namespace ShiftBalance.MVC.Services
{
    public class MatrixSolutionPrinter
    {
        private int[] _openings;
        private int[] _closeings;
        private int[] _availability;

        public MatrixSolutionPrinter(int[] openings, int[] closeings, int[] availability)
        {
            _openings = openings;
            _closeings = closeings;
            _availability = availability;
        }

        public void Print()
        {
            // Genera l'excel con i giorni come colonne e dipendenti come righe
        }
    }
}
