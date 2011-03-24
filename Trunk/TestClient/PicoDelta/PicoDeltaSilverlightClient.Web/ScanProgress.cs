
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoDeltaSilverlightClient.Web
{



    public class ScanProgress
    {
        public bool ScanCompleted
        {
            get; set;
        
        }

        public Guid FileId { get; set; }

        public int NumberOfBlocks { get; set; }


        public int CompleteBlockCount
        {
            get; set;
        }
    }
}

