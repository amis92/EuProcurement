using System.Diagnostics.CodeAnalysis;

namespace EuProcurement.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class FilteredTedRecord
    {
        public string ID_NOTICE_CAN { get; set; }
        public string YEAR { get; set; }
        public string ISO_COUNTRY_CODE { get; set; }
        public string CPV { get; set; }
        public string VALUE_EURO_FIN_2 { get; set; }
        public string WIN_COUNTRY_CODE { get; set; }
    }
}