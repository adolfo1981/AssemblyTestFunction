using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Services
{
 public interface IRedisProvider
    {
        ConnectionMultiplexer Connection { get; }
    }
    public class RedisProvider : IRedisProvider
    {   
        public RedisProvider(string connectionString, bool increaseTimeout = false)
        {
            if (increaseTimeout)
            {
                _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
                {
                    var options = ConfigurationOptions.Parse(connectionString);
                    options.AbortOnConnectFail = false;
                    options.ConnectRetry = 3;
                    options.ConnectTimeout = 1000;
                    options.SyncTimeout = 1000;
                    var connection = ConnectionMultiplexer.Connect(connectionString);
                    connection.PreserveAsyncOrder = false;
                    ThreadPool.SetMinThreads(300, 300);
                    ThreadPool.SetMaxThreads(500, 500);
                    return connection;
                });
            }
            else
            {
                _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
                {
                    var connection = ConnectionMultiplexer.Connect(connectionString);
                    connection.PreserveAsyncOrder = false;
                    return connection;
                });
                        
                        
            }
        }

        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

        public ConnectionMultiplexer Connection => _lazyConnection.Value;
    }
}
