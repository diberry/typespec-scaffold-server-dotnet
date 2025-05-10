using System;
using System.Net;
using System.Threading.Tasks;
using DemoService.Service.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace DemoService.Service
{
    /// <summary>
    /// Implementation of the IWidgets interface that uses Azure Cosmos DB for persistence
    /// </summary>
    public class WidgetsCosmos : IWidgets
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<WidgetsCosmos> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _databaseName = "WidgetDb";
        private readonly string _containerName = "Widgets";

        /// <summary>
        /// Initializes a new instance of the WidgetsCosmos class.
        /// </summary>
        /// <param name="cosmosClient">The Cosmos DB client instance</param>
        /// <param name="logger">Logger for diagnostic information</param>
        /// <param name="httpContextAccessor">Accessor for the HTTP context</param>
        public WidgetsCosmos(
            CosmosClient cosmosClient, 
            ILogger<WidgetsCosmos> logger, 
            IHttpContextAccessor httpContextAccessor)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets a reference to the Cosmos DB container for widgets
        /// </summary>
        private Container WidgetsContainer => _cosmosClient.GetContainer(_databaseName, _containerName);

        /// <summary>
        /// Lists all widgets in the database
        /// </summary>
        /// <returns>Array of Widget objects</returns>
        public async Task<Widget[]> ListAsync()
        {
            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c");
                var widgets = new List<Widget>();

                using var iterator = WidgetsContainer.GetItemQueryIterator<Widget>(queryDefinition);
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    widgets.AddRange(response.ToList());
                }

                return widgets.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing widgets from Cosmos DB");
                throw new Error(500, "Failed to retrieve widgets from database");
            }
        }

        /// <summary>
        /// Retrieves a specific widget by ID
        /// </summary>
        /// <param name="id">The ID of the widget to retrieve</param>
        /// <returns>The retrieved Widget</returns>
        public async Task<Widget> ReadAsync(string id)
        {
            try
            {
                var response = await WidgetsContainer.ReadItemAsync<Widget>(
                    id, new PartitionKey(id));
                
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Widget with ID {WidgetId} not found", id);
                throw new Error(404, $"Widget with ID '{id}' not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading widget {WidgetId} from Cosmos DB", id);
                throw new Error(500, "Failed to retrieve widget from database");
            }
        }

        /// <summary>
        /// Creates a new widget with the specified properties
        /// </summary>
        /// <param name="weight">The weight of the widget</param>
        /// <param name="color">The color of the widget (red or blue)</param>
        /// <returns>The created Widget</returns>
        public async Task<Widget> CreateAsync(int weight, string color)
        {
            if (color != "red" && color != "blue")
            {
                throw new Error(400, "Color must be 'red' or 'blue'");
            }

            try
            {
                var widget = new Widget
                {
                    Id = Guid.NewGuid().ToString(),
                    Weight = weight,
                    Color = color
                };

                var response = await WidgetsContainer.CreateItemAsync(
                    widget, new PartitionKey(widget.Id));
                
                _logger.LogInformation("Created widget with ID {WidgetId}", widget.Id);
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating widget in Cosmos DB");
                throw new Error(500, "Failed to create widget in database");
            }
        }

        /// <summary>
        /// Updates an existing widget with new properties
        /// </summary>
        /// <param name="id">The ID of the widget to update</param>
        /// <param name="weight">The new weight of the widget</param>
        /// <param name="color">The new color of the widget (red or blue)</param>
        /// <returns>The updated Widget</returns>
        public async Task<Widget> UpdateAsync(string id, int weight, string color)
        {
            if (color != "red" && color != "blue")
            {
                throw new Error(400, "Color must be 'red' or 'blue'");
            }

            try
            {
                // First check if the item exists
                Widget existingWidget;
                try
                {
                    var response = await WidgetsContainer.ReadItemAsync<Widget>(
                        id, new PartitionKey(id));
                    existingWidget = response.Resource;
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Widget with ID {WidgetId} not found for update", id);
                    throw new Error(404, $"Widget with ID '{id}' not found");
                }

                // Update the widget properties
                existingWidget.Weight = weight;
                existingWidget.Color = color;

                // Save the updated widget
                var updateResponse = await WidgetsContainer.ReplaceItemAsync(
                    existingWidget, id, new PartitionKey(id));
                
                _logger.LogInformation("Updated widget with ID {WidgetId}", id);
                return updateResponse.Resource;
            }
            catch (Error)
            {
                // Rethrow Error exceptions
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating widget {WidgetId} in Cosmos DB", id);
                throw new Error(500, "Failed to update widget in database");
            }
        }

        /// <summary>
        /// Deletes a widget by its ID
        /// </summary>
        /// <param name="id">The ID of the widget to delete</param>
        public async Task DeleteAsync(string id)
        {
            try
            {
                await WidgetsContainer.DeleteItemAsync<Widget>(id, new PartitionKey(id));
                _logger.LogInformation("Deleted widget with ID {WidgetId}", id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Widget with ID {WidgetId} not found for deletion", id);
                throw new Error(404, $"Widget with ID '{id}' not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting widget {WidgetId} from Cosmos DB", id);
                throw new Error(500, "Failed to delete widget from database");
            }
        }
    }
}