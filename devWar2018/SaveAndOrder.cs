
using System;
using System.IO;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace devWar2018
{
    public static class SaveAndOrder
    {
        [FunctionName("SaveAndOrder")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            PhotoOrder orderData = null;
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                orderData = JsonConvert.DeserializeObject<PhotoOrder>(requestBody);
            }
            catch (Exception )
            {
                return new BadRequestErrorMessageResult("Invalid data");
            }

            return (ActionResult) new OkObjectResult($"Order processed");
        }
    }

    internal class PhotoOrder
    {
        public string CustomerEmail { get; set; }
        public string FileName { get; set; }
        public int RequiredHeight { get; set; }
        public int RequiredWidht { get; set; }
    }
}
