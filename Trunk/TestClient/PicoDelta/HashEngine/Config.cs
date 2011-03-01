using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashEngine
{
    public class Config
    {

        public Config()
        {
            _blockLength = 1024 ;
            _engineMode = EngineMode.Parallel;

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
