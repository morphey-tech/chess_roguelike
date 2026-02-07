using UnityEngine;

namespace Project.Core.Window
{
    public class WindowAnimationSettings : MonoBehaviour
    {
        private enum AnimationType
        {
            Top = 0,
            Bottom = 1,
            Left = 2,
            Right = 3
        }

        [SerializeField] private AnimationType openAnimation;
        [SerializeField] private AnimationType closeAnimation;

        public int OpenAnimation => (int)openAnimation;
        public int CloseAnimation => (int)closeAnimation;
    }
}