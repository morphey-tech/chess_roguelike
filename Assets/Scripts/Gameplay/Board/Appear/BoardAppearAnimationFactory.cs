using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Board.Appear.Strategies;

namespace Project.Gameplay.Gameplay.Board.Appear
{
    public sealed class BoardAppearAnimationFactory
    {
        private readonly Dictionary<string, IBoardAppearAnimationStrategy> _strategies;
        private readonly IBoardAppearAnimationStrategy _fallback;

        public BoardAppearAnimationFactory(
            IEnumerable<IBoardAppearAnimationStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(s => s.Id);
            _fallback = new BoardNoneAppearStrategy();
        }

        public IBoardAppearAnimationStrategy Get(string id)
        {
            return string.IsNullOrEmpty(id) ? _fallback : _strategies.GetValueOrDefault(id, _fallback);
        }
    }
}