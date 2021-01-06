using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Services;
using System;
using System.Collections.Generic;
using System.Text;


[assembly: WebJobsStartup(typeof(AssemblyTestFunction.Startup))]

namespace AssemblyTestFunction
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            var appConfig = new AppConfig();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<AppConfig>(x => appConfig);
            builder.Services.AddSingleton<IGuidedCosmosConfig>(x => appConfig);
            builder.Services.AddLogging();
            builder.Services.AddScoped<IValidateService, ValidateService>(); 
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(appConfig.StorageConnectionString);
            builder.Services.AddSingleton<IHttpSettings>(x => appConfig);
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IStreamServiceFactory>(x =>
                                                          new StreamServiceFactory(x.GetRequiredService<CloudTableClient>(),
                                                                                   //x.GetRequiredService<IHttpLog>(),
                                                                                   //x.GetRequiredService<IGuidedDb>(),
                                                                                   x.GetRequiredService<RedisProvider>(),
                                                                                   //x.GetRequiredService<IGuidedCache>(),
                                                                                   //x.GetRequiredService<IBlobEncryptionService>(),
                                                                                   x.GetRequiredService<IMemoryCache>()));
        }
    }
}
