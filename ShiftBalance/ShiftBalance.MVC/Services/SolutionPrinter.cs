using Google.OrTools.Sat;

namespace ShiftBalance.MVC.Services
{
    public class SolutionPrinter : CpSolverSolutionCallback
    {

        private int solutionCount_;
        private int[] allDipendenti_;
        private int[] allDays_;
        private int[] allShifts_;
        private Dictionary<(int, int, int), BoolVar> shifts_;
        private int solutionLimit_;

        public SolutionPrinter(int[] allDipendenti, int[] allDays, int[] allShifts,Dictionary<(int, int, int), BoolVar> shifts, int limit)
        {
            solutionCount_ = 0;
            allDipendenti_ = allDipendenti;
            allDays_ = allDays;
            allShifts_ = allShifts;
            shifts_ = shifts;
            solutionLimit_ = limit;
        }

        public override void OnSolutionCallback()
        {
            Console.WriteLine($"Solution #{solutionCount_}:");
            foreach (int d in allDays_)
            {
                Console.WriteLine($"Day {d}");
                foreach (int n in allDipendenti_)
                {
                    foreach (int s in allShifts_)
                    {
                        if (Value(shifts_[(n, d, s)]) == 1L)
                        {
                            Console.WriteLine($"  Dipendente {n} work shift {s}");
                        }
                    }                    
                }
            }
            solutionCount_++;
            if (solutionCount_ >= solutionLimit_)
            {
                Console.WriteLine($"Stop search after {solutionLimit_} solutions");
                StopSearch();
            }
        }

        public int SolutionCount()
        {
            return solutionCount_;
        }
    }
}
