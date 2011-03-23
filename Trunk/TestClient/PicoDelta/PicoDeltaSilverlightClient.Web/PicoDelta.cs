using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PicoDeltaSilverlightClient.Web.Interfaces;
using PicoDeltaSl;

namespace PicoDeltaSilverlightClient.Web
{
    public class PicoDelta : IPicoDelta 
    {
        public ConcurrentDictionary<long, FileHash> GetHashesForFile()
        {
            throw new NotImplementedException();
        }

        public Config DownloadCurrentConfig()
        {
            return new Config();
        }

        public void UploadManifest()
        {
            throw new NotImplementedException();
        }
    }
}