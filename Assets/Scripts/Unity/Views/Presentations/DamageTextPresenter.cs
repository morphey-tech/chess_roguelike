using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.UI;
using Project.Unity.UI.Components;
using UnityEngine;

namespace Project.Unity.Presentations
{
    public sealed class DamageTextPresenter : MonoBehaviour
    {
        [SerializeField] private DamageText _template;
        [SerializeField] private Transform _pivot;

        private static WorldUIWindow? _cachedWorldUi;

        public async UniTask ShowFor(DamageVisualContext ctx)
        {
            if (_cachedWorldUi == null)
            {
                await UIService.Initialized;
                _cachedWorldUi = await UIService.GetOrCreateAsync<WorldUIWindow>();
            }
                
            DamageText? damageText = _cachedWorldUi.Add(_template, _pivot);
            damageText?.Play(ctx);
        }

    }
}
