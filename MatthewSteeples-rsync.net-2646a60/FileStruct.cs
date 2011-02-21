using System;

namespace NetSync
{
    public class FileStruct
    {
        private string _baseName;

        private string _dirName;
        public int Length { get; set; }

        public string BaseName
        {
            get { return _baseName; }
            set { _baseName = value; }
        }

        public string DirName
        {
            get { return _dirName; }
            set { _dirName = value; }
        }

        public string BaseDir { get; set; }

        public DateTime ModTime { get; set; }

        public uint Mode { get; set; }

        public int Uid { get; set; }

        public int Gid { get; set; }

        public uint Flags { get; set; }

        public bool IsTopDir { get; set; }

        public byte[] Sum { get; set; }

        public string GetFullName()
        {
            string fullName = String.Empty;

            if (string.IsNullOrEmpty(_baseName))
            {
                _baseName = null;
            }
            if (string.IsNullOrEmpty(_dirName))
            {
                _baseName = null;
            }

            if (_dirName != null && _baseName != null)
            {
                fullName = _dirName + "/" + _baseName;
            }
            else if (_baseName != null)
            {
                fullName = _baseName;
            }
            else if (_dirName != null)
            {
                fullName = _dirName;
            }
            return fullName;
        }
    }
}