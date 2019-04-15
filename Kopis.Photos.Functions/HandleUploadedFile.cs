using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Kopis.Photos.Functions
{
    public static class HandleUploadedFile
    {
        [FunctionName("HandleUploadedFile")]
        public static void Run([BlobTrigger("uploaded/{name}", Connection = "UploadStorage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
