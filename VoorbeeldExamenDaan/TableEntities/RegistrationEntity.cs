using Microsoft.Azure.Cosmos.Table;

namespace VoorbeeldExamenDaan
{
    public class RegistrationEntity : TableEntity
    {

        public RegistrationEntity()
        {
           
        }

        public RegistrationEntity(string EMail, string guid)
        {
            this.PartitionKey = EMail;
            this.RowKey = guid;
        }

        public int Amount { get; set; }
        public double Price { get; set; }
        public string Sensor { get; set; }
    }
}