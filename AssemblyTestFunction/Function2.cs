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
    public class Function2 : IFunctionInvocationFilter
    {
        private readonly IValidateService _validateService;
        private ILogger _logger;
        private SimpleUnloadableAssemblyLoadContext _alc;

        public Function2(IValidateService validateService, ILoggerFactory loggerFactory)
        { 
            _logger = loggerFactory.CreateLogger<Function1>();
            _validateService = validateService;
            ApplicationHelper.Startup(_logger);
        }

        public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            _alc.Unload();
            return Task.CompletedTask;
        }

        public Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            //Simulate loading Domain assembly multiple times
            _alc = new SimpleUnloadableAssemblyLoadContext();
            var dllPath = Assembly.GetExecutingAssembly().Location;
            var dllParentPath = Path.GetDirectoryName(dllPath);
            _alc.LoadFromAssemblyPath(Path.Combine(dllParentPath, "Domain.dll"));

            return Task.CompletedTask;
        }

        [FunctionName("Function2")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function2 processed a request.");

            string phone = req.Query["phone"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            phone = phone ?? data?.phone;

            bool isValid = _validateService.ValidatePhone(phone);

            string responseMessage = string.IsNullOrEmpty(phone)
                ? "This HTTP triggered function2 executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"This HTTP triggered function2 executed successfully. Is Phone Valid: {isValid}";

            //var rand = new System.Random();
            //var timeout = rand.Next(1,3) * 1000;

            //log.LogDebug($"TIMEOUT: {timeout}");

            //Thread.Sleep(timeout);

            return new OkObjectResult(responseMessage);
        }
    }
}
