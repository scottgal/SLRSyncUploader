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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetSync
{
    public class ClientInfo
    {
        public Thread ClientThread;
        public IOStream IoStream;
        public Options Options;
    }

    public class TCPSocketListener
    {
        private readonly Socket Client;
        private readonly ClientInfo ClientInfo;
        private readonly List<TCPSocketListener> ClientSockets;
        private Thread ClientThread;

        public TCPSocketListener(Socket client, ref List<TCPSocketListener> clientSockets)
        {
            Client = client;
            ClientSockets = clientSockets;
            ClientInfo = new ClientInfo();
            ClientInfo.Options = new Options();
        }

        public void StartSocketListener()
        {
            if (Client != null)
            {
                ClientThread = new Thread(StartDaemon);
                ClientInfo.IoStream = new IOStream(new NetworkStream(Client));
                ClientInfo.IoStream.ClientThread = ClientThread;
                ClientThread.Start();
            }
        }

        public void StartDaemon()
        {
            string remoteAddr = Client.RemoteEndPoint.ToString();
            remoteAddr = remoteAddr.Substring(0, remoteAddr.IndexOf(':'));
            //string remoteHost = Dns.GetHostByAddress(IPAddress.Parse(remoteAddr)).HostName;
            string remoteHost = Dns.GetHostEntry(IPAddress.Parse(remoteAddr)).HostName;
            ClientInfo.Options.remoteAddr = remoteAddr;
            ClientInfo.Options.remoteHost = remoteHost;

            Daemon.StartDaemon(ClientInfo);
            Client.Close();
            ClientSockets.Remove(this);
        }
    }


    public class Daemon
    {
        private static TcpListener _server;
        private static List<TCPSocketListener> _clientSockets;
        private static bool _stopServer = true;
        public static Options ServerOptions;

        public static Configuration Config;

        public static int DaemonMain(Options options)
        {
            ServerOptions = options;
            Config = new Configuration(ServerOptions.configFile);
            if (Config.LoadParm(options))
            {
                StartAcceptLoop(options.rsyncPort);
            }
            return -1;
        }

        public static void StartAcceptLoop(int port)
        {
            var localAddr = IPAddress.Parse(ServerOptions.bindAddress);
            _server = new TcpListener(localAddr, port); 


            try
            {
                _server.Start();
            }
            catch (Exception)
            {
                MainClass.Exit("Can't listening address " + ServerOptions.bindAddress + " on port " + port, null);
                Environment.Exit(0);
            }
            Log.WriteLine("WinRSyncd starting, listening on port " + port);
            _stopServer = false;
            _clientSockets = new List<TCPSocketListener>();
            while (!_stopServer)
            {
                try
                {
                    Socket soc = _server.AcceptSocket();
                    if (!Config.LoadParm(ServerOptions))
                    {
                        continue;
                    }
                    var socketListener = new TCPSocketListener(soc, ref _clientSockets);
                    lock (_clientSockets)
                    {
                        _clientSockets.Add(socketListener);
                    }
                    socketListener.StartSocketListener();
                    for (var i = 0; i < _clientSockets.Count; i++)
                    {
                        if (_clientSockets[i] == null)
                        {
                            _clientSockets.RemoveAt(i);
                        }
                    }
                }
                catch (SocketException)
                {
                    _stopServer = true;
                }
            }
            if (ServerOptions.logFile != null)
            {
                ServerOptions.logFile.Close();
            }
        }

        public static int StartDaemon(ClientInfo clientInfo)
        {
            var stream = clientInfo.IoStream;
            var options = clientInfo.Options;
            options.amDaemon = true;

            stream.IOPrintf("@RSYNCD: " + options.protocolVersion + "\n");
            string line = stream.ReadLine();
            try
            {
                options.remoteProtocol = Int32.Parse(line.Substring(9, 2));
            }
            catch
            {
                options.remoteProtocol = 0;
            }
            bool isValidstring = line.StartsWith("@RSYNCD: ") && line.EndsWith("\n") && options.remoteProtocol > 0;
            if (!isValidstring)
            {
                stream.IOPrintf("@ERROR: protocol startup error\n");
                return -1;
            }
            if (options.protocolVersion > options.remoteProtocol)
            {
                options.protocolVersion = options.remoteProtocol;
            }
            line = stream.ReadLine();
            if (line.CompareTo("#list\n") == 0)
            {
                ClientServer.SendListing(stream);
                return -1;
            }

            if (line[0] == '#')
            {
                stream.IOPrintf("@ERROR: Unknown command '" + line.Replace("\n", String.Empty) + "'\n");
                return -1;
            }

            int i = Config.GetNumberModule(line.Replace("\n", String.Empty));
            if (i < 0)
            {
                stream.IOPrintf("@ERROR: Unknown module " + line);
                MainClass.Exit("@ERROR: Unknown module " + line, clientInfo);
            }
            options.doStats = true;
            options.ModuleId = i;
            ClientServer.RsyncModule(clientInfo, i);
            clientInfo.IoStream.Close();
            return 1;
        }

        public static void StartServer(ClientInfo cInfo, string[] args)
        {
            var f = cInfo.IoStream;
            var options = cInfo.Options;

            if (options.protocolVersion >= 23)
            {
                f.IOStartMultiplexOut();
            }
            if (options.amSender)
            {
                options.keepDirLinks = false; /* Must be disabled on the sender. */
                var excl = new Exclude(options);
                excl.ReceiveExcludeList(f);
                DoServerSender(cInfo, args);
            }
            else
            {
                DoServerReceive(cInfo, args);
            }
        }

        private static void DoServerSender(ClientInfo clientInfo, string[] args)
        {
            var dir = args[0];
            var ioStream = clientInfo.IoStream;
            var options = clientInfo.Options;

            if (options.verbose > 2)
            {
                Log.Write("Server sender starting");
            }
            if (options.amDaemon && Config.ModuleIsWriteOnly(options.ModuleId))
            {
                MainClass.Exit("ERROR: module " + Config.GetModuleName(options.ModuleId) + " is write only", clientInfo);
                return;
            }

            if (!options.relativePaths && !Util.pushDir(dir))
            {
                MainClass.Exit("Push_dir#3 " + dir + "failed", clientInfo);
                return;
            }

            var fList = new FileList(options);
            var fileList = fList.SendFileList(clientInfo, args);
            if (options.verbose > 3)
            {
                Log.WriteLine("File list sent");
            }
            if (fileList.Count == 0)
            {
                MainClass.Exit("File list is empty", clientInfo);
                return;
            }
            ioStream.IOStartBufferingIn();
            ioStream.IOStartBufferingOut();

            var sender = new Sender(options);
            sender.SendFiles(fileList, clientInfo);
            ioStream.Flush();
            MainClass.Report(clientInfo);
            if (options.protocolVersion >= 24)
            {
                ioStream.ReadInt();
            }
            ioStream.Flush();
        }

        public static void DoServerReceive(ClientInfo cInfo, string[] args)
        {
            var options = cInfo.Options;
            var f = cInfo.IoStream;
            if (options.verbose > 2)
            {
                Log.Write("Server receive starting");
            }
            if (options.amDaemon && Config.ModuleIsReadOnly(options.ModuleId))
            {
                MainClass.Exit("ERROR: module " + Config.GetModuleName(options.ModuleId) + " is read only", cInfo);
                return;
            }

            f.IOStartBufferingIn();
            if (options.deleteMode && !options.deleteExcluded)
            {
                var excl = new Exclude(options);
                excl.ReceiveExcludeList(f);
            }

            var fList = new FileList(cInfo.Options);
            var fileList = fList.ReceiveFileList(cInfo);
            DoReceive(cInfo, fileList, null);
        }

        public static int DoReceive(ClientInfo cInfo, List<FileStruct> fileList, string localName)
        {
            var f = cInfo.IoStream;
            var options = cInfo.Options;
            var receiver = new Receiver(options);

            options.copyLinks = false;
            f.Flush();
            if (!options.deleteAfter)
            {
                if (options.recurse && options.deleteMode && localName == null && fileList.Count > 0)
                {
                    receiver.DeleteFiles(fileList);
                }
            }
            f.IOStartBufferingOut();
            var gen = new Generator(options);
            gen.GenerateFiles(f, fileList, localName);
            f.Flush();
            if (fileList != null && fileList.Count != 0)
            {
                receiver.ReceiveFiles(cInfo, fileList, localName);
            }
            MainClass.Report(cInfo);
            if (options.protocolVersion >= 24)
            {
                /* send a final goodbye message */
                f.writeInt(-1);
            }
            f.Flush();
            return 0;
        }
    }
}