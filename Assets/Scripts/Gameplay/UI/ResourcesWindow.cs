using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using VContainer;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Economy;

namespace Project.Gameplay.UI
{
    /// <summary>
    /// Window that displays current currency amounts:
    /// - Crowns (run resource)
    /// - Scrolls (meta resource)
    /// Updates reactively via UniRx subscriptions.
    /// </summary>
    public class ResourcesWindow : ParameterlessWindow
    {
        [SerializeField] private Image _crownsIcon;
        [SerializeField] private TextMeshProUGUI _crownsText;
        [SerializeField] private Image _scrollsIcon;
        [SerializeField] private TextMeshProUGUI _scrollsText;

        protected override bool HideOtherWindows => false;
        protected override bool IgnoreHideOthersWindows => true;
        public override bool NeedShowBackground => false;
        public override int ZOrder => 100;
        
        private EconomyService _economyService;
        private CompositeDisposable _disposables;

        [Inject]
        private void Construct(EconomyService economyService)
        {
            _economyService = economyService;
        }
        
        protected override void OnInit()
        {
            _disposables = new CompositeDisposable();

            _economyService.GetCrownsProperty()
                .Subscribe(value => _crownsText.text = value.ToString())
                .AddTo(_disposables);

            _economyService.GetScrollsProperty()
                .Subscribe(value => _scrollsText.text = value.ToString())
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
