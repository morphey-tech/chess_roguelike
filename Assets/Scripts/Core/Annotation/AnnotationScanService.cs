using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Project.Core.Core.Logging;
using VContainer;

namespace Project.Core.Core.Annotation
{
    public sealed class AnnotationScanService
    {
        private readonly ILogger<AnnotationScanService> _logger;
        
        private readonly Dictionary<Type, List<Action<Attribute, Type>>> _handlers = new();

        [Inject]
        private AnnotationScanService(ILogService logService)
        {
            _logger = logService.CreateLogger<AnnotationScanService>();
        }
        
        public void RegisterHandler<T>(Action<T, Type> handler)
            where T : Attribute
        {
            Type attributeType = typeof(T);
            if (!_handlers.ContainsKey(attributeType)) 
            {
                _handlers[attributeType] = new List<Action<Attribute, Type>>();
            }
            List<Action<Attribute, Type>> actions = _handlers[attributeType];
            if (actions.Contains(handler)) 
            {
                _logger.Warning($"Handler already registered or type. Attribute={attributeType.Name}");
                return;
            }
            actions.Add((a, t) => handler.Invoke((T) a, t));
        }

        public void CleanHandlers()
        {
            _handlers.Clear();
        }

        public void ScanAssembly(string packageName)
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                .Single(assembly => assembly.GetName().Name == packageName);
            ScanAssembly(assembly);
        }

        private void ScanAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes()) {
                foreach (Attribute customAttribute in type.GetCustomAttributes()) {
                    if (!_handlers.ContainsKey(customAttribute.GetType())) {
                        continue;
                    }
                    _handlers[customAttribute.GetType()].ForEach(h => h.Invoke(customAttribute, type));
                }
            }
        }
    }
}