Steps:

1. Meet all prereqs (TSP, Node.js, .NET 9)
1. Create new TSP project in /tsp
    * main.tsp
    * tspconfig.yaml
1. Create package.json, install TSP packages, and add compile script
1. Run compile script to get ./server and ./spec/openapi3/
1. Change into generated ./server folder and run dotnet restore and dotnet run.
1. Build succeeds so run the application with dotent run. http://127.0.0.1:5471/swagger/index.html
1. Create AZD infra with azd init
1. Change container to max replicas of 1
1. Deploy with azd up
