using System.Collections;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Commands
{
    public class TallyRoundScoreCommand : ICommand
    {
        private readonly GameContext _context;
        private readonly float _delay;

        public TallyRoundScoreCommand(GameContext context, float delay = 0.2f)
        {
            _context = context;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            float trailDuration = _delay; 

            foreach (var item in _context.HarvestedIngredients)
            {
                if (item is { CurrentScore: > 0 })
                {
                    // Trail 발사 (목표는 TotalScore)
                    EventBus<PlayScoreTrailEvent>.Publish(new PlayScoreTrailEvent
                    {
                        SourceIngredient = item,
                        TargetType = EffectTargetType.TotalScore, 
                        Duration = trailDuration
                    });

                    // 꼬리가 목표에 도달할 때까지 대기
                    yield return WaitCache.Seconds(trailDuration);

                    // 점수는 RoundScore에 누계
                    _context.RoundScore += item.CurrentScore;
                    EventBus<RoundScoreUpdatedEvent>.Publish(new RoundScoreUpdatedEvent { RoundScore = _context.RoundScore });
                }
            }

            // 모든 누계가 끝난 후 잠깐 대기
            yield return WaitCache.Seconds(0.2f);
        }
    }
}
