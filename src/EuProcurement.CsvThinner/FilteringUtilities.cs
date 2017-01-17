using System;
using System.IO;
using System.Linq;
using CsvHelper;
using static EuProcurement.CsvThinner.FileConstants;

namespace EuProcurement.CsvThinner
{
    public class FilteringUtilities
    {
        public static void FilterTedCsvColumns()
        {
            using (var fileWriter = new StreamWriter(File.Create(TedFilteredCsvFilename)))
            {
                FilterColumns(fileWriter);
            }
        }

        private static void FilterColumns(TextWriter output)
        {
            var writer = new CsvWriter(output);
            var ignoredRowsAnalyzer = new IgnoredRowsAnalyzer();
            using (var reader = new StreamReader(File.OpenRead(TedFullCsvFilename)))
            {
                var parser = new CsvParser(reader);
                string[] row;
                while ((row = parser.Read()) != null)
                {
                    if (ignoredRowsAnalyzer.IsIgnored(row))
                    {
                        continue;
                    }
                    foreach (var importantIndex in ImportantIndexes)
                    {
                        writer.WriteField(row[importantIndex]);
                    }
                    writer.WriteField(GetContractValueFromRow(row));
                    writer.NextRecord();
                }
            }
            Console.Out.WriteLine(ignoredRowsAnalyzer.SummarizeToString());
        }

        private static string GetContractValueFromRow(string[] row)
        {
            foreach (var valueIndex in ValueIndexes.Reverse())
            {
                if (row[valueIndex] != string.Empty)
                {
                    return row[valueIndex];
                }
            }
            return string.Empty;
        }
    }
}
