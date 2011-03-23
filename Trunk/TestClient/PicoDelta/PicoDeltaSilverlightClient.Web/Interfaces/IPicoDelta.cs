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
        [OperationContract(IsOneWay = true)]
        void CalculateHashesForFile();

        [OperationContract]
        void UploadCurrentConfig(Config config);

        [OperationContract]
        void GetHashesForFile(Guid sessionId);


        [OperationContract]
        ScanProgress GetProgress(Guid sessionId);

        [OperationContract]
        Config DownloadCurrentConfig();

        [OperationContract]
        void UploadManifest();

    }
}
