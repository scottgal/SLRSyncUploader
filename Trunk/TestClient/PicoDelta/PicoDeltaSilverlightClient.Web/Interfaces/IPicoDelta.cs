using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using PicoDeltaSl;

namespace PicoDeltaSilverlightClient.Web.Interfaces
{
    [ServiceContract]
    public interface IPicoDelta
    {
        [OperationContract]
        ConcurrentDictionary<long, FileHash> GetHashesForFile();

        [OperationContract]
        Config DownloadCurrentConfig();

        [OperationContract]
        void UploadManifest();

    }
}
