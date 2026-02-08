using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Project.Core.Core.Infrastructure;

namespace Project.Core.Core.Configs
{
    public abstract class ConfigProviderBase
    {
        public JsonSerializerSettings JsonSerializerSettings { get; } = new()
        {
            ContractResolver = new DefaultContractResolver(),
            TypeNameHandling = TypeNameHandling.None,
            Converters =
            {
                new Vector2IntConverter(),
                new Vec2ObservableDictionaryConverter(),
                new StringEnumConverter()
            }
        };

        public JsonSerializerSettings JsonDeserializerSettings { get; } = new()
        {
            TypeNameHandling = TypeNameHandling.None,
            Converters =
            {
                new Vector2IntConverter(),
                new Vec2ObservableDictionaryConverter(),
                new StringEnumConverter()
            }
        };

        public async UniTask<T> Download<T>(string key, T storage, CancellationToken cancellationToken)
        {
            T result;
            try
            {
                result = await InnerDownload(key, storage, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cannot download config", ex);
            }

            return result;
        }

        protected abstract UniTask<T> InnerDownload<T>(string key, T storage, CancellationToken cancellationToken);
    }
}
