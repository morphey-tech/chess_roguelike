using Project.Core.Core.Configs.Artifacts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Single artifact item view in the list.
    /// </summary>
    public class ArtifactItemView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _triggerText;
        [SerializeField] private Image _rarityBG;

        [Header("Rarity Colors")]
        [SerializeField] private Color _commonColor = new(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private Color _rareColor = new(0.2f, 0.6f, 1f, 0.3f);
        [SerializeField] private Color _legendaryColor = new(1f, 0.8f, 0.2f, 0.3f);

        public void Initialize(ArtifactConfig config)
        {
            _nameText.text = config.Name;
            _descriptionText.text = config.Description;
            _triggerText.text = GetTriggerDisplayName(config.ParseTrigger());

            // Set rarity color
            Color rarityColor = config.ParseRarity() switch
            {
                ArtifactRarity.Rare => _rareColor,
                ArtifactRarity.Legendary => _legendaryColor,
                _ => _commonColor
            };

            if (_rarityBG != null)
            {
                _rarityBG.color = rarityColor;
            }

            // Load icon (placeholder - would need sprite atlas or addressables)
            // _icon.sprite = Resources.Load<Sprite>($"Icons/Artifacts/{config.Icon}");
        }

        private static string GetTriggerDisplayName(ArtifactTrigger trigger)
        {
            return trigger switch
            {
                ArtifactTrigger.Passive => "⚡ Passive",
                ArtifactTrigger.OnBattleStart => "⚔️ Battle Start",
                ArtifactTrigger.OnBattleEnd => "🏆 Battle End",
                ArtifactTrigger.OnUnitDeath => "💀 Unit Death",
                ArtifactTrigger.OnUnitKill => "⚔️ On Kill",
                ArtifactTrigger.OnAllyDeath => "💔 Ally Death",
                ArtifactTrigger.OnDamageReceived => "🛡️ When Hit",
                ArtifactTrigger.OnReward => "🎁 On Reward",
                _ => trigger.ToString()
            };
        }
    }
}