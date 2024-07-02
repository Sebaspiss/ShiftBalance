﻿using Google.OrTools.Sat;

namespace ShiftBalance.MVC.Services
{
    public class SolutionPrinter : CpSolverSolutionCallback
    {
        private int[] allDipendenti_;
        private int[] allDays_;
        private int[] allShifts_;
        private Dictionary<(int, int, int), BoolVar> shifts_;

        public SolutionPrinter(int[] allDipendenti, int[] allDays, int[] allShifts, Dictionary<(int, int, int), BoolVar> shifts)
        {
            allDipendenti_ = allDipendenti;
            allDays_ = allDays;
            allShifts_ = allShifts;
            shifts_ = shifts;
        }

        public override void OnSolutionCallback()
        {
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
            StopSearch();
        }
    }
}
