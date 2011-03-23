using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using PicoDeltaSilverlightClient.Web.Interfaces;
using PicoDeltaSl;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PicoDeltaSilverlightClient.Web
{
    public class PicoDelta : IPicoDelta 
    {

     
        public void GetHashesForFile()
        {
            var fileProcessor = new FileProcessor();
            var config = new Config();
            var filePath = @"D:\\Downloads\\en_windows_7_ultimate_rc_x64_dvd_347803.iso";

            using (var ds = new DataStoreEntities())
            {

                var fileSignature = ds.FileSignatures.Where(x => x.FilePath == filePath).FirstOrDefault();
                ConcurrentDictionary<long, FileHash> fileHashes;

                var binaryFormatter = new BinaryFormatter();
                if (fileSignature != null)
                {


                    fileHashes =
                        (ConcurrentDictionary<long, FileHash>)
                        binaryFormatter.Deserialize(new MemoryStream(fileSignature.Signature));


                    //return fileHashes;
                }

                fileHashes = fileProcessor.GetHashesForFile(filePath, config);
                var ms = new MemoryStream();
                binaryFormatter.Serialize(ms, fileHashes);

                ds.FileSignatures.AddObject(new FileSignature()
                                                {FileId = Guid.NewGuid(), FilePath = filePath, Signature = ms.ToArray()});
                ds.SaveChanges();
                //return fileHashes;
            }
            
            
          
            
            
        }




        public void UploadCurrentConfig(Config config, Guid sessionId)
        {
            throw new NotImplementedException();
        }

        public void GetHashesForFile(Guid sessionId)
        {
            throw new NotImplementedException();
        }

        public ScanProgress GetProgress(Guid sessionId)
        {
            
        }

        public Config DownloadCurrentConfig()
        {
            return new Config();
        }

        public void UploadManifest(Manifest manifest)
        {
            throw new NotImplementedException();
        }
    }
}                                                                                       