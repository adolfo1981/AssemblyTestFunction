using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Services;
using System.Threading;
using Microsoft.Azure.WebJobs.Host;
using System.Reflection;

namespace AssemblyTestFunction
{
    public class Function3
    {
        private readonly IValidateService _validateService;
        private ILogger _logger;
        private SimpleUnloadableAssemblyLoadContext _alc;

        public Function3(IValidateService validateService, ILoggerFactory loggerFactory)
        { 
            _logger = loggerFactory.CreateLogger<Function1>();
            _validateService = validateService;
            ApplicationHelper.Startup(_logger);
        }

        [FunctionName("Function3")]
        public async Task Run(
            [ServiceBusTrigger("validate-deductions", Connection = "GuidedActivationPayrollServiceBusConnectionString")]
            ValidateClientCommand validateClient,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function1 processed a request.");

            //var dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            var name = "Test";
            bool isValid = _validateService.ValidateName(name);

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function3 executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function1 executed successfully. Is Name Valid: {isValid}";

            return;
        }
    }
}
