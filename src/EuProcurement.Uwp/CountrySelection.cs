using System;

namespace EuProcurement.Uwp
{
    public struct CountrySelection : IEquatable<CountrySelection>, IComparable<CountrySelection>
    {
        public const string AnyCountry = "Any";

        public CountrySelection(string countryFrom, string countryTo)
        {
            CountryFrom = countryFrom;
            CountryTo = countryTo;
        }

        public string CountryFrom { get; }

        public string CountryTo { get; }

        public override string ToString()
        {
            return CountryFrom == null && CountryTo == null
                ? string.Empty
                : $"{CountryFrom} -> {CountryTo}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CountrySelection && Equals((CountrySelection)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((CountryFrom?.GetHashCode() ?? 0) * 397) ^ (CountryTo?.GetHashCode() ?? 0);
            }
        }

        public bool Equals(CountrySelection other)
        {
            return CountryFrom == other.CountryFrom && CountryTo == other.CountryTo;
        }

        public int CompareTo(CountrySelection other)
        {
            return string.CompareOrdinal(ToString(), other.ToString());
        }
        public static bool operator ==(CountrySelection left, CountrySelection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CountrySelection left, CountrySelection right)
        {
            return !(left == right);
        }
    }
}