using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Unity.Unity.Views.Animations.Board.Strategies
{
    public class NoneAnimationStrategy : IBoardAnimationStrategy
    {
        public string Id => "none";
        
        public UniTask Play(BoardAnimationTarget target)
        {
            foreach (Transform? tfm in target.Targets)
            {
                if (tfm != null)
                    tfm.transform.localScale = Vector3.zero;
            }
            foreach (Transform? tfm in target.Targets)
            {
                if (tfm != null)
                    tfm.transform.localScale = Vector3.one;
            }
            return UniTask.CompletedTask;
        }
    }
}