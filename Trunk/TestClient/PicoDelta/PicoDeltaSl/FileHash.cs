namespace PicoDeltaSl
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
