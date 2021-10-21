using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VoorbeeldExamenDaan.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using System.Text;
using System.Diagnostics;
using Microsoft.Azure.Cosmos.Table;

namespace VoorbeeldExamenDaan
{
    public class Function1
    {
        [FunctionName("GetPrices")]
        public static async Task<IActionResult> GetPrices(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/prices")] HttpRequest req,
            ILogger log)
        {

            try
            {
                List<Price> prices = new List<Price>();
                string connectionstring = Environment.GetEnvironmentVariable("SQLSERVER");


                using (SqlConnection sqlConnection = new SqlConnection(connectionstring))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "SELECT * FROM Prices";

                        var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
                        while (await sqlDataReader.ReadAsync())
                        {
                            prices.Add(new Price()
                            {
                                Type = sqlDataReader["Type"].ToString(),
                                ItemPrice = double.Parse(sqlDataReader["Price"].ToString())
                            });
                        }
                    }
                }

                return new OkObjectResult(prices);
            }
            catch (Exception ex)
            {

                log.LogError(ex.ToString());
                return new StatusCodeResult(500);
                throw;
            }
        }


        [FunctionName("GetRegistrations")]
        public static async Task<IActionResult> GetRegistrations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/registrations/{email}/{sensor}")] HttpRequest req,
            string email, string sensor, ILogger log)
        {

            try
            {
                string connectionstring = Environment.GetEnvironmentVariable("TABLESTORAGE");
                CloudStorageAccount cloudstorageaccount = CloudStorageAccount.Parse(connectionstring);
                CloudTableClient cloudtableclient = cloudstorageaccount.CreateCloudTableClient();
                CloudTable table = cloudtableclient.GetTableReference("registraties");

                TableQuery<RegistrationEntity> rangeQuery = new TableQuery<RegistrationEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email.ToLower()));

                var queryResult = await table.ExecuteQuerySegmentedAsync<RegistrationEntity>(rangeQuery, null);
                List<Registration> registrations = new List<Registration>();

                foreach (var reg in queryResult.Results)
                {
                    if (reg.Sensor == sensor.ToLower())
                    {
                        registrations.Add(new Registration()
                        {
                            Sensor = reg.Sensor.ToString().ToLower(),
                            Amount = int.Parse(reg.Amount.ToString()),
                            Price = double.Parse(reg.Price.ToString()),
                            EMail = email.ToLower()
                        });
                    }
                    
                }

                return new OkObjectResult(registrations);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return new StatusCodeResult(500);
                throw;
            }
        }

        [FunctionName("MQTTReciever")]
        public static void MQTTReciever(
        [MqttTrigger("/daandewilde")] IMqttMessage message,
        ILogger logger)
        {
            var body = message.GetMessage();
            var bodyString = Encoding.UTF8.GetString(body);
            Registration reg = JsonConvert.DeserializeObject<Registration>(bodyString);
            Debug.WriteLine(reg.Sensor);
            SendToTable(reg);

        }

        public static async Task SendToTable(Registration reg)
         {
              try
             {
                 string connectionstring = Environment.GetEnvironmentVariable("TABLESTORAGE");
                 CloudStorageAccount cloudstorageaccount = CloudStorageAccount.Parse(connectionstring);
                 CloudTableClient cloudtableclient = cloudstorageaccount.CreateCloudTableClient();
                 CloudTable table = cloudtableclient.GetTableReference("registraties");
                 await table.CreateIfNotExistsAsync();
                 string guid = Guid.NewGuid().ToString();

                 RegistrationEntity registrationentity = new RegistrationEntity(reg.EMail.ToString().ToLower(), guid)
                 {
                     Amount = int.Parse(reg.Amount.ToString()),
                     Price = double.Parse(reg.Price.ToString()),
                     Sensor = reg.Sensor.ToString().ToLower()
                 };

                 TableOperation insertoperation = TableOperation.Insert(registrationentity);
                 await table.ExecuteAsync(insertoperation);


             }
             catch (Exception ex)
             {
                 Debug.WriteLine(ex.ToString());
                 throw;
             }
         }
    }
}
