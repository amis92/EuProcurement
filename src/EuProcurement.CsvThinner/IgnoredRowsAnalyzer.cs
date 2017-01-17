namespace EuProcurement.CsvThinner
{
    public class IgnoredRowsAnalyzer
    {
        public int ProcessedRows { get; private set; }

        public int TotalIgnoredRows =>
            SameCountryRows +
            CancelledRows +
            EuInstitutionRows +
            NoAmountRows +
            NoCountryContractingRows +
            NoCountryWinnerRows + 
            NoCpvRows;

        public int SameCountryRows { get; private set; }

        public int CancelledRows { get; private set; }

        public int EuInstitutionRows { get; private set; }

        public int NoAmountRows { get; private set; }

        public int NoCountryContractingRows { get; private set; }

        public int NoCountryWinnerRows { get; private set; }

        public int NoCpvRows { get; private set; }

        public string SummarizeToString()
        {
            return $@"{nameof(SameCountryRows)} = {SameCountryRows}
{nameof(CancelledRows)} = {CancelledRows}
{nameof(EuInstitutionRows)} = {EuInstitutionRows}
{nameof(NoAmountRows)} = {NoAmountRows}
{nameof(NoCountryContractingRows)} = {NoCountryContractingRows}
{nameof(NoCountryWinnerRows)} = {NoCountryWinnerRows}
{nameof(NoCpvRows)} = {NoCpvRows}
{nameof(TotalIgnoredRows)} = {TotalIgnoredRows}/{ProcessedRows}({nameof(ProcessedRows)})";
        }

        public bool IsIgnored(string[] row)
        {
            ++ProcessedRows;
            return IsEuInstitution(row)
                   || IsCancelled(row)
                   || IsSameCountryWinner(row)
                   || IsNoAmount(row)
                   || IsNoCountryContracting(row)
                   || IsNoCountryWinning(row)
                   || IsNoCpv(row);
        }

        private bool IsSameCountryWinner(string[] row)
        {
            var ignored = row[12] == row[41];
            SameCountryRows += ignored ? 1 : 0;
            return ignored;
        }

        private bool IsCancelled(string[] row)
        {
            var ignored = row[5] == "1";
            CancelledRows += ignored ? 1 : 0;
            return ignored;
        }

        private bool IsEuInstitution(string[] row)
        {
            var ignored = row[13] == "5";
            EuInstitutionRows += ignored ? 1 : 0;
            return ignored;
        }

        private bool IsNoAmount(string[] row)
        {
            var ignored = row[24] == string.Empty && row[25] == string.Empty && row[26] == string.Empty;
            NoAmountRows += ignored ? 1 : 0;
            return ignored;
        }

        private bool IsNoCountryContracting(string[] row)
        {
            var ignored = row[12] == string.Empty;
            NoCountryContractingRows += ignored ? 1 : 0;
            return ignored;
        }

        private bool IsNoCountryWinning(string[] row)
        {
            var ignored = row[41] == string.Empty;
            NoCountryWinnerRows += ignored ? 1 : 0;
            return ignored;
        }

        private bool IsNoCpv(string[] row)
        {
            var ignored = row[21] == string.Empty;
            NoCpvRows += ignored ? 1 : 0;
            return ignored;
        }
    }
}