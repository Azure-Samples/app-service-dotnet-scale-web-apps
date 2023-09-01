// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;
using Azure.ResourceManager.TrafficManager;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ManageWebAppWithTrafficManager
{
    /**
     * Azure App Service sample for managing web apps.
     *  - Create a domain
     *  - Create a self-signed certificate for the domain
     *  - Create 3 app service plans in 3 different regions
     *  - Create 5 web apps under the 3 plans, bound to the domain and the certificate
     *  - Create a traffic manager in front of the web apps
     *  - Scale up the app service plans to twice the capacity
     */

    public class Program
    {
        private static string CERT_PASSWORD = Utilities.CreatePassword();
        private static string pfxPath;

        public static async Task RunSample(ArmClient client)
        {
            AzureLocation region = AzureLocation.EastUS;
            string resourceGroupName = Utilities.CreateRandomName("rgNEMV_");
            string app1Name = Utilities.CreateRandomName("webapp1-");
            string app2Name = Utilities.CreateRandomName("webapp2-");
            string app3Name = Utilities.CreateRandomName("webapp3-");
            string app4Name = Utilities.CreateRandomName("webapp4-");
            string app5Name = Utilities.CreateRandomName("webapp5-");
            string plan1Name = Utilities.CreateRandomName("jplan1_");
            string plan2Name = Utilities.CreateRandomName("jplan2_");
            string plan3Name = Utilities.CreateRandomName("jplan3_");
            string domainName = Utilities.CreateRandomName("jsdkdemo-") + ".com";
            string trafficManagerName = Utilities.CreateRandomName("jsdktm-");
            var lro = await client.GetDefaultSubscription().GetResourceGroups().CreateOrUpdateAsync(Azure.WaitUntil.Completed, resourceGroupName, new ResourceGroupData(AzureLocation.EastUS));
            ResourceGroupResource resourceGroup = lro.Value;

            try
            {
                //============================================================
                // Purchase a domain (will be canceled for a full refund)

                Utilities.Log("Purchasing a domain " + domainName + "...");

                var domainCollection = resourceGroup.GetAppServiceDomains();
                var domainData = new AppServiceDomainData(region)
                {
                    ContactRegistrant = new RegistrationContactInfo("jondoe@contoso.com", "Jon", "Doe", "4258828080")
                    {
                        AddressMailing = new RegistrationAddressInfo("123 4th Ave", "Redmond", "UnitedStates", "98052", "WA")
                    },
                    IsDomainPrivacyEnabled = true,
                    IsAutoRenew = false
                };
                var domain_lro = domainCollection.CreateOrUpdate(WaitUntil.Completed, domainName, domainData);
                var domain = domain_lro.Value;
                Utilities.Log("Purchased domain " + domain.Data.Name);
                Utilities.Print(domain);

                //============================================================
                // Create a self-singed SSL certificate

                pfxPath = "webapp_" + nameof(ManageWebAppWithTrafficManager).ToLower() + ".pfx";

                Utilities.Log("Creating a self-signed certificate " + pfxPath + "...");

                Utilities.CreateCertificate(domainName, pfxPath, CERT_PASSWORD);

                //============================================================
                // Create 3 app service plans in 3 regions

                Utilities.Log("Creating app service plan " + plan1Name + " in US West...");

                var plan1 =await CreateAppServicePlanAsync(resourceGroup, plan1Name, region);

                Utilities.Log("Created app service plan " + plan1.Data.Name);
                Utilities.Print(plan1);

                Utilities.Log("Creating app service plan " + plan2Name + " in Europe West...");

                var plan2 = await CreateAppServicePlanAsync(resourceGroup, plan2Name, region);

                Utilities.Log("Created app service plan " + plan2.Data.Name);
                Utilities.Print(plan1);

                Utilities.Log("Creating app service plan " + plan3Name + " in Asia East...");

                var plan3 = await CreateAppServicePlanAsync(resourceGroup, plan3Name, region);

                Utilities.Log("Created app service plan " + plan2.Data.Name);
                Utilities.Print(plan1);

                //============================================================
                // Create 5 web apps under these 3 app service plans

                Utilities.Log("Creating web app " + app1Name + "...");

                WebSiteResource app1 =await CreateWebAppAsync(resourceGroup, domain, app1Name, plan1, region);

                Utilities.Log("Created web app " + app1.Data.Name);
                Utilities.Print(app1);

                Utilities.Log("Creating another web app " + app2Name + "...");
                WebSiteResource app2 = await CreateWebAppAsync(resourceGroup, domain, app2Name, plan2, region);

                Utilities.Log("Created web app " + app2.Data.Name);
                Utilities.Print(app2);

                Utilities.Log("Creating another web app " + app3Name + "...");
                WebSiteResource app3 = await CreateWebAppAsync(resourceGroup, domain, app3Name, plan3, region);

                Utilities.Log("Created web app " + app3.Data.Name);
                Utilities.Print(app3);

                Utilities.Log("Creating another web app " + app3Name + "...");
                WebSiteResource app4 = await CreateWebAppAsync(resourceGroup, domain, app4Name, plan1, region);

                Utilities.Log("Created web app " + app4.Data.Name);
                Utilities.Print(app4);

                Utilities.Log("Creating another web app " + app3Name + "...");
                WebSiteResource app5 = await CreateWebAppAsync(resourceGroup, domain, app5Name, plan1, region);

                Utilities.Log("Created web app " + app5.Data.Name);
                Utilities.Print(app5);

                //============================================================
                // Create a traffic manager

                Utilities.Log("Creating a traffic manager " + trafficManagerName + " for the web apps...");

                var trafficManagerCollection = resourceGroup.GetTrafficManagerProfiles();
                var trafficData = new TrafficManagerProfileData()
                {
                    TrafficRoutingMethod = Azure.ResourceManager.TrafficManager.Models.TrafficRoutingMethod.Weighted,
                    Endpoints =
                    {
                        new TrafficManagerEndpointData()
                        {
                            Name = "endpoint1",
                            TargetResourceId = app1.Id
                        },
                        new TrafficManagerEndpointData()
                        {
                            Name = "endpoint2",
                            TargetResourceId = app2.Id
                        },
                        new TrafficManagerEndpointData()
                        {
                            Name = "endpoint3",
                            TargetResourceId = app3.Id
                        }
                    }
                };
                var traffic_lro =await trafficManagerCollection.CreateOrUpdateAsync(WaitUntil.Completed, trafficManagerName, trafficData);
                var trafficManager = traffic_lro.Value;

                Utilities.Log("Created traffic manager " + trafficManager.Data.Name);

                //============================================================
                // Scale up the app service plans

                Utilities.Log("Scaling up app service plan " + plan1Name + "...");

                await plan1.UpdateAsync(new AppServicePlanPatch()
                {
                    TargetWorkerCount = plan1.Data.TargetWorkerCount*2
                });

                Utilities.Log("Scaled up app service plan " + plan1Name);
                Utilities.Print(plan1);

                Utilities.Log("Scaling up app service plan " + plan2Name + "...");

                await plan2.UpdateAsync(new AppServicePlanPatch()
                {
                    TargetWorkerCount = plan2.Data.TargetWorkerCount * 2
                });

                Utilities.Log("Scaled up app service plan " + plan2Name);
                Utilities.Print(plan2);

                Utilities.Log("Scaling up app service plan " + plan3Name + "...");

                await plan1.UpdateAsync(new AppServicePlanPatch()
                {
                    TargetWorkerCount = plan1.Data.TargetWorkerCount * 2
                });

                Utilities.Log("Scaled up app service plan " + plan3Name);
                Utilities.Print(plan3);
            }
            finally
            {
                try
                {
                    Utilities.Log("Deleting Resource Group: " + resourceGroupName);
                    await resourceGroup.DeleteAsync(WaitUntil.Completed);
                    Utilities.Log("Deleted Resource Group: " + resourceGroupName);
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Utilities.Log(g);
                }
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);

                // Print selected subscription
                Utilities.Log("Selected subscription: " + client.GetSubscriptions().Id);

                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }

        private static async Task<AppServicePlanResource> CreateAppServicePlanAsync(ResourceGroupResource resourceGroup, string name, AzureLocation region)
        {
            var collection = resourceGroup.GetAppServicePlans();
            var data = new AppServicePlanData(region)
            {
            };
            var lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, name, data);
            return lro.Value;
        }
        private static async Task<WebSiteResource> CreateWebAppAsync(ResourceGroupResource resourceGroup, AppServiceDomainResource domain, string name, AppServicePlanResource plan, AzureLocation region)
        {
            var webSiteCollection = resourceGroup.GetWebSites();
            var webSiteData = new WebSiteData(region)
            {
                SiteConfig = new Azure.ResourceManager.AppService.Models.SiteConfigProperties()
                {
                    WindowsFxVersion = "PricingTier.StandardS1",
                    NetFrameworkVersion = "NetFrameworkVersion.V4_6",
                },
                AppServicePlanId = plan.Id,
            };
            var webSite_lro = await webSiteCollection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, name, webSiteData);
            var website = webSite_lro.Value;
            var bindCollection = website.GetSiteHostNameBindings();
            var bindingData = new HostNameBindingData()
            {
                CustomHostNameDnsRecordType = CustomHostNameDnsRecordType.CName,
                HostNameType = AppServiceHostNameType.Managed,
                AzureResourceType = AppServiceResourceType.Website,
                SslState = HostNameBindingSslState.SniEnabled
            };
            var binding_lro = await bindCollection.CreateOrUpdateAsync(WaitUntil.Completed, Utilities.CreateRandomName("binging-"), bindingData);
            var binding = binding_lro.Value;
            return website;
        }
    }
}