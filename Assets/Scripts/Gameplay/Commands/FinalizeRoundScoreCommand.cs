using System.Collections;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Commands
{
    public class FinalizeRoundScoreCommand : ICommand
    {
        private readonly GameContext _context;
        private readonly float _delay;

        public FinalizeRoundScoreCommand(GameContext context, float delay = 0.2f)
        {
            _context = context;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            if (_context.RoundScore > 0)
            {
                _context.TotalScore += _context.RoundScore;
                _context.RoundScore = 0;

                EventBus<TotalScoreUpdatedEvent>.Publish(new TotalScoreUpdatedEvent { TotalScore = _context.TotalScore });
                EventBus<RoundScoreUpdatedEvent>.Publish(new RoundScoreUpdatedEvent { RoundScore = _context.RoundScore });

                if (_delay > 0f)
                    yield return WaitCache.Seconds(_delay);
            }
        }
    }
}
