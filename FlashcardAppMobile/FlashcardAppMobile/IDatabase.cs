using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace FlashcardAppMobile
{
    public interface IDatabase
    {
        Task InitialiseAsync();
        Task DatabaseSetup();
        Task CreateDatabaseAsync();
        Task CreateContainerAsync();
        Task AddItemToContainerAsync(UserFlashcardInfo userFlashcardInfo);
        Task ReplaceDatabaseItemAsync(UserFlashcardInfo userFlashcardInfo);
    }

    public class DatabaseInfo : IDatabase
    {
        private static readonly string EndpointUri = "https://flashcardappdatabase.documents.azure.com:443/";
        private static readonly string PrimaryKey = "eB4uCXLiIgwelA2u82MYxvy6ZLRLy4nuCpXGuwtB5u1uk7Q8NELrSdVzAY52sc4eabSuSHYERbPDhDXqhzd09g==";

        private CosmosClient cosmosClient;
        private Database database;
        private Container container;

        private string databaseId = "FlashcardAppDatabase";
        private string containerId = "FlashcardAppContainer";

        public delegate void DBInitialisationFinished(bool succeeded);
        public event DBInitialisationFinished OnDatabaseInitialisationFinished;

        public delegate void DBQueryItems(List<UserFlashcardInfo> userFlashcardInfos);
        public event DBQueryItems OnDatabaseQueryItems;

        public delegate void DBAddToContainerFinished();
        public event DBAddToContainerFinished OnDatabaseAddToContainerFinished;

        public async Task InitialiseAsync()
        {
            try
            {
                Console.WriteLine("Beginning operations...\n");
                await DatabaseSetup();
            }
            catch (CosmosException cosmosException)
            {
                Console.WriteLine("Cosmos Exception with Status {0} : {1}\n", cosmosException.StatusCode, cosmosException);
                OnDatabaseInitialisationFinished?.Invoke(false);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
                OnDatabaseInitialisationFinished?.Invoke(false);
            }
            finally
            {
                Console.WriteLine("Database successfully initialised.");
                OnDatabaseInitialisationFinished?.Invoke(true);
            }
        }

        public async Task DatabaseSetup()
        {
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
        }

        public async Task CreateDatabaseAsync()
        {
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(this.databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }

        public async Task CreateContainerAsync()
        {
            this.container = await this.database.CreateContainerIfNotExistsAsync(this.containerId, "/id");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        public async Task AddItemToContainerAsync(UserFlashcardInfo userFlashcardInfo)
        {
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<UserFlashcardInfo> userFlashcardInfoResponse = await this.container.ReadItemAsync<UserFlashcardInfo>(userFlashcardInfo.Id, new PartitionKey(userFlashcardInfo.UserId));
                Console.WriteLine("Item in database with id: {0} already exists\n", userFlashcardInfoResponse.Resource.Id);
                await ReplaceDatabaseItemAsync(userFlashcardInfo);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the users flashcard sets. Note we provide the value of the partition key for this item, which is "toast"
                ItemResponse<UserFlashcardInfo> userFlashcardInfoResponse = await this.container.CreateItemAsync<UserFlashcardInfo>(userFlashcardInfo, new PartitionKey(userFlashcardInfo.UserId));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", userFlashcardInfoResponse.Resource.Id, userFlashcardInfoResponse.RequestCharge);
                OnDatabaseAddToContainerFinished?.Invoke();
            }
        }

        public async Task ReplaceDatabaseItemAsync(UserFlashcardInfo userFlashcardInfo)
        {
            // replace the item with the updated content
            ItemResponse<UserFlashcardInfo> userFlashcardInfoResponse = await container.ReplaceItemAsync(userFlashcardInfo, userFlashcardInfo.Id, new PartitionKey(userFlashcardInfo.UserId));
            Console.WriteLine("Updated Item [{0},{1}].\n \tBody is now: {2}\n \tRequests charged: {3}", userFlashcardInfo.UserId, userFlashcardInfo.Id, userFlashcardInfo, userFlashcardInfoResponse.RequestCharge);
            OnDatabaseAddToContainerFinished?.Invoke();
        }

        public async Task QueryItemsAsync(string userId)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.UserId = '{userId}'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using FeedIterator<UserFlashcardInfo> queryResultSetIterator = this.container.GetItemQueryIterator<UserFlashcardInfo>(queryDefinition);

            List<UserFlashcardInfo> userFlashcardInfos = new List<UserFlashcardInfo>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<UserFlashcardInfo> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (UserFlashcardInfo userFlashcardInfo in currentResultSet)
                {
                    userFlashcardInfos.Add(userFlashcardInfo);
                    Console.WriteLine("\tRead {0}\n", userFlashcardInfo);
                }
            }

            OnDatabaseQueryItems?.Invoke(userFlashcardInfos);
        }
    }
}
