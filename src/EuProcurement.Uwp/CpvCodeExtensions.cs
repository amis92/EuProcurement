using System;
using System.Collections.Generic;
using System.Linq;

namespace EuProcurement.Uwp
{
    public static class CpvCodeExtensions
    {
        public static string ReplaceAt(this string input, int index, char newChar)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            var chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }

        public static CpvCode WithDivision(this CpvCode @this, string division)
        {
            const int expectedLength = 2;
            if (string.IsNullOrEmpty(division) || division.Length != expectedLength)
            {
                throw new ArgumentException($"'{nameof(division)}' has to be of length {expectedLength}.");
            }
            return $"{division}{@this.CodeWithControlNumber.Substring(2)}";
        }

        public static CpvCode WithGroup(this CpvCode @this, char group)
        {
            return @this.CodeWithControlNumber.ReplaceAt(3, group);
        }

        public static CpvCode WithClass(this CpvCode @this, char @class)
        {
            return @this.CodeWithControlNumber.ReplaceAt(4, @class);
        }

        public static CpvCode WithCategory(this CpvCode @this, char category)
        {
            return @this.CodeWithControlNumber.ReplaceAt(5, category);
        }

        public static CpvCode WithSubcategory(this CpvCode @this, string subcategory)
        {
            const int expectedLength = 3;
            if (string.IsNullOrEmpty(subcategory) || subcategory.Length != expectedLength)
            {
                throw new ArgumentException($"'{nameof(subcategory)}' has to be of length {expectedLength}.");
            }
            return $"{@this.Code.Substring(0,5)}{subcategory}-{@this.ControlNumber}";
        }

        public static CpvCode MostGeneric(this IEnumerable<CpvCode> codes)
        {
            if (codes == null)
            {
                throw new ArgumentNullException(nameof(codes));
            }
            return codes.OrderBy(x => x).First();
        }
    }
}