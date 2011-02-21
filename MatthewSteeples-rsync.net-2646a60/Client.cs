using System;
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
using System.Collections.Generic;
namespace NetSync
{
    public class Client
    {
        public int ClientRun(ClientInfo clientInfo, int pid, string[] args)
        {
            var options = clientInfo.Options;
            var ioStream = clientInfo.IoStream;
            FileList fList;
            List<FileStruct> fileList;

            MainClass.SetupProtocol(clientInfo);
            if (options.protocolVersion >= 23 && !options.readBatch)
            {
                ioStream.IOStartMultiplexIn();
            }
            if (options.amSender)
            {
                ioStream.IOStartBufferingOut();

                if (options.deleteMode && !options.deleteExcluded)
                {
                    Exclude excl = new Exclude(options);
                    excl.SendExcludeList(ioStream);
                }

                if (options.verbose > 3)
                {
                    Log.Write("File list sent\n");
                }
                ioStream.Flush();
                fList = new FileList(options);
                fileList = fList.SendFileList(clientInfo, args);
                if (options.verbose > 3)
                {
                    Log.WriteLine("file list sent");
                }
                ioStream.Flush();
                Sender sender = new Sender(options);
                sender.SendFiles(fileList, clientInfo);
                ioStream.Flush();
                ioStream.writeInt(-1);
                return -1;
            }
            options.dir = args[0];
            if (options.dir.CompareTo(String.Empty) != 0 && options.dir.IndexOf(':') == -1)
            {
                if (options.dir[0] == '/')
                {
                    options.dir = "c:" + options.dir;
                }
                else if (options.dir.IndexOf('/') != -1)
                {
                    options.dir = options.dir.Insert(options.dir.IndexOf('/') - 1, ":");
                }
            }
            if (!options.readBatch)
            {
                Exclude excl = new Exclude(options);
                excl.SendExcludeList(ioStream);
            }
            fList = new FileList(options);
            fileList = fList.ReceiveFileList(clientInfo);
            return Daemon.DoReceive(clientInfo, fileList, null);
        }
    }
}
