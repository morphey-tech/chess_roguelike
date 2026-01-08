using Cysharp.Threading.Tasks;
using LiteUI.Binding.Attributes;
using LiteUI.Element.Texts;
using LiteUI.Popup.Component;
using LiteUI.Popup.Model;
using LiteUI.UI.Service;
using UnityEngine;
using VContainer;

namespace LiteUI.Popup.Panel
{
    [UIController("TitleDescriptionArrowPopup")]
    public class TitleDescriptionArrowPopup : MonoBehaviour, IPopup
    {
        [UIComponentBinding]
        private ArrowPopupPanel _arrowPopupPanel = null!;
        [UIComponentBinding("Title")]
        private UILabel _title = null!;
        [UIComponentBinding("Description")]
        private UILabel _description = null!;

        private ScreenLayout _screenLayout = null!;

        [Inject]
        public void Construct(ScreenLayout screenLayout)
        {
            _screenLayout = screenLayout;
        }

        [UICreated]
        private void OnUICreated(string title, string description)
        {
            _arrowPopupPanel.Init(_screenLayout);

            Title = title;
            Description = description;
        }

        async UniTask IPopup.Show(RectTransform target, PopupAlign defaultAlign, Vector2 offset)
        {
            await _arrowPopupPanel.Show(target, defaultAlign, offset);
        }

        async UniTask IPopup.Hide()
        {
            await _arrowPopupPanel.Hide();
        }

        private string Title
        {
            set => _title.Text = value;
        }

        private string Description
        {
            set => _description.Text = value;
        }
    }
}
