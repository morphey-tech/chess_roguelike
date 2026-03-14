using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Gameplay.Gameplay.Economy;
using Project.Gameplay.Gameplay.UI.Currency;
using TMPro;
using UnityEngine;
using UniRx;
using VContainer;

namespace Project.Gameplay.Gameplay.UI.Combat
{
    /// <summary>
    /// View, отвечающая за отображение валюты (крон) в бою.
    /// Реактивно обновляется через UniRx подписку на EconomyService.
    /// </summary>
    public class CombatCurrencyView : MonoBehaviour
    {
        private IAssetService _assetService = null!;
        private EconomyService _economyService = null!;
        
        [Inject]
        private void Construct(IAssetService assetService, EconomyService economyService)
        {
            _assetService = assetService;
            _economyService = economyService;
        }

        public async UniTask Initialize()
        {
            GameObject prefab = await _assetService.LoadAsync<GameObject>("CurrencyItemView");
            CurrencyItemView crownInstance = _assetService.Instantiate<CurrencyItemView>(prefab.gameObject, Vector3.zero, Quaternion.identity);
            CurrencyItemView scrollInstance = _assetService.Instantiate<CurrencyItemView>(prefab.gameObject, Vector3.zero, Quaternion.identity);
            crownInstance.transform.SetParent(transform);
            scrollInstance.transform.SetParent(transform);
            await crownInstance.Initialize(ResourceIds.Crowns);
            await scrollInstance.Initialize(ResourceIds.Scrolls);
        }
    }
}
