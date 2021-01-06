using Microsoft.Extensions.Caching.Memory;
using Microsoft.WindowsAzure.Storage.Table;

namespace Services
{
    public class StreamServiceFactory : IStreamServiceFactory
    {
        private readonly CloudTable _cloudTable;
        private readonly CloudTable _prospectEntityTable;
        //private readonly IHttpLog _logger;
        //private readonly IGuidedDb _guidedDb;
        private readonly RedisProvider _redisProvider;
        //private readonly IGuidedCache _guidedCache;
        //private readonly IBlobEncryptionService _blobService;
        private readonly IMemoryCache _memoryCache;

        public StreamServiceFactory(CloudTableClient cloudTableClient,/* IHttpLog logger, IGuidedDb guidedDb,*/
            RedisProvider redisProvider,/* IGuidedCache guidedCache, IBlobEncryptionService blobService,*/ IMemoryCache memoryCache)
        {
            _cloudTable          = cloudTableClient.GetTableReference(TableNames.GuidedActivationPayrollEventStore);
            _prospectEntityTable = cloudTableClient.GetTableReference(TableNames.ProspectStreamEntities);
            //_logger              = logger;
            //_guidedDb            = guidedDb;
            _redisProvider       = redisProvider;
            //_guidedCache         = guidedCache;
            //_blobService         = blobService;
            _memoryCache         = memoryCache;
        }

        public IStreamService CreateStreamService(string aggregateRoot, bool createStream = true)
        {
            return new StreamService(_cloudTable, _prospectEntityTable, aggregateRoot,/* _logger, _guidedDb, _redisProvider,
                _guidedCache, _blobService,*/ _memoryCache, createStream);
        }
    }
    public static class TableNames
    {
        public const string ClientValidations = "ClientValidations";
        public const string AuditVersion = "AuditVersions";
        public const string GuidedActivationPayrollEventStore = "GuidedActivationPayrollEventStore";            
        public const string Application = "Application";
        public const string DeferedProgressClient = "DeferedProgressClient";
        public const string DeferedProgressClientDepartments = "DeferedProgressClientDepartments";
        public const string DeferedProgressClientEarnings = "DeferedProgressClientEarnings";
        public const string DeferedProgressClientDeductions = "DeferedProgressClientDeductions";
        public const string DeferedProgressClientTaxes = "DeferedProgressClientTaxes";
        public const string DeferedProgressEmployees = "DeferedProgressEmployees";
        public const string DeferedProgressEmployeeProfile = "DeferedProgressEmployeeProfile";
        public const string DeferedProgressEmployeeEarnings = "DeferedProgressEmployeeEarnings";
        public const string DeferedProgressEmployeePayrates = "DeferedProgressEmployeePayrates";
        public const string DeferedProgressEmployeePriorPays = "DeferedProgressEmployeePriorPays";
        public const string DeferedProgressEmployeeDirectDeposits = "DeferedProgressEmployeeDirectDeposits";
        public const string DeferedProgressEmployeeDeductions = "DeferedProgressEmployeeDeductions";
        public const string DeferedProgressEmployeeTaxes = "DeferedProgressEmployeeTaxes";
        public const string DeferedProgressEmployeePersonalInformation = "DeferedProgressEmployeePersonalInformation";
        public const string DeferedProgressEmployeeEmergencyContacts = "DeferedProgressEmployeeEmergencyContacts";
        public const string DeferedProgressEmployeeCustomFields = "DeferedProgressEmployeeCustomFields";
        public const string PushLog = "PushLog";
        public const string DeferedDuplicateEvents = "DeferedDuplicateEvents";
        public const string ParallelArchiveData = "ParallelArchiveData";
        public const string ParallelRunLog = "ParallelRunLog";
        public const string UnknownValueOption = "UnknownValueOption";
        public const string ProspectStreamEntities = "ProspectStreamEntities";
        public const string ValidationLog = "ValidationLog";
        public const string ProcessCompleteCheckSchedule = "ProcessCompleteCheckSchedule";
        public const string ProcessDeferredProgress = "ProcessDeferredProgress";
    }
}