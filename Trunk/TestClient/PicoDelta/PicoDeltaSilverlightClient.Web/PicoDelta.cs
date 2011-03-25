using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PicoDeltaSilverlightClient.Web.Interfaces;
using PicoDeltaSl;
using System.Runtime.Serialization.Formatters.Binary;

namespace PicoDeltaSilverlightClient.Web
{
    public class PicoDelta : IPicoDelta 
    {


        public Guid CalculateHashesForFile(Config config, Guid sessionId)
        {
            var fileProcessor = new FileProcessor();
            var filePath = @"D:\\Downloads\\en_windows_7_ultimate_rc_x64_dvd_347803.iso";
            var fileId = Guid.NewGuid();
            fileProcessor.GetHashesForFileBockComplete += delegate(Guid currentFileId)
                                                                  {

                                                                      using (var ds = new DataStoreEntities())
                                                                      {
                                                                          var statusEntry =
                                                                              ds.FileStatuses.Where(
                                                                                  x => x.FileId == fileId)
                                                                          _.FirstOrDefault;

                                                                          if(statusEntry != null)
                                                                          {
                                                                              
                                                                          }

                                                                          else
                                                                          {
                                                                              

                                                                          }
                                                                      }
                                                                  };
            using (var ds = new DataStoreEntities())
            {

                var fileSignature = ds.FileSignatures.Where(x => x.FilePath == filePath).FirstOrDefault();



                if (fileSignature != null)
                {
                    return fileSignature.FileId;

                }

                var progressReporter = new ProgressReporter();



               
               
                Task.Factory.StartNew(() =>
                                          {


                                              var fileHashes = fileProcessor.GetHashesForFile(filePath, progressReporter,
                                                                                              config);
                                              using (var ms = new MemoryStream())
                                              {
                                                  var binaryFormatter = new BinaryFormatter();
                                                  binaryFormatter.Serialize(ms, fileHashes);



                                                  ds.FileSignatures.AddObject(new FileSignature()
                                                                                  {
                                                                                      FileId = fileId,
                                                                                      FilePath = filePath,
                                                                                      Signature = ms.ToArray()
                                                                                  });
                                              }
                                              ds.SaveChanges();
                                          });


                return fileId;


            }
        }




        public void UploadCurrentConfig(Config config, Guid sessionId)
        {
          
        }

        public void GetHashesForFile(Guid sessionId)
        { 
          var  fileHashes =
                        (ConcurrentDictionary<long, FileHash>)
                        binaryFormatter.Deserialize(new MemoryStream(fileSignature.Signature));
NotImplementedException();
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