using System.Collections.Immutable;

namespace EuProcurement.CsvThinner
{
    public static class FileConstants
    {
        public const string TedFullCsvFilename = "data/TED_CAN_2009_2015.csv";
        public const string TedFilteredCsvFilename = "data/TED_CAN_2009_2015_filtered.csv";
        public const string TedAggregatedCsvFilename = "data/TED_CAN_2009_2015_aggregated.csv";
        public const string HeaderFilename = "data/Headers.txt";

        public static ImmutableArray<int> ImportantIndexes { get; } = new[]
        {
            1, // YEAR
            21, // CPV
            0, // ID_NOTICE_CAN
            12, // ISO_COUNTRY_CODE
            41, // WIN_COUNTRY_CODE
        }.ToImmutableArray();

        public static ImmutableArray<int> ValueIndexes { get; } = new[]
        {
            24, // VALUE_EURO
            25, // VALUE_EURO_FIN_1
            26, // VALUE_EURO_FIN_2
        }.ToImmutableArray();
    }
}
