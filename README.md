---
page_type: sample
languages:
- java
products:
- azure
services: App-Service
platforms: dotnet
author: yaohaizh
---

# Getting started on scaling Web Apps in C# #

      Azure App Service sample for managing web apps.
       - Create a domain
       - Create a self-signed certificate for the domain
       - Create 3 app service plans in 3 different regions
       - Create 5 web apps under the 3 plans, bound to the domain and the certificate
       - Create a traffic manager in front of the web apps
       - Scale up the app service plans to twice the capacity


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/app-service-dotnet-scale-web-apps.git

    cd app-service-dotnet-scale-web-apps

    dotnet build

    bin\Debug\net452\ManageWebAppWithTrafficManager.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.