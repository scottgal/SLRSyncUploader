using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace HashEngine
{
    public class FileProcessor
    {


        public static ConcurrentBag<FileChunk> GetNonMatchingBlocksForFile(ConcurrentDictionary<long, FileHash> remoteHashes, string filePath, Config config)
        {
          
            var fileInfo = new FileInfo(filePath);
            var fileChunkSize =Convert.ToInt64(fileInfo.Length/config.DegreeOfParalleism);
            var taskArray = new List<Task>(config.DegreeOfParalleism);

            long chunkStart =0;
            long chunkEnd =0;
            
            var resultBag = new ConcurrentBag<FileChunk>();
            for (var i = 0; i <= config.DegreeOfParalleism; i++)
            {
                chunkEnd = chunkEnd + fileChunkSize;
                var chunkOverlap = chunkEnd + config.BlockLength;


                if (taskArray.Count <= config.DegreeOfParalleism)
                {
                    taskArray.Add(Task.Factory.StartNew(
                        () => ScanFileChunk(remoteHashes, filePath, chunkStart, chunkOverlap, config))
                                      .ContinueWith(
                                          outHash => outHash.Result.ForEach(resultBag.Add)));

                }
                else
                {
                    var complete = Task.WaitAny(taskArray.ToArray());
                    taskArray.RemoveAt(complete);
                }

                chunkStart = chunkEnd + 1;
            }
            return null;
        }

        
        public static List<FileChunk> ScanFileChunk(ConcurrentDictionary<long, FileHash> remoteHashes, string filePath, long startOffset, long endOffset, Config config)
        {
            var windowBuffer = new byte[config.BlockLength];
            var windowChecksum = new Adler32();

            var chunkLength = endOffset - startOffset;
            var localBlockLength = config.BlockLength;
            long currentFilePosition = 0;
            var procCount = config.DegreeOfParalleism * 15;
            var taskArray = new List<Task>(procCount);
            var bytesToRead = config.BlockLength;
            int complete = 0;
            var buffer = new byte[localBlockLength];
           using (var file = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.Read))
           {
               file.Seek(startOffset, SeekOrigin.Begin); 
               while (currentFilePosition < chunkLength) 
                {
                    if (currentFilePosition + localBlockLength > chunkLength)
                    {
                        bytesToRead = checked((int) (chunkLength - currentFilePosition));
                        buffer = new byte[bytesToRead];
                    }
                    var offSetPosition = currentFilePosition;

                    if (file.Read(buffer, 0, bytesToRead) < 0)
                    {
                        break;
                    }
                }

                windowChecksum.Update(windowBuffer);
            }

            return null;
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
            using (var file = new FileStream(filePath,FileMode.Open,FileAccess.Read, FileShare.Read))
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



        private static Adler32 adler32 = new Adler32();


        public static long CalculateWeakHash(byte[] buffer)
        {

            adler32.Reset();
            adler32.Update(buffer);
            var weakHash = adler32.Value;
            return weakHash;
        }



        public static byte[]  CalculateStrongHash(byte[] buffer)
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
