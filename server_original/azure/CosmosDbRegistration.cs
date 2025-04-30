using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Azure.Identity;

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
                // Validate configuration
                if (string.IsNullOrEmpty(cosmosEndpoint))
                {
                    throw new ArgumentException("Cosmos DB Endpoint must be specified in configuration");
                }
                
                // Create chained credential with fallback options
                var credential = new ChainedTokenCredential(
                    new ManagedIdentityCredential(),
                    new AzureCliCredential()
                );
                
                // Create Cosmos client with token credential authentication
                return new CosmosClient(cosmosEndpoint, credential, cosmosClientOptions);
            });

            // Initialize Cosmos DB if needed
            builder.Services.AddHostedService<TypeSpec.Helpers.CosmosDbInitializer>();
            
            // Register WidgetsCosmos implementation of IWidgets
            builder.Services.AddScoped<IWidgets, WidgetsCosmos>();
        }
    }
}