# MongoDB.Driver.Core.Extensions.DiagnosticSources

## Usage
When instancing a MongoClient,as shown below 
```csharp
var clientSettings = MongoClientSettings.FromUrl(mongoUrl);
clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
var mongoClient = new MongoClient(clientSettings);
``` 
This package reference  https://github.com/jbogard/MongoDB.Driver.Core.Extensions.DiagnosticSources
 
