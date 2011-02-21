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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NetSync
{
    internal class Sender
    {
        private readonly CheckSum checkSum;
        private readonly NotifyIcon icon = new NotifyIcon();
        private readonly Options options;

        public Sender(Options opt)
        {
            options = opt;
            checkSum = new CheckSum(options);
        }

        public void SendFiles(List<FileStruct> fileList, ClientInfo clientInfo)
        {
            ShowMessage("Processing...");
            try
            {
                IOStream ioStream = clientInfo.IoStream;
                string fileName = String.Empty, fileName2 = String.Empty;
                SumStruct s = null;
                int phase = 0;
                bool saveMakeBackups = options.makeBackups;
                var match = new Match(options);

                if (options.verbose > 2)
                {
                    Log.WriteLine("SendFiles starting");
                }
                while (true)
                {
                    fileName = String.Empty;
                    int i = ioStream.ReadInt();
                    if (i == -1)
                    {
                        if (phase == 0)
                        {
                            phase++;
                            checkSum.length = CheckSum.SUM_LENGTH;
                            ioStream.writeInt(-1);
                            if (options.verbose > 2)
                            {
                                Log.WriteLine("SendFiles phase=" + phase);
                            }
                            options.makeBackups = false;
                            continue;
                        }
                        break;
                    }

                    if (i < 0 || i >= fileList.Count)
                    {
                        MainClass.Exit("Invalid file index " + i + " (count=" + fileList.Count + ")", clientInfo);
                    }

                    FileStruct file = fileList[i];

                    Options.stats.currentFileIndex = i;
                    Options.stats.numTransferredFiles++;
                    Options.stats.totalTransferredSize += file.Length;

                    if (!string.IsNullOrEmpty(file.BaseDir))
                    {
                        fileName = file.BaseDir;
                        if (!fileName.EndsWith("/"))
                        {
                            fileName += "/";
                        }
                    }
                    fileName2 = file.GetFullName();
                    fileName += file.GetFullName();
                    ShowMessage("uploading " + fileName);

                    if (options.verbose > 2)
                    {
                        Log.WriteLine("sendFiles(" + i + ", " + fileName + ")");
                    }

                    if (options.dryRun)
                    {
                        if (!options.amServer && options.verbose != 0)
                        {
                            Log.WriteLine(fileName2);
                        }
                        ioStream.writeInt(i);
                        continue;
                    }

                    Stats initialStats = Options.stats;
                    s = ReceiveSums(clientInfo);

                    Stream fd;
                    try
                    {
                        fd = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    }
                    catch (FileNotFoundException)
                    {
                        Log.WriteLine("file has vanished: " + Util.fullFileName(fileName));
                        s = null;
                        continue;
                    }
                    catch (Exception)
                    {
                        Log.WriteLine("SendFiles failed to open " + Util.fullFileName(fileName));
                        s = null;
                        continue;
                    }

                    var st = new FStat();
                    var fi = new FileInfo(fileName);
                    // TODO: path length
                    st.mTime = fi.LastWriteTime;
                    // TODO: path length
                    st.size = fi.Length;

                    MapFile mbuf = null;
                    if (st.size != 0)
                    {
                        var mapSize = (int) Math.Max(s.bLength*3, Options.MAX_MAP_SIZE);
                        mbuf = new MapFile(fd, (int) st.size, mapSize, (int) s.bLength);
                    }

                    if (options.verbose > 2)
                    {
                        Log.WriteLine("SendFiles mapped " + fileName + " of size " + st.size);
                    }

                    ioStream.writeInt(i);
                    var gen = new Generator(options);
                    gen.WriteSumHead(ioStream, s);

                    if (options.verbose > 2)
                    {
                        Log.WriteLine("calling MatchSums " + fileName);
                    }

                    if (!options.amServer && options.verbose != 0)
                    {
                        Log.WriteLine(fileName2);
                    }

                    var token = new Token(options);
                    token.SetCompression(fileName);

                    match.MatchSums(ioStream, s, mbuf, (int) st.size);
                    Log.LogSend(file, initialStats);

                    if (mbuf != null)
                    {
                        bool j = mbuf.UnMapFile();
                        if (j)
                        {
                            Log.WriteLine("read errors mapping " + Util.fullFileName(fileName));
                        }
                    }
                    fd.Close();

                    s.sums = null;

                    if (options.verbose > 2)
                    {
                        Log.WriteLine("sender finished " + fileName);
                    }
                }
                options.makeBackups = saveMakeBackups;

                if (options.verbose > 2)
                {
                    Log.WriteLine("send files finished");
                }

                match.MatchReport(ioStream);
                ioStream.writeInt(-1);
            }
            finally
            {
                HideMessage();
            }
        }

        public SumStruct ReceiveSums(ClientInfo cInfo)
        {
            var f = cInfo.IoStream;
            var s = new SumStruct();
            int i;
            int offset = 0;
            ReadSumHead(cInfo, ref s);
            s.sums = null;

            if (options.verbose > 3)
            {
                Log.WriteLine("count=" + s.count + " n=" + s.bLength + " rem=" + s.remainder);
            }

            if (s.count == 0)
            {
                return s;
            }

            s.sums = new SumBuf[s.count];

            for (i = 0; i < s.count; i++)
            {
                s.sums[i] = new SumBuf();
                s.sums[i].sum1 = (UInt32) f.ReadInt();
                s.sums[i].sum2 = f.ReadBuffer(s.s2Length);
                s.sums[i].offset = offset;
                s.sums[i].flags = 0;

                if (i == s.count - 1 && s.remainder != 0)
                {
                    s.sums[i].len = s.remainder;
                }
                else
                {
                    s.sums[i].len = s.bLength;
                }
                offset += (int) s.sums[i].len;

                if (options.verbose > 3)
                {
                    Log.WriteLine("chunk[" + i + "] len=" + s.sums[i].len);
                }
            }

            s.fLength = offset;
            return s;
        }

        public void ReadSumHead(ClientInfo clientInfo, ref SumStruct sum)
        {
            IOStream ioStream = clientInfo.IoStream;
            sum.count = ioStream.ReadInt();
            sum.bLength = (UInt32) ioStream.ReadInt();
            if (options.protocolVersion < 27)
            {
                sum.s2Length = checkSum.length;
            }
            else
            {
                sum.s2Length = ioStream.ReadInt();
                if (sum.s2Length > CheckSum.MD4_SUM_LENGTH)
                {
                    MainClass.Exit("Invalid checksum length " + sum.s2Length, clientInfo);
                }
            }
            sum.remainder = (UInt32) ioStream.ReadInt();
        }

        public void ShowMessage(string msg)
        {
            if (!File.Exists("logo.ico"))
            {
                return;
            }

            if (msg.Length > 64)
            {
                msg = msg.Substring(0, 60) + "...";
            }

            icon.Icon = new Icon("logo.ico");
            icon.Text = msg;
            icon.Visible = true;
        }

        public void HideMessage()
        {
            icon.Visible = false;
        }
    }
}