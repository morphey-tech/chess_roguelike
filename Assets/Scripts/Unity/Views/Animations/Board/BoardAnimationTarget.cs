using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Unity.Unity.Views.Animations.Board
{
    public class BoardAnimationTarget
    {
        public IReadOnlyList<Transform> Targets { get; }

        public bool IsGroup => Targets.Count > 1;

        public BoardAnimationTarget(IEnumerable<Transform> targets)
        {
            Targets = targets.ToList();
        }

        public static BoardAnimationTarget Single(Transform t)
        {
            return new BoardAnimationTarget(new[] { t });
        }

        public static BoardAnimationTarget Group(IEnumerable<Transform> t)
        {
            return new BoardAnimationTarget(t);
        }
    }
}