using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
           builder.Services.AddLogging();
           builder.Services.AddScoped<IValidateService, ValidateService>(); 
        }
    }
}
