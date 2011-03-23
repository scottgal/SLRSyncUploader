
using System;
using System.Collections.Generic;
using PicoDeltaSilverlightClient.Web;
using System.Linq;

namespace PicoDeltaSilverlightClient.Web
{

    public class ChunkProgress
    {
        public int ChunkNumber { get; set; }

        public int ChunkLength { get; set; }

        public  int CurrentPosition { get; set; }

        public int PercentComplete { get { return ChunkLength/CurrentPosition*100; } }
        
    }


    public class ScanProgress
    {
        public Guid SessionId { get; set; }

        public int NumberOfChunks
        {
            get { return ChunkProgress.Count; }
        }

        public int TotalPercentComplete
        {
            get { return ChunkProgress.Sum(x => x.PercentComplete) / NumberOfChunks; }
        }

        public List<ChunkProgress> ChunkProgress
        {
            get; set;
        }
    }
}

