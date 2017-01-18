using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using EuProcurement.Records;
using static EuProcurement.CsvThinner.FileConstants;

namespace EuProcurement.CsvThinner
{
    public static class AggregationUtilities
    {
        public static void AggregateAndSaveFilteredData()
        {
            var data = LoadFilteredData();
            var aggregateRecords = AggregateFilteredData(data);

            using (var outputStream = new StreamWriter(File.Create(TedAggregatedCsvFilename)))
            {
                var writer = new CsvWriter(outputStream);
                writer.Configuration.CultureInfo = CultureInfo.InvariantCulture;
                writer.WriteRecords(aggregateRecords);
            }
        }

        public static ImmutableArray<TedAggregateRecord> AggregateFilteredData(ImmutableArray<FilteredTedRecord> data)
        {
            var formatted = from record in data
                            where record.ISO_COUNTRY_CODE != record.WIN_COUNTRY_CODE
                            let contractingCountryLesser = string.CompareOrdinal(record.ISO_COUNTRY_CODE, record.WIN_COUNTRY_CODE) < 0
                            let cc1 = contractingCountryLesser ? record.ISO_COUNTRY_CODE : record.WIN_COUNTRY_CODE
                            let cc2 = contractingCountryLesser ? record.WIN_COUNTRY_CODE : record.ISO_COUNTRY_CODE
                            let amount = decimal.Parse(record.VALUE_EURO_FIN_2, CultureInfo.InvariantCulture)
                            let signedAmount = contractingCountryLesser ? amount : -amount
                            select new
                            {
                                Year = record.YEAR,
                                Cpv = record.CPV,
                                CountryFrom = cc1,
                                CountryTo = cc2,
                                EuroValue = signedAmount
                            };
            var result = from record in formatted
                         group record by new { record.Year, record.Cpv, record.CountryFrom, record.CountryTo } into g
                         orderby g.Key.Cpv
                         orderby g.Key.CountryTo
                         orderby g.Key.CountryFrom
                         orderby g.Key.Year
                         select new TedAggregateRecord
                         {
                             Year = g.Key.Year,
                             Cpv = g.Key.Cpv,
                             CountryFrom = g.Key.CountryFrom,
                             CountryTo = g.Key.CountryTo,
                             EuroTotal = g.Aggregate(0M, (sum, r) => sum + r.EuroValue)
                         };
            return result.ToImmutableArray();
        }

        private static ImmutableArray<FilteredTedRecord> LoadFilteredData()
        {
            using (var streamReader = new StreamReader(File.OpenRead(TedFilteredCsvFilename)))
            {
                var csvReader = new CsvReader(streamReader);
                var records = csvReader.GetRecords<FilteredTedRecord>();
                return records.ToImmutableArray();
            }
        }

    }
}
