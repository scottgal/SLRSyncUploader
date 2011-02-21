using System;

namespace NetSync
{
    public class SumBuf
    {
        public byte flags;
        public UInt32 len;
        public int offset;
        public UInt32 sum1;
        public byte[] sum2 = new byte[CheckSum.SUM_LENGTH];
    }
}