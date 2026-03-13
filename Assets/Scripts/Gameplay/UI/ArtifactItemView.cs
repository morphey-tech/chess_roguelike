using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Artifacts;
using Project.Gameplay.Gameplay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Single artifact item view in the list.
    /// </summary>
    public class ArtifactItemView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _icon;
        [SerializeField] private Image _rarityBG;
        [SerializeField] private TextMeshProUGUI _stacks;
        
        [Header("Rarity Colors")]
        [SerializeField] private Color _commonColor = new(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private Color _rareColor = new(0.2f, 0.6f, 1f, 0.3f);
        [SerializeField] private Color _legendaryColor = new(1f, 0.8f, 0.2f, 0.3f);

        private IAssetService _assetService;

        [Inject]
        private void Construct(IAssetService assetService)
        {
            _assetService = assetService;
        }

        public async UniTask Initialize(ArtifactConfig config, int stacks = 0)
        {
            Color rarityColor = config.ParseRarity() switch
            {
                ArtifactRarity.Rare => _rareColor,
                ArtifactRarity.Legendary => _legendaryColor,
                _ => _commonColor
            };

            _rarityBG.color = rarityColor;
            _stacks.text = stacks <= 0 ? string.Empty : stacks.ToString();
            //Message to exception and maybe ct?
            _icon.sprite = await _assetService.LoadAsync<Sprite>(config.Icon) 
                           ?? throw new InvalidOperationException();
        }
    }
}
