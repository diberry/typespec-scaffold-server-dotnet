using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TypeSpec.Helpers
{
    /// <summary>
    /// Hosted service that initializes Cosmos DB resources on application startup
    /// </summary>
    public class CosmosDbInitializer : IHostedService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<CosmosDbInitializer> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _databaseName;
        private readonly string _containerName = "Widgets";

        public CosmosDbInitializer(CosmosClient cosmosClient, ILogger<CosmosDbInitializer> logger, IConfiguration configuration)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
            _configuration = configuration;
            _databaseName = _configuration["CosmosDb:DatabaseName"] ?? "WidgetDb";
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Ensuring Cosmos DB database and container exist...");

            try
            {
                // Create database if it doesn't exist
                var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(
                    _databaseName,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Database {DatabaseName} status: {Status}", _databaseName, 
                    databaseResponse.StatusCode == System.Net.HttpStatusCode.Created ? "Created" : "Already exists");

                // Create container if it doesn't exist (using id as partition key)
                var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(
                    new ContainerProperties
                    {
                        Id = _containerName,
                        PartitionKeyPath = "/id"
                    },
                    throughput: 400, // Minimum RU/s
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Container {ContainerName} status: {Status}", _containerName,
                    containerResponse.StatusCode == System.Net.HttpStatusCode.Created ? "Created" : "Already exists");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Cosmos DB");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}