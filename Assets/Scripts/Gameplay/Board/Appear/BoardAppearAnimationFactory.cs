using System.Collections.Generic;
using System.Linq;

namespace Project.Gameplay.Gameplay.Board.Appear
{
    public class BoardAppearAnimationFactory
    {
        private readonly Dictionary<string, IBoardAppearAnimationStrategy> _strategies;

        public BoardAppearAnimationFactory(
            IEnumerable<IBoardAppearAnimationStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(s => s.Id);
        }

        public IBoardAppearAnimationStrategy Get(string id)
        {
            return _strategies.TryGetValue(id, out IBoardAppearAnimationStrategy strategy) 
                ? strategy : _strategies["none"];
        }
    }
}