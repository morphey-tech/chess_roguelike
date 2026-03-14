using Cysharp.Threading.Tasks;
using Project.Core.Window;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.Gameplay.UI.Combat
{
    public sealed class CombatHUDWindow : ParameterlessWindow
    {
        [Header("Views")]
        [SerializeField] private CombatHeroView _heroView;
        [SerializeField] private CombatCurrencyView _currencyView;
        [SerializeField] private CombatTurnView _turnView;
        [SerializeField] private CombatExtraView _extraView;

        private IObjectResolver _resolver;

        [Inject]
        private void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        protected override void OnInit()
        {
            base.OnInit();
            MonoBehaviour[]? views = GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour view in views)
            {
                if (view is CombatHeroView or CombatCurrencyView or CombatTurnView or CombatExtraView)
                {
                    _resolver.Inject(view);
                }
            }
            _currencyView.Initialize().Forget();
            _turnView.Initialize();
        }
    }
}
