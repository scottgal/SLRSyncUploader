using System;
using System.Collections;
using System.Collections.Generic;

namespace NetSync
{
    public class FileStructComparer : IComparer<FileStruct>, IComparer
    {
        #region IComparer Members

        int IComparer.Compare(Object x, Object y)
        {
            return FileList.FileCompare((FileStruct) x, (FileStruct) y);
        }

        #endregion

        #region IComparer<FileStruct> Members

        public int Compare(FileStruct x, FileStruct y)
        {
            return FileList.FileCompare(x, y);
        }

        #endregion
    }
}