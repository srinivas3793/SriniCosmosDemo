using System;
using Microsoft.Azure.Cosmos;

namespace CosmosDemo
{
    class Program
    {
        private static string connectionString = "AccountEndpoint=https://srinicoresql.documents.azure.com:443/;AccountKey=BAWUpHn9gEknYmCw7L0iXDaOFwsXYh9OcrAOChSTKeaRwHJnihZqDVczfjzjv2R4iLPAvnoSfFZgIPP0rE1TRA==;";
        private static CosmosClient client;

        static void Main(string[] args)
        {
            CosmosClientOptions options = new CosmosClientOptions
            {
                ConsistencyLevel = ConsistencyLevel.Eventual
            };


            client = new CosmosClient(connectionString, options);

            DatabaseResponse dr = client.CreateDatabaseIfNotExistsAsync("TestDatabase", 500).GetAwaiter().GetResult();

            UniqueKey key1 = new UniqueKey();
            key1.Paths.Add("/lastname");

            UniqueKeyPolicy policy = new UniqueKeyPolicy();
            policy.UniqueKeys.Add(key1);

            ContainerProperties cp = new ContainerProperties
            {
                Id = "testContainer",
                PartitionKeyPath = "/email",
                UniqueKeyPolicy = policy,
            };

            cp.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath() { Path = "/*" });
            cp.IndexingPolicy.IncludedPaths.Add(new IncludedPath() { Path = "/firstname/*" });
            //Will provision 600 to new container if not then uses shared or 400 as specified at db level
            ContainerResponse cr = dr.Database.CreateContainerIfNotExistsAsync(cp, 600).GetAwaiter().GetResult();
            Employee e1 = new Employee
            {
                age = 11,
                email = "srinivas379@gmail.com",
                firstName = "microsoft",
                lastName = "Da",
                id = "002"
            };

            ItemResponse<Employee> item = cr.Container.CreateItemAsync(e1, new PartitionKey(e1.email)).GetAwaiter().GetResult();
            Console.WriteLine(item.Resource.email.ToString());
            Console.WriteLine(item.RequestCharge);

            //Querying
            QueryRequestOptions ro = new QueryRequestOptions
            {
                MaxItemCount = 1 //Pagination settings
            };

            FeedIterator<Employee> fi = cr.Container.GetItemQueryIterator<Employee>("select * from c", null, ro);
            
            //By default Query fetches only 100 records if it has more than 100 then it fetches next hundred with other set of query execution
            //Pagination
            while(fi.HasMoreResults)
            {
                FeedResponse<Employee> fr = fi.ReadNextAsync().GetAwaiter().GetResult();

                foreach(var efr in fr)
                {
                    Console.WriteLine($"Record : {efr.email} --- {efr.firstName}");
                }
            }
        }
    }

    public class Employee
    {
        public int age { get; set; }
        public string email { get; set; }
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }
}
