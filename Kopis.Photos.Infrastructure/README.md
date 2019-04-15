You can deploy this project to your Azure tenant by clicking on this button and configuring the parameters:
[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://azuredeploy.net/)

Parameters
* WebsiteSku: Describes plan's pricing tier and capacity. Check details at https://azure.microsoft.com/en-us/pricing/details/app-service/ (defaults to the free F1 level)
* WebsiteCapacity: Describes plan's instance count (defaults to 1)
* Environment: Name of the environment that the resource group creates (e.g. Dev, Test, Prod). Useful if you want to run multiple versions of this app.
* StorageType: Describes the storage tier for the account that will host uploads (defaults to Standard Locally Redundant Storage)
* CognitiveServicesSku: Describes the pricing tier for Cognitive Services. Defaults to the free F0 level, but note that you may only have one F0 instance per subscription.
* CognitiveServicesLocation: The datacenter location that the Cognitive Services API will be hosted at
* SearchSku: Describes the SKU of the search service you want to create. E.g. free or standard
* SearchReplicaCount: Replicas distribute search workloads across the service. You need 2 or more to support high availability (applies to Basic and Standard only).
* SearchPartitionCount: Partitions allow for scaling of document count as well as faster indexing by sharding your index over multiple Azure Search units.
* SearchHostingMode: Applicable only for SKU set to standard3. You can set this property to enable a single, high density partition that allows up to 1000 indexes, which is much higher than the maximum indexes allowed for any other SKU.

In general, most of these default to the free or lowest-cost version. The only required item is the "Environment" one.
