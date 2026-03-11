using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.UI;
using Project.Unity.UI.Components;
using UnityEngine;
using VContainer;

namespace Project.Unity.Presentations
{
    public sealed class DamageTextPresenter : MonoBehaviour
    {
        [SerializeField] private DamageText _template;
        [SerializeField] private Transform _pivot;

        private static WorldUIWindow? _cachedWorldUi;
        private static IUIService? _uiService;

        [Inject]
        private void Construct(IUIService uiService)
        {
            _uiService = uiService;
        }

        public async UniTask ShowFor(DamageVisualContext ctx)
        {
            if (_cachedWorldUi == null)
            {
                await _uiService!.Initialized;
                _cachedWorldUi = await _uiService.GetOrCreateAsync<WorldUIWindow>();
            }

            DamageText? damageText = _cachedWorldUi.Add(_template, _pivot);
            damageText?.Play(ctx);
        }

    }
}
