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
using System.Linq;
using Microsoft.Azure.WebJobs.Host;
using System.Reflection;

namespace AssemblyTestFunction
{
    public class Function1 : IFunctionInvocationFilter
    {
        private readonly IValidateService _validateService;
        private ILogger _logger;
        private SimpleUnloadableAssemblyLoadContext _alc;

        private void PrintCount(string msg)
        {
           var assemblies = AppDomain.CurrentDomain.GetAssemblies();
           var domainDllCount = AppDomain.CurrentDomain.GetAssemblies().Count(x => x.FullName == "Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
          _logger.LogInformation($"DOMAIN ASSEMBLIES LOADED ({msg}): {domainDllCount}");
        }

        public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            _alc.Unload();
            PrintCount("POST");
            return Task.CompletedTask;
        }

        public Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            _alc = new SimpleUnloadableAssemblyLoadContext();
            var dllPath = Assembly.GetExecutingAssembly().Location;
            var dllParentPath = Path.GetDirectoryName(dllPath);
            _alc.LoadFromAssemblyPath(Path.Combine(dllParentPath, "Domain.dll"));
            PrintCount("PRE");
            return Task.CompletedTask;
        }

        public Function1(IValidateService validateService, ILoggerFactory loggerFactory)
        { 
            _logger = loggerFactory.CreateLogger<Function1>();
            _validateService = validateService;
            ApplicationHelper.Startup(_logger);
        }

        [FunctionName("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function1 processed a request.");

            var dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            bool isValid = _validateService.ValidateName(name);

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function1 executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function1 executed successfully. Is Name Valid: {isValid}";

            //var rand = new System.Random();
            //var timeout = rand.Next(1,3) * 1000;

            //log.LogDebug($"TIMEOUT: {timeout}");

            //Thread.Sleep(timeout);

            return new OkObjectResult(responseMessage);
        }
    }
}
