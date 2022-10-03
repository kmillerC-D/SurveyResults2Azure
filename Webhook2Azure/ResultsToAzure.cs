using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Linq;
using Azure.Storage.Blobs;

namespace SurveyResultsToAzure
{
    public static class ResultsToAzure
    {
        [FunctionName("ResultsToAzure")]
        public static async Task<IActionResult> Run(


        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Blob("connexp", Connection = "AzureWebJobsStorage")] CloudBlobContainer outputContainer, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogDebug($"Request body: {requestBody}");

            // Process form data in the body.
            var formValues = requestBody.Split('&')
                .Select(value => value.Split('='))
                .ToDictionary(pair => Uri.UnescapeDataString(pair[0]),
                    pair => Uri.UnescapeDataString(pair[1]));
            var jsonObj = JsonConvert.SerializeObject(formValues, Formatting.Indented);
            log.LogDebug($"Request parsed to JSON: {JsonConvert.SerializeObject(jsonObj)}");

            // Now persist in Azure Storage
            // This can be omitted
            await outputContainer.CreateIfNotExistsAsync();
            // Create a blob name like: 2020-12-18 20-45-12 59A0962E-2AE9-498C-8B50-FE3F5935B556.json
            var blobName = $"{DateTime.UtcNow:yyyy-MM-dd HH-mm-ss} {Guid.NewGuid()}.json";
            var cloudBlockBlob = outputContainer.GetBlockBlobReference(blobName);
            await cloudBlockBlob.UploadTextAsync(jsonObj);

            return new OkResult();
        }
    }
}

/*
 * 
            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");
            Stream myBlob = new MemoryStream();
            var file = req.Form.Files["File"];
            myBlob = file.OpenReadStream();
            var blobClient = new BlobContainerClient(Connection, containerName);
            var blob = blobClient.GetBlobClient(file.FileName);
            await blob.UploadAsync(myBlob);
            return new OkObjectResult("file uploaded successfylly");
*/
