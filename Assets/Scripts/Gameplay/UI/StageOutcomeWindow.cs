using System;
using Cysharp.Threading.Tasks;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Flow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Gameplay.UI
{
    public class StageOutcomeWindow : ParameterWindow<StageOutcomeWindow.Model>
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _bodyText;
        [SerializeField] private HorizontalLayoutGroup _buttonsLayout;
        [SerializeField] private Button _buttonPrefab;
        [SerializeField] private GameObject _buttonContainer;

        private UniTaskCompletionSource<StageFlowAction> _completionSource;

        public class Model
        {
            public string Title;
            public string Body;
            public (string label, StageFlowAction action)[] Actions;
        }

        protected override void OnInit()
        {
            base.OnInit();
            OnHide += HandleHide;
        }

        public UniTask<StageFlowAction> ShowAsync(Model model)
        {
            _completionSource = new UniTaskCompletionSource<StageFlowAction>();
            Show(model);
            return _completionSource.Task;
        }

        private void HandleHide(Window wnd)
        {
            _completionSource?.TrySetResult(StageFlowAction.RestartStage);
            _completionSource = null;
        }

        protected override void OnShow(Model model)
        {
            _titleText.text = model.Title;
            _bodyText.text = model.Body;

            // Скрываем префаб кнопки
            _buttonPrefab.gameObject.SetActive(false);

            // Удаляем старые кнопки (кроме префаба)
            for (int i = _buttonContainer.transform.childCount - 1; i >= 0; i--)
            {
                var child = _buttonContainer.transform.GetChild(i);
                if (child.gameObject != _buttonPrefab.gameObject)
                    UnityEngine.Object.Destroy(child.gameObject);
            }

            bool resolved = false;
            foreach (var (label, action) in model.Actions)
            {
                Button button = UnityEngine.Object.Instantiate(_buttonPrefab, _buttonContainer.transform);
                button.gameObject.SetActive(true);
                button.name = label.Replace(" ", "_");

                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = label;

                int capturedAction = (int)action;
                button.onClick.AddListener(() =>
                {
                    if (resolved)
                        return;
                    resolved = true;
                    _completionSource?.TrySetResult((StageFlowAction)capturedAction);
                    Hide();
                });
            }
        }
    }
}
