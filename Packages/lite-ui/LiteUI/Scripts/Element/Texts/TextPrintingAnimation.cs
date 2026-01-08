using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LiteUI.Element.Texts
{
    public class TextPrintingAnimation : MonoBehaviour
    {
        [SerializeField]
        private StepType _stepType = StepType.CHAR;
        [SerializeField]
        private float _timePerStep = 0.025f;
        [SerializeField]
        private UILabel _label = null!;

        private int _totalSteps;

        public void Clear()
        {
            _label.Text = "";
            _totalSteps = 0;
            CurrentStep = 0;
        }

        public void CompleteMessage()
        {
            CurrentStep = _totalSteps;
        }

        public async UniTask Show(string text)
        {
            _label.Text = text;
            _totalSteps = GetTotalSteps(text);
            CurrentStep = 0;

            float startTime = Time.time;

            while (CurrentStep < _totalSteps) {
                int stepsLeft = Mathf.RoundToInt((Time.time - startTime) / _timePerStep);
                if (CurrentStep != stepsLeft) {
                    CurrentStep = stepsLeft;
                }

                await UniTask.Yield(destroyCancellationToken);
            }
        }

        private int GetTotalSteps(string message)
        {
            switch (_stepType) {
                case StepType.CHAR:
                    return message.Length;
                case StepType.WORD:
                    return message.Split(' ').Length;
                default:
                    throw new ArgumentException("Unknown StepType");
            }
        }

        private int CurrentStep
        {
            get
            {
                switch (_stepType) {
                    case StepType.CHAR:
                        return _label.maxVisibleCharacters;
                    case StepType.WORD:
                        return _label.maxVisibleWords;
                    default:
                        throw new ArgumentException("Unknown StepType");
                }
            }
            set
            {
                switch (_stepType) {
                    case StepType.CHAR:
                        _label.maxVisibleCharacters = value;
                        break;
                    case StepType.WORD:
                        _label.maxVisibleWords = value;
                        break;
                    default:
                        throw new ArgumentException("Unknown StepType");
                }
            }
        }

        private enum StepType
        {
            CHAR,
            WORD
        }
    }
}
