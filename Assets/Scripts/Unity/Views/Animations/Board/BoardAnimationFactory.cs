using System.Collections.Generic;
using System.Linq;
using Project.Unity.Unity.Views.Animations.Board.Strategies;
using VContainer;

namespace Project.Unity.Unity.Views.Animations.Board
{
    public sealed class BoardAnimationFactory
    {
        private readonly Dictionary<string, IBoardAnimationStrategy> _strategies;
        
        private readonly IBoardAnimationStrategy _fallback;

        [Inject]
        private BoardAnimationFactory(IEnumerable<IBoardAnimationStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(s => s.Id);
            _fallback = new NoneAnimationStrategy();
        }

        public IBoardAnimationStrategy Get(string id)
        {
            return _strategies.GetValueOrDefault(id, _fallback);
        }
    }
}