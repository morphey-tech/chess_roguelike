using MessagePipe;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Prepare.Messages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Project.Gameplay.Gameplay.UI
{
    public class PrepareWindow : ParameterlessWindow
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private Button _button;

        private IPublisher<string, PrepareMessage> _preparePublisher = null!;
        
        [Inject]
        private void Construct(IPublisher<string, PrepareMessage> preparePublisher)
        {
            _preparePublisher = preparePublisher;
        }

        protected override void OnInit()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        protected override void OnShowed()
        {
            base.OnShowed();
        }

        protected override void OnHidden()
        {
            base.OnHidden();
        }

        private void OnButtonClick()
        {
            _preparePublisher.Publish(PrepareMessage.COMPLETE_REQUESTED, new PrepareMessage());
        }
    }
}