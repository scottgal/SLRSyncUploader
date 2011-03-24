using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading;

namespace PicoDeltaSl
{
    public class FileProcessor
    {
        public event DiffBlockScanProgressChangedDelegate DiffBlockScanProgressChanged;
        public delegate void  DiffBlockScanProgressChangedDelegate(int chunkCount, int percentProgress);



        public void FileBlocksAssemble(List<FileBlock> files, ConcurrentDictionary<long, FileHash> destinationHashes,  string originalFIleName, string destinationPath)
        { 
            
        }

        private  CancellationTokenSource _cancellationSource;
        public  List<FileBlock> GetDiffBlocksForFile(ConcurrentDictionary<long, FileHash> remoteHashes, string filePath, ProgressReporter progressReporter, Config config)
        {
            _cancellationSource = new CancellationTokenSource();
            var cancellationToken = _cancellationSource.Token;
  
            var fileInfo = new FileInfo(filePath);
            var fileChunkSize = Convert.ToInt64(fileInfo.Length / config.DegreeOfParalleism);


            long chunkStart = 0;
            long chunkEnd = 0;
            var resultBag = new List<FileBlock>();
            using (var mappedFile =new FileStream(filePath, FileMode.Open,FileAccess.Read,FileShare.Read))
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
                        
                        int counter = chunkCounter;
                        var chunkScanTask = Task.Factory.StartNew(
                            () => GetChunksForFileBlock(counter, cancellationToken, remoteHashes, mappedFile, localChunkStart, length, progressReporter, config),
                            cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default)
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




        public  List<FileBlock> GetChunksForFileBlock(int blockNumber, CancellationToken cancellationToken, 
            ConcurrentDictionary<long, FileHash> remoteHashes, FileStream mappedFile, long fileBlockChunkStartOffset, long chunkLength, ProgressReporter progressReporter, Config config)
        {
            
            var chunkBuffer = FillBuffer(mappedFile, fileBlockChunkStartOffset, config);
            var windowSize = config.BlockLength;
            

            //our position in the chunk buffer
            long chunkBufferReadOffset = 0;

            //out position in the actual file block.
            var chunkReadOffset = fileBlockChunkStartOffset;

            //lets us track the 'non matched' blocks.
            var nonMatchStartOffset = chunkBufferReadOffset;


            var windowWeakChecksum = new Adler32();

            var outList = new List<FileBlock>();
            while (chunkReadOffset < chunkLength)
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                var buffer = new byte[windowSize];

                if (chunkBufferReadOffset + config.BlockLength >= chunkBuffer.Length)
                {
                    FillBuffer(mappedFile, chunkReadOffset, config);
                    chunkBufferReadOffset = 0;
                }
                //correct for 'end of block' so the buffer array is the right length.
                if (chunkReadOffset + config.BlockLength > chunkLength)
                {
                    windowSize = checked((int)(chunkLength - chunkReadOffset));
                    buffer = new byte[windowSize];
                }

                

                 Array.ConstrainedCopy(chunkBuffer,(int)chunkBufferReadOffset,buffer,0,windowSize);

                if (chunkBufferReadOffset == 0)
                {
                    windowWeakChecksum.Update(buffer);
                }
                else
                {
                    windowWeakChecksum.Update(buffer, buffer.Length - 1, 1);
                }

                var weakCheckSum = windowWeakChecksum.Value;

                FileHash match;
                remoteHashes.TryGetValue(weakCheckSum, out match);
                if (match != null)
                {
                    var strongHash = CalculateStrongHash(buffer);
                    if (strongHash.SequenceEqual(match.StrongHash))
                    {
                        if (chunkBufferReadOffset > 0)
                        {
                            long nonMatchEndOffset = fileBlockChunkStartOffset - 1;
                            var nonMatchingChunk = new FileBlock
                                                       {
                                                           IsMatch = false,
                                                           BlockLength = nonMatchEndOffset - nonMatchStartOffset,
                                                           SourceOffset = nonMatchStartOffset + fileBlockChunkStartOffset
                                                       };
                            outList.Add(nonMatchingChunk);
                            nonMatchStartOffset = chunkBufferReadOffset + config.BlockLength;
                        }

                        var matchingChunk = new FileBlock
                                                {
                                                    IsMatch = true,
                                                    DestinationOffset = chunkReadOffset,
                                                    SourceOffset = match.Offset,
                                                    BlockLength = config.BlockLength
                                                };
                        outList.Add(matchingChunk);
                        windowWeakChecksum.Reset();
                    }
                }
                

                if (chunkBufferReadOffset % 100 == 0)
                {
                    long offset = chunkBufferReadOffset;
                    progressReporter.ReportProgress(()=>
                                                         {
                                                             if (DiffBlockScanProgressChanged != null)
                                                                 DiffBlockScanProgressChanged(blockNumber,
                                                                                 
                                                                                     ((int)
                                                                                      (offset/chunkLength*100)));
                       
                                                         })
                    ;
                }
                chunkBufferReadOffset += 1;
                chunkReadOffset += 1;
            }
            if (chunkLength - nonMatchStartOffset > 1)
            {
                var nonMatchingChunk = new FileBlock() { IsMatch = false, BlockLength = chunkLength - nonMatchStartOffset, SourceOffset = nonMatchStartOffset + chunkBufferReadOffset };
                outList.Add(nonMatchingChunk);
            }
            return outList;
        }


        private byte[] FillBuffer(Stream fileStream, long startOffset, Config config)
        {
            var outArray = new byte[config.BufferSize];
            fileStream.Seek(startOffset, SeekOrigin.Begin);
            fileStream.Read(outArray, 0, outArray.Length);
            return outArray;
        }
        public ConcurrentDictionary<long, FileHash> GetHashesForFile(string filepath, ProgressReporter progressReporter, Config config)
          {
              using (var fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
              {
                  return GetHashesForFile(fileStream,progressReporter, config);
              }
          }


        public event GetHashesForFileBockCompleteDelegate GetHashesForFileBockComplete;
        public delegate void GetHashesForFileBockCompleteDelegate(int blockNumber, int blockCount);

          public ConcurrentDictionary<long, FileHash> GetHashesForFile(FileStream file, ProgressReporter progressReporter, Config config)
        {

            var resultDictionary = new ConcurrentDictionary<long, FileHash>();


            var fileLength = file.Length;

            var localBlockLength = config.BlockLength;
            if (fileLength <= localBlockLength)
            {
                localBlockLength = checked((int)fileLength);
            }

            long currentFilePosition = 0;
            var bytesToRead = config.BlockLength;
              var numberOfBlocks = checked((int) fileLength/config.BlockLength);
            var buffer = new byte[bytesToRead];
            var maxTaskCount = config.DegreeOfParalleism * 15;


            if (maxTaskCount >= numberOfBlocks)
                maxTaskCount = numberOfBlocks;

            var taskArray = new List<Task>(maxTaskCount);

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
                    var taskCount = 0;
                    if (taskArray.Count <= maxTaskCount)
                    {
                        taskCount++;
                        byte[] taskBuffer = buffer;
                        int read = bytesToRead;
                        taskArray.Add(Task.Factory.StartNew(() => CalculateHashes(taskBuffer, offSetPosition, read, config))
                                          .ContinueWith(
                                              outHash =>
                                                  {
                                                      resultDictionary.TryAdd(outHash.Result.WeakHash, outHash.Result);

                                                      progressReporter.ReportProgressAsync(
                                                          () =>
                                                              {
                                                                  if (GetHashesForFileBockComplete != null)
                                                                      GetHashesForFileBockComplete(taskCount,
                                                                                                   numberOfBlocks);
                                                              });

                                                  }));
                    }
                    else
                    {
                        var complete = Task.WaitAny(taskArray.ToArray());


                        taskArray.RemoveAt(complete);
                    }
                }

            
            Task.WaitAll(taskArray.ToArray());

            return resultDictionary;
        }



        private static readonly Adler32 _adler32 = new Adler32();


        public  long CalculateWeakHash(byte[] buffer)
        {

            _adler32.Reset();
            _adler32.Update(buffer);
            var weakHash = _adler32.Value;
            return weakHash;
        }



        public  byte[] CalculateStrongHash(byte[] buffer)
        {
            
            var strongHashAlgo = new SHA256Managed();
            var strongHash = strongHashAlgo.ComputeHash(buffer);
            return strongHash;
        }


        public byte[] CalculateStrongHash(Stream inputStream)
        {

            var strongHashAlgo = new SHA256Managed();
            var strongHash = strongHashAlgo.ComputeHash(inputStream);
            return strongHash;
        }



        public  FileHash CalculateHashes(byte[] buffer, long offset, int length, Config config)
        {

            var weakHash = CalculateWeakHash(buffer);
            var strongHash = CalculateStrongHash(buffer);

            var fileHash = new FileHash() { Length = length, Offset = offset, WeakHash = weakHash, StrongHash = strongHash };


            return fileHash;
        }

    }
}
