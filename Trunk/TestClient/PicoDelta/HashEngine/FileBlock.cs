using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashEngine
{
    public class FileBlock
    {
        public bool IsMatch
        {
            get; set;
        }
        public long DestinationOffset
        {
            get; set;
        }

        public long SourceOffset
        {
            get; set;
        }

        public long BlockLength
        {
            get; set;
        }
    }
}
