using System;
using System.Diagnostics;
using static EuProcurement.CsvThinner.CommandLineUtilities;

namespace EuProcurement.CsvThinner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!CheckFullCsvFileExists())
            {
                PauseConsole();
                return;
            }
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Started.");

            PrintCsvHeadersToFile();
            FilteringUtilities.FilterTedCsvColumns();
            AggregationUtilities.AggregateAndSaveFilteredData();

            stopwatch.Stop();
            Console.WriteLine("Finished.");
            Console.WriteLine($"Time elapsed: {stopwatch.Elapsed}");
            PauseConsole();
        }
    }
}
