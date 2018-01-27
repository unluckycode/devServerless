
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace devWar2018
{
    public static class ResizePicture
    {
        [FunctionName("ResizePicture")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req,
                                        [Blob("photos", FileAccess.Read,Connection = "StorageConnection")]CloudBlobContainer photosContainer,
                                        [Blob("doneorders/{rand-guid}",FileAccess.ReadWrite,Connection = "StorageConnection")]ICloudBlob resizedPhotoCloudBlob,
                                        TraceWriter log)
        {
            var pictureResizeRequest = GetResizeRequest(req);
            var photoStrean = await GetSourcePhotoStream(photosContainer, pictureResizeRequest.FileName);
            SetAttachmentAsContentDisposition(resizedPhotoCloudBlob, pictureResizeRequest);

            var image = Image.Load(photoStrean);
            image.Mutate(e=>e.Resize(pictureResizeRequest.RequiredWidth,pictureResizeRequest.RequiredHeight));

            var resizedPhotoStream = new MemoryStream();
            image.Save(resizedPhotoStream,new JpegEncoder());
            resizedPhotoStream.Seek(0, SeekOrigin.Begin);

            await resizedPhotoCloudBlob.UploadFromStreamAsync(resizedPhotoStream);
            
            return new JsonResult(new { FileName= resizedPhotoCloudBlob.Name});
        }

        private static async Task<Stream> GetSourcePhotoStream(CloudBlobContainer photosContainer, string fileName)
        {
            var photoBlob = await photosContainer.GetBlobReferenceFromServerAsync(fileName);
            var photoStream = await photoBlob.OpenReadAsync(AccessCondition.GenerateEmptyCondition(),
                new BlobRequestOptions(), new OperationContext());
            return photoStream;
        }

        private static void SetAttachmentAsContentDisposition(ICloudBlob resizedPhotoCloudBlob, PictureResizeRequest pictureResizeRequest)
        {
            resizedPhotoCloudBlob.Properties.ContentDisposition =
                $"attachment; filename={pictureResizeRequest.RequiredWidth}x{pictureResizeRequest.RequiredHeight}.jpeg";
        }

        private static PictureResizeRequest GetResizeRequest(HttpRequest req)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            PictureResizeRequest pictureResizeRequest =
                JsonConvert.DeserializeObject<PictureResizeRequest>(requestBody);
            return pictureResizeRequest;
        }
    }

    public class PictureResizeRequest
    {
        public int RequiredWidth { get; set; }
        public int RequiredHeight { get; set; }
        public string FileName { get; set; }
    }
}
