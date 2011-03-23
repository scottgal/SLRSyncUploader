using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading;

namespace HashEngine
{
    public class FileProcessor
    {

        private static ProgressReporter _progressReporter;
        private static CancellationTokenSource _cancellationSource;
        public static ConcurrentBag<FileBlock> GetDiffBlocksForFile(ConcurrentDictionary<long, FileHash> remoteHashes, string filePath, Config config)
        {
            _cancellationSource = new CancellationTokenSource();
            var cancellationToken = _cancellationSource.Token;
            _progressReporter = new ProgressReporter();
            var fileInfo = new FileInfo(filePath);
            var fileChunkSize = Convert.ToInt64(fileInfo.Length / config.DegreeOfParalleism);


            long chunkStart = 0;
            long chunkEnd = 0;
            var resultBag = new ConcurrentBag<FileBlock>();
            using (var mappedFile = MemoryMappedFile.CreateFromFile(filePath))
            {

                var taskArray = new List<Task>();
                int chunkCounter = 0;
                for (var i = 0; i < config.DegreeOfParalleism; i++)
                {
                    chunkEnd = chunkEnd + fileChunkSize;
                    var chunkOverlap = chunkEnd;
                    if (chunkEnd > fileInfo.Length)
                    {
                        chunkEnd = fileInfo.Length;
                        chunkOverlap = chunkEnd;
                    }
                    

                    if (taskArray.Count() <= config.DegreeOfParalleism)
                    {
                        chunkCounter++;
                        var localChunkStart = chunkStart;
                        var length = chunkOverlap - chunkStart;
                        var mappedAccessor = mappedFile.CreateViewAccessor(localChunkStart, length, MemoryMappedFileAccess.Read);
                        var chunkScanTask = Task.Factory.StartNew(
                            () => ScanFileChunk(chunkCounter, cancellationToken, remoteHashes, mappedAccessor, localChunkStart, length, config), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                            .ContinueWith(
                                outHash => outHash.Result.ForEach(resultBag.Add));

                        taskArray.Add(chunkScanTask);

                    }

                    Interlocked.Add(ref chunkStart, fileChunkSize);

                }
                Task.WaitAll(taskArray.ToArray());
            }
            return resultBag;
        }




        public static List<FileBlock> ScanFileChunk(int blockNumber, CancellationToken cancellationToken, 
            ConcurrentDictionary<long, FileHash> remoteHashes, MemoryMappedViewAccessor chunkMap, long chunkStartOffset, long chunkLength, Config config)
        {
            var bytesToRead = config.BlockLength;

            var outList = new List<FileBlock>();
            long readStartOffset = 0;
            var nonMatchStartOffset = readStartOffset;
            var windowChecksum = new Adler32();
            
            while (readStartOffset < chunkLength)
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                var buffer = new byte[bytesToRead];


                if (readStartOffset + config.BlockLength > chunkLength)
                {
                    bytesToRead = checked((int)(chunkLength - readStartOffset));
                    buffer = new byte[bytesToRead];
                }

                chunkMap.ReadArray((int)readStartOffset, buffer, 0, buffer.Length);

                if (readStartOffset == 0)
                {
                    windowChecksum.Update(buffer);
                }
                else
                {
                    windowChecksum.Update(buffer, buffer.Length - 1, 1);
                }

                var weakCheckSum = windowChecksum.Value;

                FileHash match;
                remoteHashes.TryGetValue(weakCheckSum, out match);
                if (match != null)
                {
                    var strongHash = CalculateStrongHash(buffer);
                    if (strongHash.SequenceEqual(match.StrongHash))
                    {
                        if (readStartOffset > 0)
                        {
                            long nonMatchEndOffset = readStartOffset - 1;
                            var nonMatchingChunk = new FileBlock
                                                       {
                                                           IsMatch = false,
                                                           BlockLength = nonMatchEndOffset - nonMatchStartOffset,
                                                           SourceOffset = nonMatchStartOffset + readStartOffset
                                                       };
                            outList.Add(nonMatchingChunk);
                            nonMatchStartOffset = readStartOffset + config.BlockLength;
                        }

                        var matchingChunk = new FileBlock
                                                {
                                                    IsMatch = true,
                                                    DestinationOffset = chunkStartOffset + readStartOffset,
                                                    SourceOffset = match.Offset,
                                                    BlockLength = config.BlockLength
                                                };
                        outList.Add(matchingChunk);
                        windowChecksum.Reset();
                    }
                }
                readStartOffset += 1;

                if (readStartOffset % 100 == 0)
                {
                   
                    _progressReporter.ReportProgress(()=>
                                                         { 
                                                            
                       
                                                         })
                    ;
                }
            }
            if (chunkLength - nonMatchStartOffset > 1)
            {
                var nonMatchingChunk = new FileBlock() { IsMatch = false, BlockLength = chunkLength - nonMatchStartOffset, SourceOffset = nonMatchStartOffset + readStartOffset };
                outList.Add(nonMatchingChunk);
            }
            return outList;
        }


        public static ConcurrentDictionary<long, FileHash> GetHashesForFile(string filePath, Config config)
        {

            var resultDictionary = new ConcurrentDictionary<long, FileHash>();
            if (!File.Exists(filePath)) throw new NullReferenceException("File cannot be found.");

            var fileInfo = new FileInfo(filePath);

            long fileLength = fileInfo.Length;

            int localBlockLength = config.BlockLength;
            if (fileLength <= localBlockLength)
            {
                localBlockLength = checked((int)fileLength);
            }

            long currentFilePosition = 0;
            var bytesToRead = config.BlockLength;
            var buffer = new byte[bytesToRead];
            var procCount = config.DegreeOfParalleism * 15;
            var taskArray = new List<Task>(procCount);
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                while (currentFilePosition < fileLength)
                {
                    if (currentFilePosition + localBlockLength > fileLength)
                    {
                        bytesToRead = checked((int)(fileLength - currentFilePosition));
                        buffer = new byte[bytesToRead];
                    }
                    var offSetPosition = currentFilePosition;

                    if (file.Read(buffer, 0, bytesToRead) < 0)
                    {
                        break;
                    }

                    currentFilePosition = file.Position;

                    if (taskArray.Count <= procCount)
                    {
                        taskArray.Add(Task.Factory.StartNew(() => CalculateHashes(buffer, offSetPosition, bytesToRead, config))
                                          .ContinueWith(
                                              outHash => resultDictionary.TryAdd(outHash.Result.WeakHash, outHash.Result)));

                    }
                    else
                    {
                        var complete = Task.WaitAny(taskArray.ToArray());
                        taskArray.RemoveAt(complete);
                    }



                }

            }
            Task.WaitAll(taskArray.ToArray());

            return resultDictionary;
        }



        private static readonly Adler32 _adler32 = new Adler32();


        public static long CalculateWeakHash(byte[] buffer)
        {

            _adler32.Reset();
            _adler32.Update(buffer);
            var weakHash = _adler32.Value;
            return weakHash;
        }



        public static byte[] CalculateStrongHash(byte[] buffer)
        {
            var md5 = new MD5CryptoServiceProvider();
            var strongHash = md5.ComputeHash(buffer);
            return strongHash;
        }




        public static FileHash CalculateHashes(byte[] buffer, long offset, int length, Config config)
        {

            var weakHash = CalculateWeakHash(buffer);
            var strongHash = CalculateStrongHash(buffer);

            var fileHash = new FileHash() { Length = length, Offset = offset, WeakHash = weakHash, StrongHash = strongHash };


            return fileHash;
        }

    }
}
