using System;
using System.Threading;

namespace EuProcurement.Uwp
{
    public struct CpvCode : IEquatable<CpvCode>, IComparable<CpvCode>, IComparable<string>
    {
        public const string EmptyCodeFull = "00000000-0";
        public const string EmptyCode = "00000000";
        public const string EmptyDivision = "00";
        public const char EmptyGroup = '0';
        public const char EmptyClass = '0';
        public const char EmptyCategory = '0';
        public const string EmptySubcategory = "000";
        public const char EmptyControlNumber = '0';

        public static readonly CpvCode Empty = default(CpvCode);

        public CpvCode(string code)
        {
            if (code == null || code.Length < 2)
            {
                throw new ArgumentException("Invalid CPV code length.");
            }
            var fullCode = code.Length >= 10 ? code : code + EmptyCodeFull.Substring(code.Length);
            _code = fullCode.Substring(0, 8);
            _controlNumber = fullCode[9];
        }

        private readonly string _code;
        private readonly char _controlNumber;

        public string Code => _code ?? EmptyCode;

        public string CodeWithControlNumber => _code != null ? $"{Code}-{ControlNumber}" : EmptyCodeFull;

        public string Division => Code?.Substring(0, 2) ?? EmptyDivision;

        public char Group => Code?[2] ?? EmptyGroup;

        public char Class => Code?[3] ?? EmptyClass;

        public char Category => Code?[4] ?? EmptyCategory;

        public string Subcategory => Code?.Substring(5, 3) ?? EmptySubcategory;

        public char ControlNumber => _code != null ? _controlNumber : EmptyControlNumber;

        public static implicit operator string(CpvCode code)
        {
            return code.Code;
        }

        public static implicit operator CpvCode(string codeString)
        {
            return new CpvCode(codeString);
        }

        public static bool operator ==(CpvCode left, CpvCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CpvCode left, CpvCode right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is CpvCode && Equals((CpvCode)obj);
        }

        public bool Equals(CpvCode other)
        {
            return string.Equals(Code, other.Code);
        }

        public override int GetHashCode()
        {
            return Code?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Code;
        }

        public int CompareTo(CpvCode other)
        {
            return string.CompareOrdinal(Code, other.Code);
        }

        public int CompareTo(string other)
        {
            return string.CompareOrdinal(Code, other);
        }
    }
}