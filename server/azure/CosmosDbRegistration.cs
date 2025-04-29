using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using System;
using System.Threading.Tasks;

namespace DemoService.Service
{
    /// <summary>
    /// Registration class for Azure Cosmos DB services and implementations
    /// </summary>
    public static class CosmosDbRegistration
    {
        /// <summary>
        /// Registers the Cosmos DB client and related services for dependency injection
        /// </summary>
        /// <param name="builder">The web application builder</param>
        public static void RegisterCosmosServices(this WebApplicationBuilder builder)
        {
            // Register the HttpContextAccessor for accessing the HTTP context
            builder.Services.AddHttpContextAccessor();
            
            // Register JSON serialization provider
            builder.Services.AddScoped<TypeSpec.Helpers.IJsonSerializationProvider, TypeSpec.Helpers.JsonSerializationProvider>();
            
            // Get configuration settings
            var cosmosEndpoint = builder.Configuration["CosmosDb:Endpoint"];
            var cosmosDatabaseName = builder.Configuration["CosmosDb:DatabaseName"] ?? "WidgetDb";
            
            // Configure Cosmos DB client options
            var cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                ConnectionMode = ConnectionMode.Direct
            };
            
            builder.Services.AddSingleton(serviceProvider => 
            {
                // For local development, use the Cosmos DB emulator if no endpoint is specified
                if (string.IsNullOrEmpty(cosmosEndpoint) && builder.Environment.IsDevelopment())
                {
                    // Local emulator endpoint and key
                    var emulatorEndpoint = "https://localhost:8081/";
                    var emulatorKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
                    return new CosmosClient(emulatorEndpoint, emulatorKey, cosmosClientOptions);
                }
                
                if (string.IsNullOrEmpty(cosmosEndpoint))
                {
                    throw new ArgumentException("Cosmos DB Endpoint must be specified in configuration");
                }
                
                // Use DefaultAzureCredential for authentication in production
                return new CosmosClient(cosmosEndpoint, new DefaultAzureCredential(), cosmosClientOptions);
            });

            // Initialize Cosmos DB if needed
            builder.Services.AddHostedService<TypeSpec.Helpers.CosmosDbInitializer>();
            
            // Register WidgetsCosmos implementation of IWidgets
            builder.Services.AddScoped<IWidgets, WidgetsCosmos>();
        }
    }
}