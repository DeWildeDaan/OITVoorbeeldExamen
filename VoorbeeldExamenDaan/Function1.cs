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

namespace VoorbeeldExamenDaan
{
    public static class Function1
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

        
    }
}
