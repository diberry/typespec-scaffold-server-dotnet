Steps:

1. Meet all prereqs (TSP, Node.js, .NET 9)
1. Create new TSP project in /tsp
    * main.tsp
    * tspconfig.yaml
1. Create package.json, install TSP packages, and add compile script
1. Run compile script to get ./server and ./spec/openapi3/
1. Change into generated ./server folder and run dotnet restore and dotnet run.
1. Install Azure Cosmos client library

    ```
    dotnet add package Microsoft.Azure.Cosmos
    ```

1. Add key to local secrets

    * `dotnet user-secrets init`
    * `dotnet user-secrets set "CosmosDb:Endpoint" "<your-cosmos-db-endpoint-here>"`
    * `dotnet user-secrets set "CosmosDb:Key" "<your-cosmos-db-key-here>"`
    * `dotnet user-secrets list`

1. Update appsettings.json

    ```json
    {
    "CosmosDb": {
        "DatabaseName": "WidgetDb",
        "DatabaseContainer": "Widgets"
    },
    "Logging": {
        "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*"
    }
    ```

1. Build succeeds so run the application with dotent run. http://127.0.0.1:5471/swagger/index.html
1. Create AZD infra with azd init
1. Change container to max replicas of 1
1. Deploy with azd up
