using System;

namespace PicoDeltaSl
{
    public class Config
    {
        private int _blockLength;

        public Config()
        {
            _blockLength = 10240;
            EngineMode = EngineMode.Parallel;
            DegreeOfParalleism = Environment.ProcessorCount;
            ;
            BufferSize = _blockLength*1000;
        }

        public int BufferSize { get; set; }

        public int DegreeOfParalleism { get; set; }

        public int BlockLength
        {
            get { return _blockLength; }
            set { _blockLength = value; }
        }

        public EngineMode EngineMode { get; set; }
    }
}