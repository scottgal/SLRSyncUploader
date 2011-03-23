using System;

namespace PicoDeltaSl
{
    public class Config
    {

        public Config()
        {
            _blockLength = 10240;
            _engineMode = EngineMode.Parallel;
            _degreeOfParalellism = Environment.ProcessorCount; ;
            _bufferSize =   _blockLength * 1000;
        }

        private int _bufferSize;

        public int BufferSize
        {
            get
            {
                return _bufferSize;
            }

            set
            {
                _bufferSize = value;
            }

        }

        private int _degreeOfParalellism;

        public int DegreeOfParalleism
        {
            get { return _degreeOfParalellism; }

            set { _degreeOfParalellism = value; }
        }

        private int _blockLength;
        public int BlockLength
        {
            get { return _blockLength; }
            set { _blockLength = value; }
        }

        private EngineMode _engineMode;
        public EngineMode EngineMode
        {
            get { return _engineMode; }
            set { _engineMode = value; }
        }


    }
}
