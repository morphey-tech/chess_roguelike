using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Gameplay.Presentations;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Board.Appear.Strategies
{
    public class BoardRainDropAppearStrategy : IBoardAppearAnimationStrategy
    {
        public string Id => "rain_drop";

        private const float DROP_DEPTH = 0.8f;        // Основной провал
        private const float CENTER_EXTRA_DROP = 0.4f; // Центр падает сильнее
        private const float DROP_TIME = 0.28f;
        private const float LIFT_TIME = 0.18f;
        private const float MAX_LIFT_HEIGHT = 0.2f;   // Усиление для ортокамеры
        private const float RING_DELAY = 0.08f;       // Задержка по кольцам
        private const float RING_PUNCH = 0.08f;       // Рябь
        private const float RING_PUNCH_TIME = 0.14f;
        private const float WAVE_DECAY = 0.7f;        // Затухание ряби по кольцам

        public async UniTask Appear(IReadOnlyList<EntityLink>? cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return;
            }

            Vector3 center = GetCenter(cells);
            Dictionary<EntityLink, Vector3> basePos = new();

            foreach (EntityLink cell in cells)
            {
                if (cell == null) continue;
                basePos[cell] = cell.transform.position;
                cell.transform.localScale = Vector3.zero;
            }

            float maxDist = 0f;
            foreach (Vector3 pos in basePos.Values)
            {
                float d = Vector3.Distance(pos, center);
                if (d > maxDist) maxDist = d;
            }

            foreach (EntityLink cell in cells)
            {
                if (cell == null) continue;
                AnimateCell(cell, basePos[cell], center, maxDist);
            }

            const float totalTime = DROP_TIME + LIFT_TIME + RING_DELAY * 6 + RING_PUNCH_TIME * 2;
            await UniTask.Delay(System.TimeSpan.FromSeconds(totalTime));
        }

        private static void AnimateCell(EntityLink cell, Vector3 basePos, Vector3 center, float maxDist)
        {
            Transform t = cell.transform;

            float dist = Vector3.Distance(basePos, center);
            float k = maxDist > 0f ? dist / maxDist : 0f;
            float delay = Mathf.Pow(k, 0.8f) * RING_DELAY * 6f;

            float extraDrop = k < 0.01f ? CENTER_EXTRA_DROP : 0f;
            Vector3 startPos = basePos + Vector3.down * (DROP_DEPTH + extraDrop);

            t.position = startPos;

            t.DOScale(1f, DROP_TIME)
                .SetEase(Ease.OutSine)
                .SetDelay(delay);

            Sequence seq = DOTween.Sequence();
            seq.SetDelay(delay);
            seq.Append(t.DOMoveY(basePos.y - extraDrop, DROP_TIME).SetEase(Ease.InQuad));

            float lift = Mathf.Lerp(MAX_LIFT_HEIGHT, MAX_LIFT_HEIGHT * 0.5f, k);
            seq.Append(t.DOMoveY(basePos.y + lift, LIFT_TIME).SetEase(Ease.OutSine));

            if (k > 0.01f)
            {
                float ringPunch = RING_PUNCH * Mathf.Pow(WAVE_DECAY, k * 5f);
                seq.Append(t.DOPunchPosition(Vector3.up * ringPunch, RING_PUNCH_TIME, 4, 0.8f));
            }

            seq.Append(t.DOMoveY(basePos.y, 0.08f).SetEase(Ease.OutSine));
            seq.Play();
        }

        private static Vector3 GetCenter(IReadOnlyList<EntityLink> cells)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (EntityLink? c in cells)
            {
                if (c == null)
                {
                    continue;
                }
                sum += c.transform.position;
                count++;
            }
            return sum / count;
        }
    }
}
