using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Flow;
using Project.Gameplay.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Unity.UI
{
    public sealed class GameUiService : IGameUiService
    {
        private readonly Font _defaultFont;
        private readonly ILogger<GameUiService> _logger;

        public GameUiService(ILogService logService)
        {
            _logger = logService.CreateLogger<GameUiService>();
            _defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        public async UniTask ShowWorldUiAsync()
        {
            await Gameplay.Gameplay.UI.UIService.ShowAsync<WorldUIWindow>();
        }

        public async UniTask ShowPreparePhaseAsync()
        {
            TurnWindow? wnd = await Gameplay.Gameplay.UI.UIService.ShowAsync<TurnWindow>();
            wnd?.SetPreparePhase();
        }

        public void SetGamePhase()
        {
            if (!Gameplay.Gameplay.UI.UIService.IsValid)
            {
                return;
            }

            Gameplay.Gameplay.UI.UIService.GetOrCreate<TurnWindow>().SetGamePhase();
        }

        public UniTask HideCombatUiAsync()
        {
            if (Gameplay.Gameplay.UI.UIService.IsValid)
                Gameplay.Gameplay.UI.UIService.Hide<TurnWindow>();
            return UniTask.CompletedTask;
        }

        public UniTask<StageFlowAction> ShowVictoryScreenAsync(StageResult result)
        {
            _logger.Info($"Victory! turns={result.TurnCount}, kills={result.EnemiesKilled}");
            return ShowOutcomeScreenAsync(
                "Victory",
                $"Turns: {result.TurnCount}\nKills: {result.EnemiesKilled}",
                ("Next stage", StageFlowAction.NextStage),
                ("Restart stage", StageFlowAction.RestartStage),
                ("Back to hub", StageFlowAction.GoHub));
        }

        public UniTask<StageFlowAction> ShowDefeatScreenAsync(StageResult result)
        {
            _logger.Info($"Defeat! turns={result.TurnCount}, kills={result.EnemiesKilled}");
            return ShowOutcomeScreenAsync(
                "Defeat",
                $"Turns: {result.TurnCount}\nKills: {result.EnemiesKilled}",
                ("Restart stage", StageFlowAction.RestartStage),
                ("Back to hub", StageFlowAction.GoHub));
        }

        private UniTask<StageFlowAction> ShowOutcomeScreenAsync(
            string title,
            string body,
            params (string label, StageFlowAction action)[] actions)
        {
            if (!Gameplay.Gameplay.UI.UIService.IsValid)
                return UniTask.FromResult(StageFlowAction.RestartStage);

            Canvas canvas = Gameplay.Gameplay.UI.UIService.Canvas;
            if (canvas == null)
                return UniTask.FromResult(StageFlowAction.RestartStage);

            GameObject overlay = CreateUiObject("StageOutcomeOverlay", canvas.transform);
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            Stretch(overlayRect);
            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.72f);

            GameObject panel = CreateUiObject("Panel", overlay.transform);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520f, 330f);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
            VerticalLayoutGroup panelLayout = panel.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(24, 24, 24, 24);
            panelLayout.spacing = 14f;
            panelLayout.childControlHeight = true;
            panelLayout.childControlWidth = true;
            panelLayout.childForceExpandHeight = false;
            panelLayout.childForceExpandWidth = true;
            ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateText("Title", panel.transform, title, 36, FontStyle.Bold, TextAnchor.MiddleCenter);
            CreateText("Body", panel.transform, body, 22, FontStyle.Normal, TextAnchor.MiddleCenter);

            GameObject buttons = CreateUiObject("Buttons", panel.transform);
            VerticalLayoutGroup buttonsLayout = buttons.AddComponent<VerticalLayoutGroup>();
            buttonsLayout.spacing = 10f;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childForceExpandHeight = false;
            buttonsLayout.childForceExpandWidth = true;

            var tcs = new UniTaskCompletionSource<StageFlowAction>();
            bool resolved = false;
            foreach ((string label, StageFlowAction action) in actions)
            {
                Button button = CreateButton(buttons.transform, label);
                button.onClick.AddListener(() =>
                {
                    if (resolved)
                        return;
                    resolved = true;
                    if (overlay != null)
                        Object.Destroy(overlay);
                    tcs.TrySetResult(action);
                });
            }

            return tcs.Task;
        }

        private Text CreateText(
            string name,
            Transform parent,
            string value,
            int size,
            FontStyle style,
            TextAnchor alignment)
        {
            GameObject go = CreateUiObject(name, parent);
            Text text = go.AddComponent<Text>();
            text.font = _defaultFont;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = size > 30 ? 52f : 78f;
            return text;
        }

        private Button CreateButton(Transform parent, string label)
        {
            GameObject go = CreateUiObject(label.Replace(" ", "_"), parent);
            Image image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.25f, 0.33f, 1f);
            Button button = go.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.27f, 0.33f, 0.44f, 1f);
            colors.pressedColor = new Color(0.15f, 0.2f, 0.3f, 1f);
            button.colors = colors;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 52f);
            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = 52f;

            Text text = CreateText("Label", go.transform, label, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
            text.resizeTextForBestFit = false;
            RectTransform textRect = text.GetComponent<RectTransform>();
            Stretch(textRect);
            return button;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject go = new(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
