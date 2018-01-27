
using System;
using System.IO;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace devWar2018
{
    public static class SaveAndOrder
    {
        [FunctionName("SaveAndOrder")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req, 
            [Table("Orders",Connection = "StorageConnection")]ICollector<PhotoOrder> ordersTable,TraceWriter log)
        {
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                PhotoOrder orderData = JsonConvert.DeserializeObject<PhotoOrder>(requestBody);
                orderData.PartitionKey = System.DateTime.UtcNow.DayOfYear.ToString();
                orderData.RowKey = orderData.FileName;
                ordersTable.Add(orderData);
            }
            catch (System.Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            return (ActionResult)new OkObjectResult($"Order processed");
        }
    }

    public class PhotoOrder : TableEntity
    {
        public string CustomerEmail { get; set; }
        public string FileName { get; set; }
        public int RequiredHeight { get; set; }
        public int RequiredWidth { get; set; }
    }
}
