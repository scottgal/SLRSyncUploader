/**
 *  Copyright (C) 2006 Alex Pedenko
 * 
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace NetSync
{
    public class FileList
    {
        private string lastDir = String.Empty;
        private string fileListDir = String.Empty;
        private string lastName = String.Empty;
        private UInt32 mode = 0;
        private DateTime modTime = DateTime.Now;
        private Options options;
        private CheckSum checkSum;

        public FileList(Options opt)
        {
            options = opt;
            checkSum = new CheckSum(options);
        }

        public List<FileStruct> SendFileList(ClientInfo clientInfo, string[] argv)
        {
            IOStream ioStream = null;
            if (clientInfo != null)
            {
                ioStream = clientInfo.IoStream;
            }

            string dir, oldDir;
            string lastPath = String.Empty; //@todo_long seems to be Empty all the time
            string fileName = String.Empty;            
            bool useFFFD = false; //@todo_long seems to be false all the time
            if (ShowFileListProgress() && ioStream != null)
            {
                StartFileListProgress("building file list");
            }
            Int64 startWrite = Options.stats.totalWritten;
            List<FileStruct> fileList = new List<FileStruct>();
            if (ioStream != null)
            {
                ioStream.IOStartBufferingOut();
                if (Options.filesFromFD != null) //@todo_long seems to be unused because filesFromFD seems to be null all the time
                {
                    if (!string.IsNullOrEmpty(argv[0]) && !Util.pushDir(argv[0]))
                    {
                        MainClass.Exit("pushDir " + Util.fullFileName(argv[0]) + " failed", clientInfo);
                    }
                    useFFFD = true;
                }
            }
            while (true)
            {
                if (useFFFD) //@todo_long seems to be unused because useFFFD seems to be false all the time
                {
                    if ((fileName = ioStream.readFilesFromLine(Options.filesFromFD, options)).Length == 0)
                    {
                        break;
                    }
                }
                else
                {
                    if (argv.Length == 0)
                    {
                        break;
                    }
                    fileName = argv[0];
                    argv = Util.DeleteFirstElement(argv);
                    if (fileName != null && fileName.Equals("."))
                    {
                        continue;
                    }
                    if (fileName != null)
                    {
                        fileName = fileName.Replace(@"\", "/");
                    }
                }
                // TODO: path length
                if (fileName != null)
                    if (Directory.Exists(fileName) && !options.recurse && options.filesFrom == null)
                    {
                        Log.WriteLine("skipping directory " + fileName);
                        continue;
                    }

                dir = null;
                oldDir = String.Empty;

                if (!options.relativePaths)
                {
                    if (fileName != null)
                    {
                        var index = fileName.LastIndexOf('/');
                        if (index != -1)
                        {
                            dir = index == 0 ? "/" : fileName.Substring(0, index);
                            fileName = fileName.Substring(index + 1);
                        }
                    }
                }
                else
                {
                    if (ioStream != null && options.impliedDirs && fileName.LastIndexOf('/') > 0)
                    {
                        string fileDir = fileName.Substring(0, fileName.LastIndexOf('/'));
                        string slash = fileName;
                        int i = 0; //@todo_long seems to be 0 all the time
                        while (i < fileDir.Length && i < lastPath.Length && fileDir[i] == lastPath[i]) //@todo_long seems that it is never executed because lastPath is allways Empty
                        {
                            if (fileDir[i] == '/')
                            {
                                slash = fileName.Substring(i);
                            }
                            i++;
                        }
                        if (i != fileName.LastIndexOf('/') || (i < lastPath.Length && lastPath[i] != '/'))//@todo_long seems to be executed unconditionally because i=0 and fileName.LastIndexOf('/') > 0
                        {
                            bool copyLinksSaved = options.copyLinks;
                            bool recurseSaved = options.recurse;
                            options.copyLinks = options.copyUnsafeLinks;
                            options.recurse = true;
                            int j;
                            while ((j = slash.IndexOf('/')) != -1)
                            {
                                SendFileName(ioStream, fileList, fileName.Substring(0, j), false, 0);
                                slash = slash.Substring(0, j) + ' ' + slash.Substring(j + 1);

                            }
                            options.copyLinks = copyLinksSaved;
                            options.recurse = recurseSaved;
                            lastPath = fileName.Substring(0, i);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(dir))
                {
                    oldDir = Util.currDir;
                    if (!Util.pushDir(dir))
                    {
                        Log.WriteLine("pushDir " + Util.fullFileName(dir) + " failed");
                        continue;
                    }
                    if (lastDir != null && lastDir.Equals(dir))
                    {
                        fileListDir = lastDir;
                    }
                    else
                    {
                        fileListDir = lastDir = dir;
                    }
                }
                SendFileName(ioStream, fileList, fileName, options.recurse, Options.XMIT_TOP_DIR);
                if (!string.IsNullOrEmpty(oldDir))
                {
                    fileListDir = null;
                    if (Util.popDir(oldDir))
                    {
                        MainClass.Exit("pop_dir " + Util.fullFileName(dir) + " failed", clientInfo);
                    }
                }
            }
            if (ioStream != null)
            {
                SendFileEntry(null, ioStream, 0);
                if (ShowFileListProgress())
                {
                    FinishFileListProgress(fileList);
                }
            }
            CleanFileList(fileList, false, false);
            if (ioStream != null)
            {
                ioStream.writeInt(0);
                Options.stats.fileListSize = (int)(Options.stats.totalWritten - startWrite);
                Options.stats.numFiles = fileList.Count;
            }

            if (options.verbose > 3)
            {
                OutputFileList(fileList);
            }
            if (options.verbose > 2)
            {
                Log.WriteLine("sendFileList done");
            }
            return fileList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <returns></returns>
        public List<FileStruct> ReceiveFileList(ClientInfo clientInfo)
        {
            var ioStream = clientInfo.IoStream;
            var fileList = new List<FileStruct>();

            if (ShowFileListProgress())
            {
                StartFileListProgress("receiving file list");
            }

            var startRead = Options.stats.totalRead;

            UInt32 flags;
            while ((flags = ioStream.readByte()) != 0)
            {
                if (options.protocolVersion >= 28 && (flags & Options.XMIT_EXTENDED_FLAGS) != 0)
                {
                    flags |= (UInt32)(ioStream.readByte() << 8);
                }
                var file = ReceiveFileEntry(flags, clientInfo);
                if (file == null)
                {
                    continue;
                }
                fileList.Add(file);                
                Options.stats.totalSize += (fileList[fileList.Count - 1]).Length;
                EmitFileListProgress(fileList);
                if (options.verbose > 2)
                {
                    Log.WriteLine("receiveFileName(" + (fileList[fileList.Count - 1]).GetFullName() + ")");
                }
            }
            ReceiveFileEntry(0, null);

            if (options.verbose > 2)
            {
                Log.WriteLine("received " + fileList.Count + " names");
            }

            if (ShowFileListProgress())
            {
                FinishFileListProgress(fileList);
            }

            CleanFileList(fileList, options.relativePaths, true);

            if (ioStream != null)
            {
                ioStream.ReadInt();
            }

            if (options.verbose > 3)
            {
                OutputFileList(fileList);
            }
            if (options.listOnly)
            {
                LogFileList(fileList);
            }
            if (options.verbose > 2)
            {
                Log.WriteLine("receiveFileList done");
            }

            Options.stats.fileListSize = (int)(Options.stats.totalRead - startRead);
            Options.stats.numFiles = fileList.Count;
            return fileList;
        }

        public static int FileCompare(FileStruct file1, FileStruct file2)
        {
            return UStringCompare(file1.GetFullName(), file2.GetFullName());
        }

        public static int UStringCompare(string s1, string s2)
        {
            int i = 0;
            while (s1.Length > i && s2.Length > i && s1[i] == s2[i])
            {
                i++;
            }

            if ((s2.Length == s1.Length) && (s1.Length == i) && (s2.Length == i))
            {
                return 0;
            }

            if (s1.Length == i)
            {
                return -s2[i];
            }
            if (s2.Length == i)
            {
                return s1[i];
            }
            return s1[i] - s2[i];
        }

        public static int FileListFind(List<FileStruct> fileList, FileStruct file)
        {
            int low = 0, high = fileList.Count - 1;
            while (high >= 0 && (fileList[high]).BaseName == null)
            {
                high--;
            }
            if (high < 0)
            {
                return -1;
            }
            while (low != high)
            {
                int mid = (low + high) / 2;
                int ret = FileCompare(fileList[FileListUp(fileList, mid)], file);
                if (ret == 0)
                {
                    return FileListUp(fileList, mid);
                }
                if (ret > 0)
                {
                    high = mid;
                }
                else
                {
                    low = mid + 1;
                }
            }

            if (FileCompare(fileList[FileListUp(fileList, low)], file) == 0)
            {
                return FileListUp(fileList, low);
            }
            return -1;
        }

        static int FileListUp(IList<FileStruct> fileList, int i)
        {
            while ((fileList[i]).BaseName == null)
            {
                i++;
            }
            return i;
        }

        public void OutputFileList(List<FileStruct> fileList)
        {
            string uid = String.Empty, gid = String.Empty;
            for (int i = 0; i < fileList.Count; i++)
            {
                FileStruct file = fileList[i];
                if ((options.amRoot || options.amSender) && options.preserveUID)
                {
                    uid = " uid=" + file.Uid;
                }
                if (options.preserveGID && file.Gid != Options.GID_NONE)
                {
                    gid = " gid=" + file.Gid;
                }
                Log.WriteLine("[" + options.WhoAmI() + "] i=" + i + " " + Util.NS(file.BaseDir) + " " +
                    Util.NS(file.DirName) + " " + Util.NS(file.BaseName) + " mode=0" + Convert.ToString(file.Mode, 8) +
                    " len=" + file.Length + uid + gid);
            }
        }

        public void SendFileName(IOStream ioStream, List<FileStruct> fileList, string fileName, bool recursive, UInt32 baseFlags)
        {
            FileStruct file = MakeFile(fileName, fileList, ioStream == null && options.deleteExcluded ? Options.SERVER_EXCLUDES : Options.ALL_EXCLUDES);
            if (file == null)
            {
                return;
            }
            EmitFileListProgress(fileList);
            if (!string.IsNullOrEmpty(file.BaseName))
            {
                fileList.Add(file);
                SendFileEntry(file, ioStream, baseFlags);

                if (recursive && Util.S_ISDIR(file.Mode) && (file.Flags & Options.FLAG_MOUNT_POINT) == 0)
                {
                    options.localExcludeList.Clear();
                    SendDirectory(ioStream, fileList, file.GetFullName());
                }
            }
        }

        /// <summary>
        /// Generates a FileStruct filled with all info
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileList"></param>
        /// <param name="excludeLevel"></param>
        /// <returns></returns>
        public FileStruct MakeFile(string fileName, List<FileStruct> fileList, int excludeLevel)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }
            string thisName = Util.cleanFileName(fileName, false);
            if (options.sanitizePath) //@todo_long It is useless for this moment
            {
                thisName = Util.sanitizePath(thisName, String.Empty, 0);
            }
            FileStruct fileStruct = new FileStruct();
            // TODO: path length
            if (Directory.Exists(thisName))
            {
                if (thisName.LastIndexOf('/') != -1)
                {
                    thisName = thisName.TrimEnd('/');
                    fileStruct.DirName = thisName.Substring(0, thisName.LastIndexOf('/')).Replace(@"\", "/");
                    fileStruct.BaseName = thisName.Substring(thisName.LastIndexOf('/') + 1);
                    fileStruct.Gid = 0;
                    fileStruct.Uid = 0;
                    fileStruct.Mode = 0x4000 | 0x16B;
                    // TODO: path length
                    var di = new DirectoryInfo(thisName);
                    if ((di.Attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
                    {
                        fileStruct.Mode |= 0x92;
                    }
                }

            }
            // TODO: path length
            if (File.Exists(thisName))
            {

                if (excludeLevel != Options.NO_EXCLUDES && CheckExcludeFile(thisName, 0, excludeLevel))
                {
                    return null;
                }
                fileStruct.BaseName = Path.GetFileName(thisName);
                fileStruct.DirName = Path.GetDirectoryName(thisName).Replace(@"\", "/").TrimEnd('/');
                var fi = new FileInfo(thisName);

                // TODO: path length
                fileStruct.Length = (int)fi.Length;
                // TODO: path length
                fileStruct.ModTime = fi.LastWriteTime;
                fileStruct.Mode = 0x8000 | 0x1A4;
                // TODO: path length
                if ((File.GetAttributes(thisName) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
                {
                    fileStruct.Mode |= 0x92;
                }
                fileStruct.Gid = 0;
                fileStruct.Uid = 0;

                int sum_len = options.alwaysChecksum ? CheckSum.MD4_SUM_LENGTH : 0;
                if (sum_len != 0)
                {
                    var fileChecksum = fileStruct.Sum;
                    if (!checkSum.FileCheckSum(thisName, ref fileChecksum, fileStruct.Length))
                    {
                        Log.Write("Skipping file " + thisName);
                        return null;
                    }
                    fileStruct.Sum = fileChecksum;
                }
                Options.stats.totalSize += fileStruct.Length;

            }
            fileStruct.BaseDir = fileListDir;
            return fileStruct;
        }

        public FileStruct ReceiveFileEntry(UInt32 flags, ClientInfo clientInfo)
        {
            if (clientInfo == null)
            {
                lastName = String.Empty;
                return null;
            }
            IOStream f = clientInfo.IoStream;

            int l1 = 0, l2 = 0;

            if ((flags & Options.XMIT_SAME_NAME) != 0)
            {
                l1 = f.readByte();
            }

            l2 = (flags & Options.XMIT_LONG_NAME) != 0 ? f.ReadInt() : f.readByte();
            if (l2 >= Options.MAXPATHLEN - l1)
            {
                MainClass.Exit("overflow: lastname=" + lastName, clientInfo);
            }

            string thisName = lastName.Substring(0, l1);
            thisName += f.ReadStringFromBuffer(l2);
            lastName = thisName;

            thisName = Util.cleanFileName(thisName, false);
            if (options.sanitizePath)
            {
                thisName = Util.sanitizePath(thisName, String.Empty, 0);
            }

            var baseName = String.Empty;
            var dirName = String.Empty;
            if (thisName.LastIndexOf("/") != -1)
            {
                baseName = Path.GetFileName(thisName);
                dirName = Path.GetDirectoryName(thisName);
            }
            else
            {
                baseName = thisName;
                dirName = null;
            }

            var fileLength = f.ReadLongInt();

            if ((flags & Options.XMIT_SAME_TIME) == 0)
            {
                modTime = DateTime.FromFileTime(f.ReadInt());
            }
            if ((flags & Options.XMIT_SAME_MODE) == 0)
            {
                mode = (UInt32)f.ReadInt();
            }

            if (options.preserveUID && (flags & Options.XMIT_SAME_UID) == 0)
            {
                var uid = f.ReadInt();
            }
            if (options.preserveGID && (flags & Options.XMIT_SAME_GID) == 0)
            {
                var gid = f.ReadInt();
            }

            byte[] sum = new byte[0];
            if (options.alwaysChecksum && !Util.S_ISDIR(mode))
            {
                sum = new byte[CheckSum.MD4_SUM_LENGTH];
                sum = f.ReadBuffer(options.protocolVersion < 21 ? 2 : CheckSum.MD4_SUM_LENGTH);
            }

            var fs = new FileStruct
                         {
                             Length = (int) fileLength,
                             BaseName = baseName,
                             DirName = dirName,
                             Sum = sum,
                             Mode = mode,
                             ModTime = modTime,
                             Flags = flags
                         };
            return fs;
        }

        public void SendFileEntry(FileStruct file, IOStream ioStream, UInt32 baseflags)
        {
            var flags = baseflags;
            int l1 = 0, l2 = 0;

            if (ioStream == null)
            {
                return;
            }
            if (file == null)
            {
                ioStream.writeByte(0);
                lastName = String.Empty;
                return;
            }
            var fileName = file.GetFullName().Replace(":", String.Empty);
            for (l1 = 0;
                lastName.Length > l1 && (fileName[l1] == lastName[l1]) && (l1 < 255);
                l1++)
            {

            }
            l2 = fileName.Substring(l1).Length;

            flags |= Options.XMIT_SAME_NAME;

            if (l2 > 255)
            {
                flags |= Options.XMIT_LONG_NAME;
            }
            if (options.protocolVersion >= 28)
            {
                if (flags == 0 && !Util.S_ISDIR(file.Mode))
                {
                    flags |= Options.XMIT_TOP_DIR;
                }
                ioStream.writeByte((byte)flags);
            }
            else
            {
                if ((flags & 0xFF) == 0 && !Util.S_ISDIR(file.Mode))
                {
                    flags |= Options.XMIT_TOP_DIR;
                }
                if ((flags & 0xFF) == 0)
                {
                    flags |= Options.XMIT_LONG_NAME;
                }
                ioStream.writeByte((byte)flags);
            }
            if ((flags & Options.XMIT_SAME_NAME) != 0)
            {
                ioStream.writeByte((byte)l1);
            }
            if ((flags & Options.XMIT_LONG_NAME) != 0)
            {
                ioStream.writeInt(l2);
            }
            else
            {
                ioStream.writeByte((byte)l2);
            }


            byte[] b = System.Text.Encoding.ASCII.GetBytes(fileName);

            ioStream.Write(b, l1, l2);
            ioStream.WriteLongInt(file.Length);


            if ((flags & Options.XMIT_SAME_TIME) == 0)
            {
                ioStream.writeInt(file.ModTime.Second);
            }
            if ((flags & Options.XMIT_SAME_MODE) == 0)
            {
                ioStream.writeInt((int)file.Mode);
            }
            if (options.preserveUID && (flags & Options.XMIT_SAME_UID) == 0)
            {
                ioStream.writeInt(file.Uid);
            }
            if (options.preserveGID && (flags & Options.XMIT_SAME_GID) == 0)
            {
                ioStream.writeInt(file.Gid);
            }
            if (options.alwaysChecksum)
            {
                byte[] sum;
                if (!Util.S_ISDIR(file.Mode))
                {
                    sum = file.Sum;
                }
                else if (options.protocolVersion < 28)
                {
                    sum = new byte[16];
                }
                else
                {
                    sum = null;
                }

                if (sum != null)
                {
                    ioStream.Write(sum, 0, options.protocolVersion < 21 ? 2 : CheckSum.MD4_SUM_LENGTH);
                }

            }

            lastName = fileName;
        }

        public void SendDirectory(IOStream ioStream, List<FileStruct> fileList, string dir)
        {
            var directoryInfo = new DirectoryInfo(dir);
            if (directoryInfo.Exists)
            {
                if (options.cvsExclude)
                {
                    var excl = new Exclude(options);
                    excl.AddExcludeFile(ref options.localExcludeList, dir, (int)(Options.XFLG_WORD_SPLIT & Options.XFLG_WORDS_ONLY)); //@todo (int)(Options.XFLG_WORD_SPLIT & Options.XFLG_WORDS_ONLY) evaluates to 0 unconditionally. May be change & with | ?
                }
                var files = directoryInfo.GetFiles();
                for (var i = 0; i < files.Length; i++)
                {
                    // TODO: path length
                    SendFileName(ioStream, fileList, files[i].FullName.Replace(@"\", "/"), options.recurse, 0);
                }
                var dirs = directoryInfo.GetDirectories();
                for (var i = 0; i < dirs.Length; i++)
                {
                    // TODO: path length
                    SendFileName(ioStream, fileList, dirs[i].FullName.Replace(@"\", "/"), options.recurse, 0);
                }
            }
            else
            {
                Log.WriteLine("Can't find directory '" + Util.fullFileName(dir) + "'");
                return;
            }
        }

        public void CleanFileList(List<FileStruct> fileList, bool stripRoot, bool noDups)
        {
            if (fileList == null || fileList.Count == 0)
            {
                return;
            }
            fileList.Sort(new FileStructComparer());
            for (var i = 0; i < fileList.Count; i++)
            {
                if (fileList[i] == null)
                {
                    fileList.RemoveAt(i);
                }
            }
            if (stripRoot)
            {
                for (int i = 0; i < fileList.Count; i++)
                {
                    if ((fileList[i]).DirName != null && (fileList[i]).DirName[0] == '/')
                    {
                        (fileList[i]).DirName = (fileList[i]).DirName.Substring(1);
                    }
                    if ((fileList[i]).DirName != null && (fileList[i]).DirName.CompareTo(String.Empty) == 0)
                    {
                        (fileList[i]).DirName = null;
                    }
                }

            }
            return;
        }

        private bool ShowFileListProgress()
        {
            return (options.verbose != 0) && (options.recurse || options.filesFrom != null) && !options.amServer;
        }

        private void StartFileListProgress(string kind)
        {
            Log.Write(kind + " ...");
            if (options.verbose > 1 || options.doProgress)
            {
                Log.WriteLine(String.Empty);
            }
        }

        private void FinishFileListProgress(List<FileStruct> fileList)
        {
            if (options.doProgress)
            {
                Log.WriteLine(fileList.Count.ToString() + " file" + (fileList.Count == 1 ? " " : "s ") + "to consider");
            }
            else
            {
                Log.WriteLine("Done.");
            }
        }

        private void EmitFileListProgress(List<FileStruct> fileList)
        {
            if (options.doProgress && ShowFileListProgress() && (fileList.Count % 100) == 0)
            {
                //EmitFileListProgress(fileList);
                Log.WriteLine(" " + fileList.Count + " files...");
            }
        }


        /// <summary>
        /// Write short info about files to log
        /// </summary>
        /// <param name="fileList"></param>
        private static void LogFileList(List<FileStruct> fileList)
        {
            for (int i = 0; i < fileList.Count; i++)
            {
                FileStruct file = fileList[i];
                if (string.IsNullOrEmpty(file.BaseName))
                {
                    continue;
                }
                Log.WriteLine(" " + file.Length + " " + file.ModTime.ToString() + " " + file.GetFullName());
            }            
        }

        /*
         * This function is used to check if a file should be included/excluded
         * from the list of files based on its name and type etc.  The value of
         * exclude_level is set to either SERVER_EXCLUDES or ALL_EXCLUDES.
         */
        private bool CheckExcludeFile(string fileName, int isDir, int excludeLevel)
        {
            int rc;

            if (excludeLevel == Options.NO_EXCLUDES)
            {
                return false;
            }
            if (fileName.CompareTo(String.Empty) != 0)
            {
                /* never exclude '.', even if somebody does --exclude '*' */
                if (fileName[0] == '.' && fileName.Length == 1)
                {
                    return false;
                }
                /* Handle the -R version of the '.' dir. */
                if (fileName[0] == '/')
                {
                    int len = fileName.Length;
                    if (fileName[len - 1] == '.' && fileName[len - 2] == '/')
                    {
                        return true;
                    }
                }
            }
            if (excludeLevel != Options.ALL_EXCLUDES)
            {
                return false;
            }
            var excl = new Exclude(options);
            if (options.excludeList.Count > 0
                && (rc = excl.CheckExclude(options.excludeList, fileName, isDir)) != 0)
            {
                return (rc < 0) ? true : false;
            }
            return false;
        }
    }
}
