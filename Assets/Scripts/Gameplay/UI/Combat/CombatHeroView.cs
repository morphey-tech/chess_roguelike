using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Project.Gameplay.Gameplay.UI.Combat
{
    /// <summary>
    /// View, отвечающая за отображение иконки героя в бою.
    /// Загружает спрайт из AssetService по фиксированному пути.
    /// </summary>
    public class CombatHeroView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _icon;

        private IAssetService _assetService;

        [Inject]
        private void Construct(IAssetService assetService)
        {
            _assetService = assetService;
        }

        private async void Start()
        {
            await Initialize();
        }

        private async UniTask Initialize()
        {
            /*try
            {
                _icon.sprite = await _assetService.LoadAsync<Sprite>(_heroIconPath)
                               ?? throw new InvalidOperationException($"Failed to load hero icon from {_heroIconPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load hero icon: {ex.Message}");
            }*/
        }
    }
}
