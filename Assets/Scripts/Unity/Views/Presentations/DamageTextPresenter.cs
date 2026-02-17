using Cysharp.Threading.Tasks.Triggers;
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
        
        public void ShowFor(DamageVisualContext ctx)
        {
            var damageText = Gameplay.Gameplay.UI.UIService.GetOrCreate<WorldUIWindow>().Add(_template, _pivot);
            damageText.Play(ctx);
        }

    }
}
