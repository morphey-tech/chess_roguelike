using System.Threading;
using Cysharp.Threading.Tasks;
using LiteUI.Common.Attributes;
using LiteUI.UI.Balloon.Controller;
using LiteUI.UI.Balloon.Model;
using LiteUI.UI.Model;
using LiteUI.UI.Service;
using UnityEngine;

namespace LiteUI.UI.Balloon.Service
{
    [Injectable]
    public class BalloonManager
    {
        private static readonly Vector2 DEFAULT_OFFSET = new(0, 50);

        private readonly UIService _uiService;

        public BalloonManager(UIService uiService)
        {
            _uiService = uiService;
        }

        public async UniTask<T> Create<T>(GameObject target, CancellationToken cancellationToken = default, params object?[]? initParameters)
                where T : BaseBalloon
        {
            BalloonViewModel? balloonViewModel = target.GetComponentInChildren<BalloonViewModel>(true);
            Transform container = balloonViewModel != null ? balloonViewModel.transform : target.transform;
            Vector2 offset = balloonViewModel != null ? balloonViewModel.Offset : DEFAULT_OFFSET;
            T balloon = await _uiService.CreateAsync<T>(UIModel.Create<T>(initParameters).Container(container), cancellationToken);
            balloon.Offset = offset;
            return balloon;
        }

        public void Release(BaseBalloon balloon)
        {
            _uiService.Release(balloon.gameObject);
        }
    }
}
