using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashEngine
{
    public class FileHash
    {
        public long Offset
        {
            get; set;
        }

        public int Length
        {
            get; set;
        }

        public long WeakHash { get; set; }

        public byte[] StrongHash { get; set; }
    }
}
