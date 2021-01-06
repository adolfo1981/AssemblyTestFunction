using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using StackExchange.Redis;
using Streamstone;

namespace Services
{
    public class SnapShotTableEntity : TableEntity
    {
        public static string LastestSnapShot = "LatestSnapShot";

        public SnapShotTableEntity()
        {
        }

        public SnapShotTableEntity(string partition, Guid prospectId, int streamVersion, Guid snapShotLocation, Guid? previousSnapShotLocation, string snapShotPath = null, string previousSnapShotPath = null)
        {
            this.PartitionKey = partition;
            this.RowKey       = snapShotLocation.ToString();

            StreamVersion            = streamVersion;
            SnapShotLocation         = snapShotLocation;
            PreviousSnapShotLocation = previousSnapShotLocation;
            ProspectId               = prospectId;
            SnapShotPath             = snapShotPath;
            PreviousSnapShotPath     = previousSnapShotPath;
        }
        
        public Guid? PreviousSnapShotLocation { get; set; }
        public Guid SnapShotLocation { get; set; }
        public string PreviousSnapShotPath { get; set; }
        public string SnapShotPath { get; set; }
        public Guid ProspectId { get; set; }
        public int StreamVersion { get; set; }

    }

    public class ProspectStreamEntity : TableEntity
    {
        public ProspectStreamEntity()
        {}
        
        public ProspectStreamEntity(Guid prospectId, string itemId, string streamName, DateTime dateModified, string modelType, string partitionName)
        {
            PartitionKey = prospectId.ToString();
            RowKey = partitionName;

            ProspectId    = prospectId;
            ItemId        = itemId;
            StreamName    = streamName;
            DateModified  = dateModified;
            ModelType     = modelType;
        }
        
        public Guid ProspectId { get; set; }
        public string ItemId { get; set; }
        public string StreamName { get; set; }
        public DateTime DateModified { get; set; }
        public string ModelType { get; set; }
    }
    
    public class StreamService : IStreamService
    {
        public const int SnapShotVersionCount = 100;
        public const int EventPageCount = 500;
        private readonly Random _random = new Random();

        //private readonly IGuidedCache _guidedCache;
        //private readonly IBlobEncryptionService _blobService;
        protected readonly Partition _partition;
        //protected readonly IHttpLog _logger;
        protected readonly ILogger _logger;
        protected readonly bool _createStream;
        //private readonly IGuidedDb _guidedDb;
        private readonly CloudTable _cloudTable;
        private readonly CloudTable _prospectEntityTable;
        private string _partitionName;

        protected Stream _stream;
        private readonly RedisProvider _redisProvider;
        private readonly IMemoryCache _memoryCache;

        public async Task<bool> HasEventsAsync()
        {
            if (_stream == null)
            {
                await OpenStreamAsync();
            }

            return _stream.Version > 0;
        }

        public StreamService(CloudTableClient cloudTableClient, string partitionName,/* IHttpLog logger,
            IGuidedDb guidedDb, RedisProvider redisProvider, IGuidedCache guidedCache, IBlobEncryptionService blobService,*/ bool createStream = true)
        {
            _cloudTable          = cloudTableClient.GetTableReference(TableNames.GuidedActivationPayrollEventStore);
            _prospectEntityTable = cloudTableClient.GetTableReference(TableNames.ProspectStreamEntities);
            _partition           = new Partition(_cloudTable, partitionName);
            //_logger              = logger;
            //_guidedDb            = guidedDb;
            _stream              = null;
            _createStream        = createStream;
            //_redisProvider       = redisProvider;
            //_guidedCache         = guidedCache;
            //_blobService         = blobService;
            _partitionName       = partitionName;
        }


        public StreamService(CloudTable cloudTable, CloudTable prospectEntityTable, string partitionName,/* IHttpLog logger, IGuidedDb guidedDb,
            RedisProvider redisProvider, IGuidedCache guidedCache, IBlobEncryptionService blobService, */IMemoryCache memoryCache, bool createStream = true)
        {
            _prospectEntityTable = prospectEntityTable;
            _cloudTable          = cloudTable;
            _partition           = new Partition(cloudTable, partitionName);
            //_logger              = logger;
            //_guidedDb            = guidedDb;
            _stream              = null;
            _createStream        = createStream;
            //_redisProvider       = redisProvider;
            //_guidedCache         = guidedCache;
            //_blobService         = blobService;
            _partitionName       = partitionName;
            _memoryCache         = memoryCache; 
        }

        protected async Task OpenStreamAsync()
        {
            if (_partition == null)
            {
                throw new ArgumentNullException($"Call Set aggregate root to the correct stream");
            }

            var existent = await Stream.TryOpenAsync(_partition);
            if (existent.Found == false)
            {
                if (!_createStream)
                {
                    throw new ArgumentException($"{_partition.Key} partition stream not found");
                }

                _stream = new Stream(_partition);
            }
            else
            {
                _stream = existent.Stream;
            }
        }

        public async Task<int> GetStreamVersion()
        {
            if (_stream == null)
            {
                await OpenStreamAsync();
            }

            return _stream.Version;
        }

        public async Task<List<EventDataEntity>> GetEventsFromVersion(int startVersion, int? endVersion = null)
        {
            if (_stream == null)
            {
                await OpenStreamAsync();
            }

            var eventData = new List<EventDataEntity>();
            if (startVersion == 0)
            {
                return eventData;
            }

            var currentVersion = startVersion;
            while (currentVersion <= (endVersion ?? _stream.Version))
            {
                var pageCount = ((endVersion ?? _stream.Version) - currentVersion) + 1;
                if (pageCount > EventPageCount)
                {
                    pageCount = EventPageCount;
                }

                var slice = await Stream.ReadAsync<EventDataEntity>(_partition, currentVersion, pageCount);
                eventData.AddRange(slice.Events.ToList());
                if (slice.Events.Length == 0)
                {
                    break;
                }

                currentVersion += slice.Events.Length;
            }

            return eventData.ToList();
        }
        
        public async Task<List<EventDataEntity>> GetEventsFromTime(int startVersion, DateTime dateTime)
        {
            if (_stream == null)
            {
                await OpenStreamAsync();
            }

            var eventData = new List<EventDataEntity>();
            if (startVersion == 0)
            {
                return eventData;
            }
 
            var currentVersion = startVersion;
            while (currentVersion <= _stream.Version)
            {
                var pageCount = (_stream.Version - currentVersion) + 1;
                if (pageCount > EventPageCount)
                {
                    pageCount = EventPageCount;
                }

                var slice = await Stream.ReadAsync<EventDataEntity>(_partition, currentVersion, pageCount);
                var events = slice.Events.Where(e => e.DateTime <= dateTime).ToList();
                eventData.AddRange(events);
                if (slice.Events.Length == 0 || events.Count() != slice.Events.Length)
                {
                    break;
                }

                currentVersion += slice.Events.Length;
            }

            return eventData.ToList();
        }

        public async Task<T> LoadModelVersion<T>(int modelVersion) where T : TypedEventModel, IClientEntity, new()
        {
            
            T model;
            model = await GetCachedModel<T>(modelVersion);
            if (model != null)
            {
                return model;
            }

            if (_stream == null)
            {
                await OpenStreamAsync();
            }
            model = new T();
            if (modelVersion == 0 || !await HasEventsAsync())
            {
                return model;
            }

            var retrieveOperation = TableOperation.Retrieve<SnapShotTableEntity>(_partition.PartitionKey, SnapShotTableEntity.LastestSnapShot);
            var startVersion = 1;
            var snapShot     = await _cloudTable.ExecuteAsync(retrieveOperation);
            try
            {
                var latestSnapShot = (SnapShotTableEntity)snapShot.Result;
                while (latestSnapShot?.StreamVersion > startVersion &&
                       latestSnapShot.PreviousSnapShotLocation.HasValue)
                {
                    retrieveOperation = TableOperation.Retrieve<SnapShotTableEntity>(
                        _partition.PartitionKey,
                        latestSnapShot.PreviousSnapShotLocation.Value.ToString());
                    
                    snapShot = await _cloudTable.ExecuteAsync(retrieveOperation);
                    if (snapShot.Result != null)
                    {
                        latestSnapShot = (SnapShotTableEntity)snapShot.Result;
                    }
                }
                if (latestSnapShot != null && latestSnapShot.StreamVersion < modelVersion)
                {
                    var snapShotModel = await GetSnapShotModel(latestSnapShot);
                    var jsonString = snapShotModel.SnapShotJson;
                    if (snapShotModel.IsGzipCompression)
                    {
                        var bytes = Convert.FromBase64String(jsonString);
                        jsonString = Compression.Unzip(bytes);
                    }
                    model = JsonConvert.DeserializeObject<T>(jsonString,
                        new JsonSerializerSettings
                        {
                            Formatting            = Formatting.None,
                            ContractResolver      = new CamelCasePropertyNamesContractResolver(),
                            DateTimeZoneHandling  = DateTimeZoneHandling.Utc,
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
                    startVersion = snapShotModel.StreamVersion;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading snapshot {_partition.PartitionKey}", ex);
            }

            var events = await GetEventsFromVersion(startVersion, modelVersion);
            model.PlayEvents(EventHelper.GetObjects(events.ToArray()));

            return model;
        }
        
        public async Task<T> LoadModelAtTime<T>(DateTime dateTime) where T : TypedEventModel, IClientEntity, new()
        {
            if (_stream == null)
            {
                await OpenStreamAsync();
            }


            var model = new T();
            if (!await HasEventsAsync())
            {
                return model;
            }

            var startVersion = 1;
            var events = await GetEventsFromTime(startVersion, dateTime);
            if (!events.Any())
            {
                return null;
            }
            model.PlayEvents(EventHelper.GetObjects(events.ToArray()));

            return model;
        }

        public class StreamVersion
        {
            public int Version { get; set; }
        }

        public async Task<T> LoadModel<T>(int? startVersion = null, int? pageCount = null, bool ignoreCache = false, bool ignoreSnapshot = false) where T : TypedEventModel, IClientEntity, new()
        {

            await OpenStreamAsync();
            var streamVersion = _stream.Version;

            T model;
            if (!ignoreCache)
            {
                model = await GetCachedModel<T>(streamVersion);
                if (model != null)
                {
                    return model;
                }
            }

            var redisKey = $"GuidedEventModel-{_partition.Key}-{streamVersion}";  

            model = new T();
            SnapShotTableEntity latestSnapShot = null;
            if (streamVersion > 0)
            {
                if (!startVersion.HasValue && !ignoreSnapshot)
                {
                    try
                    {
                        var retrieveOperation = TableOperation.Retrieve<SnapShotTableEntity>(_partition.PartitionKey, SnapShotTableEntity.LastestSnapShot);
                        var snapShot          = await _cloudTable.ExecuteAsync(retrieveOperation);
                        if (snapShot.Result != null)
                        {
                            latestSnapShot = (SnapShotTableEntity)snapShot.Result;
                            model = await GetSavedSnapshot<T>(latestSnapShot);
                            startVersion = latestSnapShot.StreamVersion;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error loading snapshot {_partition.PartitionKey}", ex);
                    }
                }

                var currentVersion = startVersion ?? 1;
                var events = await GetEventsFromVersion(currentVersion, streamVersion);
                model.PlayEvents(EventHelper.GetObjects(events.ToArray()));

                var snapShotOnSave = (streamVersion - (startVersion ?? 1)) > SnapShotVersionCount;
                if (snapShotOnSave)
                {
                    await SaveSnapShot(model, streamVersion, latestSnapShot);
                }
            }
            CacheModelVersion(redisKey, model);
            return model;
        }


        private async Task<SnapShotModel> GetSnapShotModel(SnapShotTableEntity latestSnapshot)
        {
            //var prospect = await _guidedCache.GetClient(latestSnapshot.ProspectId.ToString());

            //if (string.IsNullOrEmpty(latestSnapshot.SnapShotPath) == false)
            //{
            //    _blobService.ChangeContainer(ContainerNames.Snapshots());
            //    return await _blobService.GetJsonAndDecryptAsync<SnapShotModel>(latestSnapshot.SnapShotPath);
            //}
            
            //_guidedDb.SetClientScope(prospect);
                            
            //var snapShotModel = await _guidedDb.GetItemAsync<SnapShotModel>(
            //    latestSnapshot.SnapShotLocation.ToString(),
            //    _guidedDb.PartitionKeyGenerator.GetClientSnapShotTablePartitionKeyVersion(latestSnapshot.StreamVersion));
                            
            //if (snapShotModel == null)
            //{
            //    throw new ArgumentException(
            //        $"Snap shot model not found {_partition.PartitionKey} {latestSnapshot.SnapShotLocation}");
            //}

            return null;
        }
        
        private async Task<T> GetSavedSnapshot<T>(SnapShotTableEntity latestSnapshot)
        {
            var snapShotModel = await GetSnapShotModel(latestSnapshot);
            var jsonString = snapShotModel.SnapShotJson;
            if (snapShotModel.IsGzipCompression)
            {
                var bytes = Convert.FromBase64String(jsonString);
                jsonString = Compression.Unzip(bytes);
            }

            return JsonConvert.DeserializeObject<T>(jsonString,
                new JsonSerializerSettings
                {
                    Formatting            = Formatting.None,
                    ContractResolver      = new CamelCasePropertyNamesContractResolver(),
                    DateTimeZoneHandling  = DateTimeZoneHandling.Utc,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
        }

        private async Task<T> GetCachedModel<T>(int streamVersion)
            where T : TypedEventModel, IClientEntity, new()
        {
            if (streamVersion > 0)
            {
                if (_redisProvider != null)
                {
                    var redisKey = $"GuidedEventModel-{_partition.Key}-{streamVersion}";
                    try
                    {
                        //var cacheModel = _redisProvider.GetItemFromMemory<T>(_logger, redisKey, _memoryCache);
                        //if (cacheModel != null)
                        //{
                        //    return cacheModel;
                        //}
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error loading model from cache {_partition.Key}", ex);
                    }
                }
            }

            return null;
        }

        private void CacheModelVersion<T>(string redisKey, T model)
            where T : TypedEventModel, IClientEntity, new()
        {
            if (_redisProvider != null)
            {
                try
                {
                    //_redisProvider.SaveToMemory(redisKey, TimeSpan.FromMinutes(5), model, _memoryCache, 5);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving redis model cache {redisKey}", ex);
                }
            }
        }
        
        private async Task SaveSnapShot<T>(T model, int streamVersion, SnapShotTableEntity latestSnapShot)
            where T : TypedEventModel, IClientEntity, new()
        {
            try
            {
                var modelJson = JsonConvert.SerializeObject(model, new JsonSerializerSettings
                {
                    Formatting            = Formatting.None,
                    ContractResolver      = new CamelCasePropertyNamesContractResolver(),
                    DateTimeZoneHandling  = DateTimeZoneHandling.Utc,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                
                var modelJsonCompressed = Convert.ToBase64String(Compression.Zip(modelJson));
                
                var nextSnapShot = new SnapShotModel()
                {
                    Id                = Guid.NewGuid(),
                    ProspectId        = model.ProspectId,
                    SnapShotJson      = modelJsonCompressed,
                    StreamVersion     = streamVersion + 1,
                    IsGzipCompression = true
                };
                //var nextSnapShotPath = FileNames.Snapshot(DateTime.UtcNow, nextSnapShot.Id);
                var snapShotPointers = new List<SnapShotTableEntity>();
                
                if (latestSnapShot != null)
                {
                    snapShotPointers.Add(new SnapShotTableEntity(
                        _partition.Key, 
                        latestSnapShot.ProspectId,
                        latestSnapShot.StreamVersion, 
                        latestSnapShot.SnapShotLocation,
                        latestSnapShot.PreviousSnapShotLocation,
                        latestSnapShot.SnapShotPath,
                        latestSnapShot.PreviousSnapShotPath));

                    latestSnapShot.ProspectId               = model.ProspectId;
                    latestSnapShot.StreamVersion            = streamVersion + 1;
                    latestSnapShot.PreviousSnapShotLocation = latestSnapShot.SnapShotLocation;
                    latestSnapShot.SnapShotLocation         = nextSnapShot.Id;
                    //latestSnapShot.SnapShotPath             = nextSnapShotPath;
                    latestSnapShot.PreviousSnapShotPath     = latestSnapShot.SnapShotPath;
                    latestSnapShot.RowKey                   = SnapShotTableEntity.LastestSnapShot;
                }
                else
                {
                    latestSnapShot = new SnapShotTableEntity(
                            _partition.Key, 
                            model.ProspectId,
                            streamVersion + 1, 
                            nextSnapShot.Id, 
                            null, 
                            null)
                        {RowKey = SnapShotTableEntity.LastestSnapShot};
                }
                
                snapShotPointers.Add(latestSnapShot);
                //var prospect = await _guidedCache.GetClient(model.ProspectId.ToString());
                //// insert snap shot json into cosmos
                //_guidedDb.SetClientScope(prospect);
                
                //_blobService.ChangeContainer(ContainerNames.Snapshots());
                //await _blobService.EncryptAndSendAsJsonAsync(nextSnapShotPath, nextSnapShot);
                
                // save snapshot points to the table
                var batchOperation = new TableBatchOperation();
                snapShotPointers.ForEach(a => batchOperation.InsertOrReplace(a));
                await _cloudTable.ExecuteBatchAsync(batchOperation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving snapshot model {_partition.Key}-{streamVersion}", ex);
            }

        }

        private async Task CaptureProspectStream<T>(T model, DateTime? dateModified, CancellationToken token) where T : TypedEventModel, IClientEntity, new()
        {
            if (model == null)
                return;

            if (model.Id == Guid.Empty)
                return;

            if (string.IsNullOrEmpty(model.StreamName))
                return;
            
            if(dateModified.HasValue && dateModified.Value == DateTime.MinValue)
                dateModified = DateTime.UtcNow;
            
            try
            {
                await Policy.Handle<StorageException>()
                    .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timespan) =>
                    {
                        token.ThrowIfCancellationRequested();
                        _logger.LogWarning($"Error when trying save captured prospect entity stream.  Trying again in {timespan.TotalSeconds}. model: prospect: {model?.ProspectId} id: {model?.Id} streamname: {model?.StreamName} modified: {dateModified}", exception); 
                    })
                    .ExecuteAsync(async () =>
                    {
                        var modelType      = model.GetType().AssemblyQualifiedName;
                        var modifiedDate   = dateModified ?? DateTime.UtcNow;
                        var prospectEntity = new ProspectStreamEntity(model.ProspectId, model.Id.ToString(), model.StreamName, modifiedDate, modelType, _partitionName);
                        var operation      = TableOperation.InsertOrReplace(prospectEntity);
                        await _prospectEntityTable.ExecuteAsync(operation, null, null, token);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Unable to capture prospect entity stream.", ex);
            }
        }

        /// <summary>
        /// Try to save the events to the stream, it will reload the model and try again if there is a ConcurrencyConflictException
        /// </summary>
        /// <typeparam name="T">TypedEventModel</typeparam>
        /// <param name="model">model to save events to</param>
        /// <param name="stopRetry">function to evaluate after the model is reloaded during a ConcurrencyConflictException, use null to not check</param>
        /// <param name="token">CancellationToken</param>
        /// <param name="retryCount">number of retries before stopping (default 10)</param>
        /// <param name="data">Events</param>
        /// <returns>latest model with events played</returns>
        public virtual async Task<T> SaveEventsWithRetry<T>(T model, Func<T, bool> stopRetry = null, CancellationToken token = default(CancellationToken), int retryCount = 100, params IDomainEvent[] data) where T : TypedEventModel, IClientEntity, new()
        {
            if (!data.Any())
            {
                return model;
            }
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            if (_stream == null)
            {
                await OpenStreamAsync();
            }
            
            model.PlayEvents(data);
            var eventData = data.Select(e => EventHelper.Event(e, model.ProspectId));
            foreach (var eventBatch in eventData.Batch(95))
            {
                await Policy
                    .Handle<ConcurrencyConflictException>()
                    .WaitAndRetryAsync(retryCount, retry => TimeSpan.FromMilliseconds(_random.Next(100, 5000)),
                        async (exception, timeSpan) =>
                        {
                            _logger.LogDebug($"Retry event stream write: {_partition.Key}, waiting for {timeSpan.TotalMilliseconds} milliseconds");
                            await OpenStreamAsync();
                            var ignoreCache = retryCount > 5;
                            
                            model = await LoadModel<T>(null, null, ignoreCache);
                            if (stopRetry != null)
                            {
                                if (stopRetry(model))
                                {
                                    throw new Exception(
                                        $"Retry event check failed, aborting stream: {_partition.Key}", exception);
                                }
                            }
                        })
                    .ExecuteAsync(ct => SaveEventData(eventBatch.ToArray()), token);
            }

            // save that the stream was created or updated
            var maxDate = data.Any() ? data.Max(e => e.DateTime) : DateTime.UtcNow;
            await CaptureProspectStream(model, maxDate, token);

            return model;
        }


        /// <summary>
        /// Try to save the events to the stream
        /// </summary>
        /// <param name="token">CancellationToken</param>
        /// <param name="retryCount">number of retries before stopping (default 10)</param>
        /// <param name="data">Events</param>
        /// <returns></returns>
        public async Task SaveEventsWithRetry(CancellationToken token = default(CancellationToken), int retryCount = 50, params IDomainEvent[] data)
        {
            if (!data.Any())
            {
                return;
            }
            if (_stream == null)
            {
                await OpenStreamAsync();
            }
                        
            var eventData = data.Select(e => EventHelper.Event(e, Guid.Empty)).ToList();
            // table storage limits to 100 events saved at once, batch it smaller
            foreach (var eventBatch in eventData.Batch(95))
            {
                token.ThrowIfCancellationRequested();
                await Policy
                    .Handle<ConcurrencyConflictException>()
                    .WaitAndRetryAsync(retryCount, retry => TimeSpan.FromMilliseconds(_random.Next(100, 5000)),
                        async (exception, timeSpan) =>
                        {
                            token.ThrowIfCancellationRequested();
                            _logger.LogDebug($"Retry event stream write: {_partition.Key}, waiting for {timeSpan.TotalMilliseconds} milliseconds");
                            await OpenStreamAsync();
                        })
                    .ExecuteAsync(() => SaveEventData(eventBatch.ToArray()));
            }
        }

        private async Task SaveEventData(EventData[] eventData)
        {
            var streamResult = await Stream.WriteAsync(_stream, eventData);            
            _stream = streamResult.Stream;
        }
    }
    public class SnapShotModel : ClientEntity, IEntity
    {
        public string PreviousSnapShotId { get; set; }
        public string tableId {get;set;}
        public int StreamVersion { get; set; }
        public string SnapShotJson { get; set; }
        public bool IsGzipCompression { get; set; }
        public DateTime SnapShotDateTime { get; set; } = DateTime.UtcNow;
    }
    public interface IClientEntity : IEntity
    {
        // this is the partition key setup in cosmos db.
        Guid ProspectId { get; set; }
    }
    public interface IEntity
    {
        Guid Id { get; set; }
    }
    public class ClientEntity : IClientEntity
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public virtual string tableId { get; }
        // prospect collection partition key
        [JsonProperty("prospectId")]
        public Guid ProspectId { get; set; }
    }

}