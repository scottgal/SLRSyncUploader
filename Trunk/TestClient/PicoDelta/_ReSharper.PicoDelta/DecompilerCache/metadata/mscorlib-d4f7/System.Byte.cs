// Type: System.Byte
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace System
{
    [ComVisible(true)]
    [Serializable]
    public struct Byte : IComparable, IFormattable, IConvertible, IComparable<byte>, IEquatable<byte>
    {
        public const byte MaxValue = 255;
        public const byte MinValue = 0;

        #region IComparable Members

        public int CompareTo(object value);

        #endregion

        #region IComparable<byte> Members

        public int CompareTo(byte value);

        #endregion

        #region IConvertible Members

        [SecuritySafeCritical]
        public string ToString(IFormatProvider provider);

        public TypeCode GetTypeCode();
        bool IConvertible.ToBoolean(IFormatProvider provider);
        char IConvertible.ToChar(IFormatProvider provider);
        sbyte IConvertible.ToSByte(IFormatProvider provider);
        byte IConvertible.ToByte(IFormatProvider provider);
        short IConvertible.ToInt16(IFormatProvider provider);
        ushort IConvertible.ToUInt16(IFormatProvider provider);
        int IConvertible.ToInt32(IFormatProvider provider);
        uint IConvertible.ToUInt32(IFormatProvider provider);
        long IConvertible.ToInt64(IFormatProvider provider);
        ulong IConvertible.ToUInt64(IFormatProvider provider);
        float IConvertible.ToSingle(IFormatProvider provider);
        double IConvertible.ToDouble(IFormatProvider provider);
        decimal IConvertible.ToDecimal(IFormatProvider provider);
        DateTime IConvertible.ToDateTime(IFormatProvider provider);
        object IConvertible.ToType(Type type, IFormatProvider provider);

        #endregion

        #region IEquatable<byte> Members

        public bool Equals(byte obj);

        #endregion

        #region IFormattable Members

        [SecuritySafeCritical]
        public string ToString(string format, IFormatProvider provider);

        #endregion

        public override bool Equals(object obj);
        public override int GetHashCode();
        public static byte Parse(string s);
        public static byte Parse(string s, NumberStyles style);
        public static byte Parse(string s, IFormatProvider provider);
        public static byte Parse(string s, NumberStyles style, IFormatProvider provider);
        public static bool TryParse(string s, out byte result);
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out byte result);

        [SecuritySafeCritical]
        public override string ToString();

        [SecuritySafeCritical]
        public string ToString(string format);
    }
}
