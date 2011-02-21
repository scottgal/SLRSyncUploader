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
    internal class CommandLineParser
    {
        public static int ParseArguments(string[] args, Options options)
        {
            int argsNotUsed = 0;
            int i = 0;
            var excl = new Exclude(options);
            while (i < args.Length)
            {
                try
                {
                    switch (args[i])
                    {
                        case "--version":
                            MainClass.PrintRsyncVersion();
                            MainClass.Exit(String.Empty, null);
                            break;
                        case "--suffix":
                            options.backupSuffix = args[++i];
                            break;
                        case "--rsync-path":
                            options.rsyncPath = args[++i];
                            break;
                        case "--password-file":
                            options.passwordFile = args[++i];
                            break;
                        case "--ignore-times":
                        case "-I":
                            options.ignoreTimes = true;
                            break;
                        case "--size-only":
                            options.sizeOnly = true;
                            break;
                        case "--modify-window":
                            options.usingModifyWindow = true;
                            options.modifyWindow = Convert.ToInt32(args[++i]);
                            break;
                        case "--one-file-system":
                        case "-x":
                            options.oneFileSystem = true;
                            break;
                        case "--delete":
                            options.deleteMode = true;
                            break;
                        case "--existing":
                            options.onlyExisting = true;
                            break;
                        case "--ignore-existing":
                            options.optIgnoreExisting = true;
                            break;
                        case "--delete-after":
                            options.deleteMode = true;
                            options.deleteAfter = true;
                            break;
                        case "--delete-excluded":
                            options.deleteMode = true;
                            options.deleteExcluded = true;
                            break;
                        case "--force":
                            options.forceDelete = true;
                            break;
                        case "--numeric-ids":
                            options.numericIds = true;
                            break;
                        case "--exclude":
                            excl.AddExclude(ref options.excludeList, args[++i], 0);
                            break;
                        case "--include":
                            excl.AddExclude(ref options.excludeList, args[++i], (int) Options.XFLG_DEF_INCLUDE);
                            options.forceDelete = true;
                            break;
                        case "--exclude-from":
                        case "--include-from":
                            string arg = args[i];
                            excl.AddExcludeFile(ref options.excludeList, args[++i],
                                                (arg.CompareTo("--exclude-from") == 0)
                                                    ? 0
                                                    : (int) Options.XFLG_DEF_INCLUDE);
                            break;
                        case "--safe-links":
                            options.safeSymlinks = true;
                            break;
                        case "--help":
                        case "-h":
                            MainClass.Usage();
                            MainClass.Exit(String.Empty, null);
                            break;
                        case "--backup":
                        case "-b":
                            options.makeBackups = true;
                            break;
                        case "--dry-run":
                        case "-n":
                            options.dryRun = true;
                            break;
                        case "--sparse":
                        case "-S":
                            options.sparseFiles = true;
                            break;
                        case "--cvs-exclude":
                        case "-C":
                            options.cvsExclude = true;
                            break;
                        case "--update":
                        case "-u":
                            options.updateOnly = true;
                            break;
                        case "--inplace":
                            options.inplace = true;
                            break;
                        case "--keep-dirlinks":
                        case "-K":
                            options.keepDirLinks = true;
                            break;
                        case "--links":
                        case "-l":
                            options.preserveLinks = true;
                            break;
                        case "--copy-links":
                        case "-L":
                            options.copyLinks = true;
                            break;
                        case "--whole-file":
                        case "-W":
                            options.wholeFile = 1;
                            break;
                        case "--no-whole-file":
                            options.wholeFile = 0;
                            break;
                        case "--copy-unsafe-links":
                            options.copyUnsafeLinks = true;
                            break;
                        case "--perms":
                        case "-p":
                            options.preservePerms = true;
                            break;
                        case "--owner":
                        case "-o":
                            options.preserveUID = true;
                            break;
                        case "--group":
                        case "-g":
                            options.preserveGID = true;
                            break;
                        case "--devices":
                        case "-D":
                            options.preserveDevices = true;
                            break;
                        case "--times":
                        case "-t":
                            options.preserveTimes = true;
                            break;
                        case "--checksum":
                        case "-c":
                            options.alwaysChecksum = true;
                            break;
                        case "--verbose":
                        case "-v":
                            options.verbose++;
                            break;
                        case "--quiet":
                        case "-q":
                            options.quiet++;
                            break;
                        case "--archive":
                        case "-a":
                            options.archiveMode = true;
                            break;
                        case "--server":
                            options.amServer = true;
                            break;
                        case "--sender":
                            options.amSender = true;
                            break;
                        case "--recursive":
                        case "-r":
                            options.recurse = true;
                            break;
                        case "--relative":
                        case "-R":
                            options.relativePaths = true;
                            break;
                        case "--no-relative":
                            options.relativePaths = false;
                            break;
                        case "--rsh":
                        case "-e":
                            options.shellCmd = args[++i];
                            break;
                        case "--block-size":
                        case "-B":
                            options.blockSize = Convert.ToInt32(args[++i]);
                            break;
                        case "--max-delete":
                            options.maxDelete = Convert.ToInt32(args[++i]);
                            break;
                        case "--timeout":
                            options.ioTimeout = Convert.ToInt32(args[++i]);
                            break;
                        case "--temp-dir":
                        case "-T":
                            options.tmpDir = args[++i];
                            break;
                        case "--compare-dest":
                            options.compareDest = args[++i];
                            break;
                        case "--link-dest":
                            options.compareDest = args[++i];
                            break;
                        case "--compress":
                        case "-z":
                            options.doCompression = true;
                            break;
                        case "--stats":
                            options.doStats = true;
                            break;
                        case "--progress":
                            options.doProgress = true;
                            break;
                        case "--partial":
                            options.keepPartial = true;
                            break;
                        case "--partial-dir":
                            options.partialDir = args[++i];
                            break;
                        case "--ignore-errors":
                            options.ignoreErrors = true;
                            break;
                        case "--blocking-io":
                            options.blockingIO = 1;
                            break;
                        case "--no-blocking-io":
                            options.blockingIO = 0;
                            break;
                        case "-P":
                            options.doProgress = true;
                            options.keepPartial = true;
                            break;
                        case "--log-format":
                            options.logFormat = args[++i];
                            break;
                        case "--bwlimit":
                            options.bwLimit = Convert.ToInt32(args[++i]);
                            break;
                        case "--backup-dir":
                            options.backupDir = args[++i];
                            break;
                        case "--hard-links":
                        case "-H":
                            options.preserveHardLinks = true;
                            break;
                        case "--read-batch":
                            options.batchName = args[++i];
                            options.readBatch = true;
                            break;
                        case "--write-batch":
                            options.batchName = args[++i];
                            options.writeBatch = true;
                            break;
                        case "--files-from":
                            options.filesFrom = args[++i];
                            break;
                        case "--from0":
                            options.eolNulls = true;
                            break;
                        case "--no-implied-dirs":
                            options.impliedDirs = true;
                            break;
                        case "--protocol":
                            options.protocolVersion = Convert.ToInt32(args[++i]);
                            break;
                        case "--checksum-seed":
                            options.checksumSeed = Convert.ToInt32(args[++i]);
                            break;
                        case "--daemon":
                            options.amDaemon = true;
                            break;
                        case "--address":
                            options.bindAddress = args[++i];
                            break;
                        case "--port":
                            options.rsyncPort = Convert.ToInt32(args[++i]);
                            break;
                        case "--config":
                            options.configFile = args[++i].Trim();
                            break;
                        default:
                            {
                                argsNotUsed += ParseMergeArgs(args[i], options);
                                break;
                            }
                    }
                    i++;
                }
                catch
                {
                    return -1;
                }
            }
            if (options.amSender && !options.amServer)
            {
                MainClass.Usage();
                MainClass.Exit(String.Empty, null);
            }
            if (options.ioTimeout > 0 && options.ioTimeout < options.selectTimeout)
            {
                options.selectTimeout = options.ioTimeout;
            }
            return argsNotUsed;
        }

        private static int ParseMergeArgs(string MergeArgs, Options options)
        {
            if (MergeArgs != null && MergeArgs.StartsWith("-") && MergeArgs.Substring(1).IndexOf('-') == -1)
            {
                MergeArgs = MergeArgs.Substring(1);
                var args = new string[MergeArgs.Length];
                for (int i = 0; i < MergeArgs.Length; i++)
                {
                    args[i] = "-" + MergeArgs[i];
                }
                return ParseArguments(args, options);
            }
            return 1;
        }
    }


    public class Stats
    {
        public int currentFileIndex;
        public int fileListSize;
        public Int64 literalData;
        public Int64 matchedData;
        public int numFiles;
        public int numTransferredFiles;
        public Int64 totalRead;
        public Int64 totalSize;
        public Int64 totalTransferredSize;
        public Int64 totalWritten;
    }

    public class Options
    {
        /// <summary>
        /// "rsync://"
        /// </summary>
        public const string URL_PREFIX = "rsync://";

        /// <summary>
        /// 873
        /// </summary>
        public const int RSYNC_PORT = 873;

        /// <summary>
        /// 1024
        /// </summary>
        public const int MAXPATHLEN = 1024;

        /// <summary>
        /// 700
        /// </summary>
        public const int BLOCK_SIZE = 700;

        /// <summary>
        /// 1000
        /// </summary>
        public const int MAX_ARGS = 1000;

        /// <summary>
        /// 20
        /// </summary>
        public const int MIN_PROTOCOL_VERSION = 20;

        /// <summary>
        /// 25
        /// </summary>
        public const int OLD_PROTOCOL_VERSION = 25;

        /// <summary>
        /// 40
        /// </summary>
        public const int MAX_PROTOCOL_VERSION = 40;

        /// <summary>
        /// (256 * 1024)
        /// </summary>
        public const int MAX_MAP_SIZE = (256*1024);

        /// <summary>
        /// (1 &lt;&lt; 0)
        /// </summary>
        public const int FLAG_TOP_DIR = (1 << 0);

        /// <summary>
        /// (1 &lt;&lt; 1)
        /// </summary>
        public const int FLAG_HLINK_EOL = (1 << 1); /* generator only */

        /// <summary>
        /// (1 &lt;&lt; 2)
        /// </summary>
        public const int FLAG_MOUNT_POINT = (1 << 2); /* sender only */

        /// <summary>
        /// 0
        /// </summary>
        public const int NO_EXCLUDES = 0;

        /// <summary>
        /// 1
        /// </summary>
        public const int SERVER_EXCLUDES = 1;

        /// <summary>
        /// 2
        /// </summary>
        public const int ALL_EXCLUDES = 2;

        /// <summary>
        /// 0
        /// </summary>
        public const int GID_NONE = 0;

        /// <summary>
        /// (1 &lt;&lt; 0)
        /// </summary>
        public const UInt32 XFLG_FATAL_ERRORS = (1 << 0);

        /// <summary>
        /// (1 &lt;&lt; 1)
        /// </summary>
        public const UInt32 XFLG_DEF_INCLUDE = (1 << 1);

        /// <summary>
        /// (1 &lt;&lt; 2)
        /// </summary>
        public const UInt32 XFLG_WORDS_ONLY = (1 << 2);

        /// <summary>
        /// (1 &lt;&lt; 3)
        /// </summary>
        public const UInt32 XFLG_WORD_SPLIT = (1 << 3);

        /// <summary>
        /// (1 &lt;&lt; 4)
        /// </summary>
        public const UInt32 XFLG_DIRECTORY = (1 << 4);

        /// <summary>
        /// (1 &lt;&lt; 0)
        /// </summary>
        public const UInt32 MATCHFLG_WILD = (1 << 0); /* pattern has '*', '[', and/or '?' */

        /// <summary>
        /// 
        /// </summary>
        public const UInt32 MATCHFLG_WILD2 = (1 << 1); /* pattern has '**' */

        /// <summary>
        /// (1 &lt;&lt; 2)
        /// </summary>
        public const UInt32 MATCHFLG_WILD2_PREFIX = (1 << 2); /* pattern starts with '**' */

        /// <summary>
        /// (1 &lt;&lt; 3)
        /// </summary>
        public const UInt32 MATCHFLG_ABS_PATH = (1 << 3); /* path-match on absolute path */

        /// <summary>
        /// (1 &lt;&lt; 4)
        /// </summary>
        public const UInt32 MATCHFLG_INCLUDE = (1 << 4); /* this is an include, not an exclude */

        /// <summary>
        /// (1 &lt;&lt; 5)
        /// </summary>
        public const UInt32 MATCHFLG_DIRECTORY = (1 << 5); /* this matches only directories */

        /// <summary>
        /// (1 &lt;&lt; 6)
        /// </summary>
        public const UInt32 MATCHFLG_CLEAR_LIST = (1 << 6); /* this item is the "!" token */

        /// <summary>
        /// (1 &lt;&lt; 0)
        /// </summary>
        public const UInt32 XMIT_TOP_DIR = (1 << 0);

        /// <summary>
        /// (1 &lt;&lt; 1)
        /// </summary>
        public const UInt32 XMIT_SAME_MODE = (1 << 1);

        /// <summary>
        /// (1 &lt;&lt; 2)
        /// </summary>
        public const UInt32 XMIT_EXTENDED_FLAGS = (1 << 2);

        /// <summary>
        /// XMIT_EXTENDED_FLAGS = (1 &lt;&lt; 2)
        /// </summary>
        public const UInt32 XMIT_SAME_RDEV_pre28 = XMIT_EXTENDED_FLAGS; /* Only in protocols < 28 */

        /// <summary>
        /// (1 &lt;&lt; 3)
        /// </summary>
        public const UInt32 XMIT_SAME_UID = (1 << 3);

        /// <summary>
        /// (1 &lt;&lt; 4)
        /// </summary>
        public const UInt32 XMIT_SAME_GID = (1 << 4);

        /// <summary>
        /// (1 &lt;&lt; 5)
        /// </summary>
        public const UInt32 XMIT_SAME_NAME = (1 << 5);

        /// <summary>
        /// (1 &lt;&lt; 6)
        /// </summary>
        public const UInt32 XMIT_LONG_NAME = (1 << 6);

        /// <summary>
        /// (1 &lt;&lt; 7)
        /// </summary>
        public const UInt32 XMIT_SAME_TIME = (1 << 7);

        /// <summary>
        /// (1 &lt;&lt; 8)
        /// </summary>
        public const UInt32 XMIT_SAME_RDEV_MAJOR = (1 << 8);

        /// <summary>
        /// (1 &lt;&lt; 9)
        /// </summary>
        public const UInt32 XMIT_HAS_IDEV_DATA = (1 << 9);

        /// <summary>
        /// (1 &lt;&lt; 10)
        /// </summary>
        public const UInt32 XMIT_SAME_DEV = (1 << 10);

        /// <summary>
        /// (1 &lt;&lt; 11)
        /// </summary>
        public const UInt32 XMIT_RDEV_MINOR_IS_SMALL = (1 << 11);

        public static DateTime lastIO = DateTime.MinValue;
        //public static System.IO.StreamReader filesFromFD = null;
        /// <summary>
        /// Seems to be null all the time
        /// </summary>
        public static Stream filesFromFD;

        public static Stats stats = new Stats();
        public int ModuleId = -1;
        public bool alwaysChecksum;
        public bool amDaemon;
        public bool amGenerator;
        public bool amRoot = true;
        public bool amSender;
        public bool amServer;
        public bool archiveMode;
        public string backupDir;
        public string backupSuffix;
        public string batchName;
        public string bindAddress = "127.0.0.1";
        public int blockSize;
        public int blockingIO = -1;
        public int bwLimit;
        public int checksumSeed;
        public string compareDest;
        public string configFile = "rsyncd.conf";
        public bool copyLinks;
        public bool copyUnsafeLinks;
        public bool cvsExclude;
        public bool daemonOpt;
        public bool delayUpdates;
        public bool deleteAfter;
        public bool deleteExcluded;
        public bool deleteMode;
        public string dir = String.Empty;
        public bool doCompression;
        public bool doProgress;
        public bool doStats;
        public bool dryRun;
        public bool eolNulls;

        //
        public List<ExcludeStruct> excludeList = new List<ExcludeStruct>();
        public string excludePathPrefix;
        public string filesFrom;
        public bool forceDelete;
        public bool ignoreErrors;

        public bool ignoreTimes;
        public bool impliedDirs;
        public bool inplace;
        public int ioTimeout;
        public bool keepDirLinks;
        public bool keepPartial;
        public bool listOnly;
        public List<ExcludeStruct> localExcludeList = new List<ExcludeStruct>();
        public FileStream logFile;
        public string logFormat;
        public bool makeBackups;
        public int maxDelete;

        /// <summary>
        /// Allowed difference between two files modification time
        /// </summary>
        public int modifyWindow;

        public bool noDetach;
        public bool numericIds;

        public bool oneFileSystem;
        public bool onlyExisting;
        public bool optIgnoreExisting;
        public string partialDir;
        public string passwordFile;
        public bool preserveDevices;
        public bool preserveGID;
        public bool preserveHardLinks;
        public bool preserveLinks;
        public bool preservePerms;
        public bool preserveTimes;
        public bool preserveUID;
        public int protocolVersion = 28;
        public int quiet;
        public bool readBatch;

        public bool readOnly;
        public bool recurse;
        public bool relativePaths = true; //changed to bool and set true as init value
        public string remoteAddr;
        public string remoteFilesFromFile;
        public string remoteHost;
        public int remoteProtocol;
        public string rsyncPath;
        public int rsyncPort = RSYNC_PORT;
        public bool safeSymlinks;
        public bool sanitizePath;
        public int selectTimeout;
        public List<ExcludeStruct> serverExcludeList = new List<ExcludeStruct>();
        public string shellCmd;
        public bool sizeOnly;
        public Stream sockFIn;
        public Stream sockFOut;
        public bool sparseFiles;
        public DateTime startTime = DateTime.Now;
        public string tmpDir;
        public bool updateOnly;
        public bool usingModifyWindow;
        public int verbose;
        public int wholeFile = -1;
        public bool writeBatch;

        public void Init()
        {
            excludeList.Add(new ExcludeStruct(String.Empty, 0, 0));
            localExcludeList.Add(new ExcludeStruct("per-dir .cvsignore ", 0, 0));
            serverExcludeList.Add(new ExcludeStruct("server ", 0, 0));
        }

        public string WhoAmI()
        {
            return amSender ? "sender" : amGenerator ? "generator" : "receiver";
        }

        public int ServerOptions(string[] args)
        {
            int argc = 0;
            args[argc++] = "--server";
            for (int i = 0; i < verbose; i++)
            {
                args[argc++] = "-v";
            }


            args[argc++] = "-R";
            if (alwaysChecksum)
            {
                args[argc++] = "-c";
            }
            if (recurse)
            {
                args[argc++] = "-r";
            }

            if (amSender)
            {
                if (deleteExcluded)
                {
                    args[argc++] = "--delete-excluded";
                }
                else if (deleteMode)
                {
                    args[argc++] = "--delete";
                }

                if (deleteAfter)
                {
                    args[argc++] = "--delete-after";
                }

                if (forceDelete)
                {
                    args[argc++] = "--force";
                }
            }

            if (!amSender)
            {
                args[argc++] = "--sender";
            }

            return argc;
        }
    }
}