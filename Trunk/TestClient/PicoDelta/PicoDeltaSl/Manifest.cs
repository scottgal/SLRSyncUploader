using System;
using System.Collections.Generic;


namespace PicoDeltaSl
{
    public class Manifest
    {
        public byte[] FileHash
        {
            get; set;
        }

        public List<FileChunk> Blocks { get; set;
        }
    }
}
