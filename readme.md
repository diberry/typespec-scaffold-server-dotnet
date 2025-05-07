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
    dotnet add package Microsoft.Azure.Cosmos --version 3.*
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

## Add managed identity 

```
dotnet add package Azure.Identity --version 1.12.*
```

login to azure cli

```
az login
```

* `dotnet user-secrets init`
* `dotnet user-secrets set "CosmosDb:Endpoint" "<your-cosmos-db-endpoint-here>"`

required RBAC permissions

https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-grant-control-plane-access?tabs=built-in-definition%2Ccsharp&pivots=azure-interface-cli

```
# User
az ad signed-in-user show

#Control plan
az role definition list \
    --name "Cosmos DB Operator"

az group show \
    --name "<name-of-existing-resource-group>"

#Data plan
az cosmosdb sql role definition list \
    --resource-group "<name-of-existing-resource-group>" \
    --account-name "<name-of-existing-nosql-account>"

az cosmosdb show \
    --resource-group "<name-of-existing-resource-group>" \
    --name "<name-of-existing-nosql-account>" \
    --query "{id:id}"

az cosmosdb sql role assignment create --resource-group "<name-of-existing-resource-group>" --account-name "<name-of-existing-nosql-account>" --role-definition-id "<id-of-new-role-definition>" --principal-id "<id-of-existing-identity>" --scope "/subscriptions/aaaa0a0a-bb1b-cc2c-dd3d-eeeeee4e4e4e/resourceGroups/msdocs-identity-example/providers/Microsoft.DocumentDB/databaseAccounts/msdocs-identity-example-nosql"

az cosmosdb sql role assignment list \
    --resource-group "<name-of-existing-resource-group>" \
    --account-name "<name-of-existing-nosql-account>"
```

## Related repos

https://github.com/azure-samples/container-apps-dotnet-minimal-api