using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyTestFunction
{
    public class AppConfig : ConfigBase<AppConfig>, IGuidedCosmosConfig, IHttpSettings, IBlobConfigurationService
    {
        public string CosmosEndpoint { get; set; }
        public string CosmosAuthKey { get; set; }
        public string CosmosDbGuided { get; set; }
        public string CosmosProspectCollection { get; set; }
        public string CosmosProspectCollectionv2 { get; set; }
        public string CosmosReferenceCollection { get; set; }
        public string StackifyApiKey { get; set; }
        public string StackifyEnvironment { get; set; }
        public string StackifyAppName => "paycor.guidedactivationpayroll.functions";
        public string StackifyMinimumLogLevel => "Trace";
        public string StackifyMaximumLogLevel => "Fatal";
        public string GlobalAppName { get; set; }
        public int LogThreshold { get; set; }
        public Guid DefaultClientId => Guid.Parse("0e7f4e23-d173-4b54-84fb-2427a1d3cc55");
        private string _azureClientId { get; set; }
        private string _azureClientSecret { get; set; }
        private string _azureSymmetricKeyUrl { get; set; }
        public string APPINSIGHTS_INSTRUMENTATIONKEY { get; set; }
        public string OAuthClientId { get; set; }
        public string OAuthClientSecret { get; set; }

        protected override void Init()
        {
            CosmosEndpoint = GetSetting("PaycorCosmosDB-Doc-URI");
            CosmosAuthKey = GetSetting("GuidedActivationPayroll-Cosmos-Key");
            CosmosDbGuided = "GuidedActivationPayroll";
            CosmosProspectCollection = "ProspectCollection";
            CosmosProspectCollectionv2 = "ProspectCollectionV2";
            CosmosReferenceCollection = "ReferenceCollection";
            StackifyApiKey = GetSetting("Stackify.ApiKey");
            StackifyEnvironment = GetSetting("Stackify.Environment");
            GlobalAppName = "Paycor.GuidedActivationPayroll.Functions";
            PaycorServiceBusConnection = GetSetting("PaycorServiceBusConnection");
            CosmosUpdateBatchSize = GetIntSettings("cosmos.updateBatchSize", 100);
            CosmosQueryPageSize = GetIntSettings("cosmos.queryPageSize", 100);
            SecurityApimKey = GetSetting("GuidedActivationPayroll-APIM-Key");
            PayrollPrivateKey = GetSetting("GuidedActivationPayroll-API-PrivateKey");
            PayrollPublicKey = GetSetting("GuidedActivationPayroll-API-PublicKey");
            RedisConnectionString = GetSetting("GuidedActivationPayroll-Redis-ConnectionString");
            StorageConnectionString = GetSetting("GuidedActivationPayroll-Storage-ConnectionString");
            GettingStartedServiceBusConnectionString = GetSetting("GettingStarted-ServiceBus-ConnectionString");
            LogThreshold = GetIntSettings("cosmos.logthreshold", 100);
            GuidedActivationPayrollServiceBusConnectionString = GetSetting("GuidedActivationPayrollServiceBusConnectionString");
            OAuthClientId = GetSetting("PaycorGuidedActivationPayrollFunctions-OAuth-ClientID");
            OAuthClientSecret = GetSetting("PaycorGuidedActivationPayrollFunctions-OAuth-ClientSecret");

            var apimBaseUrl = GetSetting("APIM_BaseURL");
            PayrollApiUri = $"https://{apimBaseUrl}/payroll";
            PayrollPrivateApiUri = $"https://{apimBaseUrl}/payrollprivate";
            EmployeeApiUri = $"https://{apimBaseUrl}/employee";
            CompanyApiUri = $"https://{apimBaseUrl}/company";
            CompanyPrivateApiUri = $"https://{apimBaseUrl}/companyprivate";
            BaseUrl = $"https://{apimBaseUrl}";
            GettingStartedApiUri = $"https://{apimBaseUrl}/gettingstarted";
            PerformApiUri = $"https://{apimBaseUrl}/performapi";
            PayrollProcessingApiUri = $"https://{apimBaseUrl}/payroll";
            FileStoreApiUri = $"https://{apimBaseUrl}/filestore";
            DocumentsApiUri = $"https://{apimBaseUrl}/documents";

            _azureClientId = Environment.GetEnvironmentVariable("GuidedActivation-KeyVault-ClientId", EnvironmentVariableTarget.Process);
            _azureClientSecret = Environment.GetEnvironmentVariable("GuidedActivation-KeyVault-ClientIDKey", EnvironmentVariableTarget.Process);
            _azureSymmetricKeyUrl = Environment.GetEnvironmentVariable("GuidedActivation-KeyVault-SecretURL", EnvironmentVariableTarget.Process);
            APPINSIGHTS_INSTRUMENTATIONKEY = GetSetting("APPINSIGHTS_INSTRUMENTATIONKEY");

        }

        public string StorageConnectionString { get; set; }
        public string GuidedActivationPayrollServiceBusConnectionString { get; set; }
        public string GettingStartedServiceBusConnectionString { get; set; }
        public string BaseUrl { get; set; }
        public string GettingStartedApiUri { get; set; }
        public string CompanyPrivateApiUri { get; set; }
        public string PayrollApiUri { get; set; }
        public string PerformApiUri { get; set; }
        public string CompanyApiUri { get; set; }
        public string PayrollPrivateKey { get; set; }
        public string PayrollPublicKey { get; set; }
        public string SecurityApimKey { get; set; }
        public string PayrollPrivateApiUri { get; set; }
        public string EmployeeApiUri { get; set; }
        public string RedisConnectionString { get; set; }
        public string PaycorServiceBusConnection { get; set; }
        public string PayrollProcessingApiUri { get; set; }
        public string FileStoreApiUri { get; set; }
        public string DocumentsApiUri { get; set; }
        public int CosmosUpdateBatchSize { get; set; }
        public int CosmosQueryPageSize { get; set; }


        public string AzureStorageAccessKey() => StorageConnectionString;
        public string AzureClientId() => _azureClientId;
        public string AzureClientSecret() => _azureClientSecret;
        public string AzureSymmetricKeyUrl() => _azureSymmetricKeyUrl;
        public int BufferSize() => 1048576;
    }

    public abstract class ConfigBase<T> where T : class, new()
    {
        private static T _instance = null;

        static ConfigBase()
        {
            _instance = new T();
        }
        protected ConfigBase()
        {
            Init();
        }
        public static T Default { get { return _instance; } }
        protected abstract void Init();
        protected string GetSetting(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
        protected bool GetBoolSetting(string name)
        {
            return Convert.ToBoolean(Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process));
        }
        protected int GetIntSettings(string name, int? defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
            if (value == null)
            {
                return defaultValue.Value;
            }

            return Convert.ToInt32(value);
        }
    }
    
    public interface IGuidedCosmosConfig
    {
        string CosmosEndpoint { get; set; }
        string CosmosAuthKey { get; set; }
        string CosmosDbGuided { get; set; }
        string CosmosProspectCollection { get; set; }
        string CosmosProspectCollectionv2 { get; set; }
        string CosmosReferenceCollection { get; set; }
        Guid DefaultClientId { get; }
        string APPINSIGHTS_INSTRUMENTATIONKEY { get; }
        int LogThreshold { get; }
        int CosmosUpdateBatchSize { get; }
        int CosmosQueryPageSize { get; }
    }
    public interface IHttpSettings
    {
        string PayrollApiUri { get; set; }
        string PerformApiUri { get; set; }
        string CompanyApiUri { get; set; }
        string PayrollPrivateKey { get; set; }
        string PayrollPublicKey { get; set; }
        string SecurityApimKey { get; set; }
        string PayrollPrivateApiUri { get; set; }
        string EmployeeApiUri { get; set; }
        string RedisConnectionString { get; set; }
        string StorageConnectionString { get; set; }
        string GuidedActivationPayrollServiceBusConnectionString { get; set; }
        string GettingStartedApiUri { get; set; }
        string CompanyPrivateApiUri { get; set; }
        string PayrollProcessingApiUri { get; set; }
        string FileStoreApiUri { get; set; }
        string DocumentsApiUri { get; set; }
        string GettingStartedServiceBusConnectionString { get; set; }
        string PaycorServiceBusConnection { get; set; }
        string OAuthClientId { get; set; }
        string OAuthClientSecret { get; set; }
    }
    public interface IBlobConfigurationService
    {
        string AzureStorageAccessKey();
        string AzureClientId();
        string AzureClientSecret();
        string AzureSymmetricKeyUrl();
        int BufferSize();
    }
}
