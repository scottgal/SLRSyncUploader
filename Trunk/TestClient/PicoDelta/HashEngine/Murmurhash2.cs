using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HashEngine
{

    namespace HashTableHashing
    {
        public class MurmurHash2Simple
        {
            public UInt32 Hash(Byte[] data)
            {
                return Hash(data, 0xc58f1a7b);
            }
            const UInt32 m = 0x5bd1e995;
            const Int32 r = 24;

            public UInt32 Hash(Byte[] data, UInt32 seed)
            {
                Int32 length = data.Length;
                if (length == 0)
                    return 0;
                UInt32 h = seed ^ (UInt32)length;
                Int32 currentIndex = 0;
                while (length >= 4)
                {
                    UInt32 k = BitConverter.ToUInt32(data, currentIndex);
                    k *= m;
                    k ^= k >> r;
                    k *= m;

                    h *= m;
                    h ^= k;
                    currentIndex += 4;
                    length -= 4;
                }
                switch (length)
                {
                    case 3:
                        h ^= BitConverter.ToUInt16(data, currentIndex);
                        h ^= (UInt32)data[currentIndex + 2] << 16;
                        h *= m;
                        break;
                    case 2:
                        h ^= BitConverter.ToUInt16(data, currentIndex);
                        h *= m;
                        break;
                    case 1:
                        h ^= data[currentIndex];
                        h *= m;
                        break;
                    default:
                        break;
                }

                // Do a few final mixes of the hash to ensure the last few
                // bytes are well-incorporated.

                h ^= h >> 13;
                h *= m;
                h ^= h >> 15;

                return h;
            }
        }

        public class MurmurHash2InlineBitConverter
        {

            public UInt32 Hash(Byte[] data)
            {
                return Hash(data, 0xc58f1a7b);
            }
            const UInt32 m = 0x5bd1e995;
            const Int32 r = 24;

            public UInt32 Hash(Byte[] data, UInt32 seed)
            {
                var length = data.Length;
                if (length == 0)
                    return 0;
                var h = seed ^ (UInt32)length;
                var currentIndex = 0;
                while (length >= 4)
                {
                    var k = (UInt32)(data[currentIndex++] | data[currentIndex++] << 8 | data[currentIndex++] << 16 | data[currentIndex++] << 24);
                    k *= m;
                    k ^= k >> r;
                    k *= m;

                    h *= m;
                    h ^= k;
                    length -= 4;
                }
                switch (length)
                {
                    case 3:
                        h ^= (UInt16)(data[currentIndex++] | data[currentIndex++] << 8);
                        h ^= (UInt32)(data[currentIndex] << 16);
                        h *= m;
                        break;
                    case 2:
                        h ^= (UInt16)(data[currentIndex++] | data[currentIndex] << 8);
                        h *= m;
                        break;
                    case 1:
                        h ^= data[currentIndex];
                        h *= m;
                        break;
                    default:
                        break;
                }

                // Do a few final mixes of the hash to ensure the last few
                // bytes are well-incorporated.

                h ^= h >> 13;
                h *= m;
                h ^= h >> 15;

                return h;
            }
        }


      
    }
}
