using System;
using System.IO;

namespace EuProcurement.CsvThinner
{
    public static class CommandLineUtilities
    {
        public static void PauseConsole()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static bool CheckFullCsvFileExists()
        {
            try
            {
                File.OpenRead(FileConstants.TedFullCsvFilename);
                return true;
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
            }
            return false;
        }


        public static void PrintCsvHeadersToFile()
        {
            using (var writer = new StreamWriter(File.Create(FileConstants.HeaderFilename)))
            {
                PrintHeaders(writer);
            }
        }

        public static void PrintHeaders(TextWriter output)
        {
            using (var reader = new StreamReader(File.OpenRead(FileConstants.TedFullCsvFilename)))
            {
                var parser = new CsvHelper.CsvParser(reader);
                var row = parser.Read();
                for (int i = 0; i < row.Length; i++)
                {
                    output.WriteLine($"[{i:D2}]: {row[i]}");
                }
            }
        }
    }
}
