---
page_type: sample
languages:
- csharp
products:
- azure
extensions:
  services: App-Service
  platforms: dotnet
---

# Getting started on managing Functions with custom domains using C# #

 Azure App Service sample for managing function apps.
  - app service plan, function app
    - Create 2 function apps under the same new app service plan
  - domain
    - Create a domain
  - certificate
    - Upload a self-signed wildcard certificate
    - update both function apps to use the domain and the created wildcard SSL certificate


## Running this Sample ##

To run this sample:

Set the environment variable `CLIENT_ID`,`CLIENT_SECRET`,`TENANT_ID`,`SUBSCRIPTION_ID` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/app-service-dotnet-manage-functions-with-custom-domains.git

    cd app-service-dotnet-manage-functions-with-custom-domains

    dotnet build

    bin\Debug\net452\ManageFunctionAppWithDomainSsl.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.