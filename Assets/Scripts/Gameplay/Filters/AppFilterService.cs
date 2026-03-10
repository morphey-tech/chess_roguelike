using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Filters;
using Project.Core.Core.Filters.Messages;
using Project.Core.Core.Logging;
using VContainer;

namespace Project.Gameplay.Gameplay.Filters
{
    public sealed class AppFilterService : IAppFilterService
    {
        private readonly IObjectResolver _resolver;
        private readonly IPublisher<string, AppFilterMessage> _publisher;
        private readonly ILogger<AppFilterService> _logger;
        
        private readonly List<Type> _filters = new();
        
        [Inject]
        private AppFilterService(IObjectResolver resolver,
            IPublisher<string, AppFilterMessage> publisher,
            ILogService logService)
        {
            _resolver = resolver;
            _publisher = publisher;
            _logger = logService.CreateLogger<AppFilterService>();
        }

        void IAppFilterService.AddFilter<T>()
        {
            if (_filters.Contains(typeof(T)))
            {
                _logger.Warning($"Application filter already added.");
                return;
            }
            _filters.Add(typeof(T));
        }

        async UniTask IAppFilterService.RunAsync()
        {
            try {
                foreach (Type filterType in _filters) {
                    try {
                        IApplicationFilter filter = (IApplicationFilter) _resolver.Resolve(filterType);
                        _logger.Debug($"Start filter {filterType.Name}");
                        _publisher.Publish(AppFilterMessage.STARTED, new AppFilterMessage(filterType));
                        await filter.RunAsync();
                        _publisher.Publish(AppFilterMessage.COMPLETED, new AppFilterMessage(filterType));
                    } catch (OperationCanceledException e) {
                        _logger.Error($"Filter cancelled {filterType.Name}", e);
                        throw;
                    } catch (Exception e) {
                        _logger.Error($"Error while run filter {filterType.Name}", e);
                        _logger.Error($"Error while run filter {filterType.Name}", e);
                        throw;
                    }
                }
            } finally {
                _filters.Clear();
            }
        }
    }
}