using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ume_Chat_External_General;
using Ume_Chat_KeyVaultProvider;

var host = new HostBuilder().ConfigureFunctionsWorkerDefaults(builder => builder.Services.AddLogging())
                            .ConfigureAppConfiguration((context, builder) =>
                                                       {
                                                           builder.AddEnvironmentVariables();
                                                           builder.SetBasePath(Environment.CurrentDirectory);
                                                           builder.AddJsonFile("appsettings.json");

                                                           if (context.HostingEnvironment.IsDevelopment())
                                                               builder.AddJsonFile("appsettings.Development.json",
                                                                                   true);

                                                           // Configure Key Vault from appsettings 
                                                           builder.AddAzureKeyVaultWithReferenceSupport();

                                                           var appConfigConnectionString =
                                                               builder.Build()
                                                                   ["DATASYNC_APP_CONFIGURATION_CONNECTION_STRING"];
                                                           builder.AddAzureAppConfiguration(o =>
                                                                                                o.Connect(appConfigConnectionString));

                                                           builder.AddVariables();
                                                       })
                            .Build();

host.Run();
