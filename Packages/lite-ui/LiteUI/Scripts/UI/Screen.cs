using System.Collections.Generic;
using System.Linq;
using LiteUI.Common.Logger;
using Cysharp.Threading.Tasks;
using LiteUI.World;
using UnityEngine;

namespace LiteUI.UI
{
    public class Screen<T> : MonoBehaviour
            where T : System.Enum
    {
        private readonly IUILogger _logger = LoggerFactory.GetLogger<Screen<T>>();

        private readonly Dictionary<T, List<IActivatable>> _panels = new();
        protected T CurrentView { get; private set; } = default!;
        private bool _hasView;
        
        private bool _switching;

        public void RegisterPanel(T viewType, IActivatable panel)
        {
            panel.Deactivate().Forget();
            if (!_panels.ContainsKey(viewType)) {
                _panels[viewType] = new List<IActivatable>();
            }
            if (_panels[viewType].Contains(panel)) {
                _logger.Warn($"Panel already registered panel={panel.GetType().Name} viewType={viewType}");
                return;
            }
            _panels[viewType].Add(panel);
        }

        public void UnregisterPanel(T viewType, IActivatable panel)
        {
            if (!_panels.ContainsKey(viewType)) {
                _logger.Warn($"Can't unregister panel={panel.GetType().Name} viewType={viewType}");
                return;
            }
            List<IActivatable> activatables = _panels[viewType];
            if (!activatables.Contains(panel)) {
                _logger.Warn($"Can't unregister panel={panel.GetType().Name} viewType={viewType}");
                return;
            }
            activatables.Remove(panel);
            if (activatables.Count == 0) {
                _panels.Remove(viewType);
            }
        }

        public async UniTask SwitchMode(T viewType)
        {
            if (_switching) {
                await UniTask.WaitWhile(() => _switching);
            }
            if (_hasView && CurrentView.Equals(viewType)) {
                return;
            }
            _switching = true;
            List<IActivatable>? deactivatablePanels = (_hasView && _panels.ContainsKey(CurrentView)) ? _panels[CurrentView] : null;
            List<IActivatable>? activatablePanels = _panels.ContainsKey(viewType) ? _panels[viewType] : null;
            if (deactivatablePanels != null) {
                List<UniTask> deactivateTasks = deactivatablePanels.Where(p => activatablePanels == null || !activatablePanels.Contains(p))
                                                                   .Select(p => p.Deactivate())
                                                                   .ToList();
                await UniTask.WhenAll(deactivateTasks);
            }
            _hasView = true;
            CurrentView = viewType;
            if (activatablePanels != null) {
                List<UniTask> activateTasks = activatablePanels.Where(p => deactivatablePanels == null || !deactivatablePanels.Contains(p))
                                                               .Select(p => p.Activate())
                                                               .ToList();
                await UniTask.WhenAll(activateTasks);
            }
            _switching = false;
        }

        protected bool Switching => _switching;
    }
}
