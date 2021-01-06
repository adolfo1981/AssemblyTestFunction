using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace Services
{
    public interface IStreamService
    {
        Task<T> LoadModel<T>(int? startVersion = null, int? pageCount = null, bool ignoreCache = false, bool ignoreSnapshot = false) where T : TypedEventModel, IClientEntity, new();
        Task<T> LoadModelVersion<T>(int modelVersion)
            where T : TypedEventModel, IClientEntity, new();

        Task<T> LoadModelAtTime<T>(DateTime dateTime) where T : TypedEventModel, IClientEntity, new();

        Task<List<EventDataEntity>> GetEventsFromVersion(int startVersion, int? endVersion = null);
        //Task SaveEvents<T>(T model, params IDomainEvent[] data) where T : TypedEventModel,IClientEntity, new();
        //Task SaveEvents<T>(T model, List<IDomainEvent> data) where T : TypedEventModel,IClientEntity, new();
        //Task SaveEvents<T>(T model, List<IAuditedEventModel> data) where T : TypedEventModel,IClientEntity, new();

        Task SaveEventsWithRetry(CancellationToken token = default(CancellationToken),
            int retryCount = 10, params IDomainEvent[] data);
        Task<T> SaveEventsWithRetry<T>(T model, Func<T, bool> stopRetry = null, CancellationToken token = default(CancellationToken), int retryCount = 10, params IDomainEvent[] data) where T : TypedEventModel,IClientEntity, new();
        Task<bool> HasEventsAsync();
        Task<int> GetStreamVersion();
    }
}