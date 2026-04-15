using System.Collections;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Commands
{
    public class AddScoreCommand : ICommand
    {
        private readonly RuntimeIngredient _source;
        private readonly RuntimeIngredient _target;
        private readonly int _scoreDelta;
        private readonly float _delay;

        public AddScoreCommand(RuntimeIngredient source, RuntimeIngredient target, int scoreDelta, float delay = 0.5f)
        {
            _source = source;
            _target = target;
            _scoreDelta = scoreDelta;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            if (_target == null || _scoreDelta == 0) yield break;
            bool hasTrail = _source != null && _source != _target;
            float trailDuration = _delay * 0.8f;

            if (hasTrail)
            {
                EventBus<PlayScoreTrailEvent>.Publish(new PlayScoreTrailEvent
                {
                    SourceIngredient = _source,
                    TargetType = EffectTargetType.Ingredient,
                    TargetIngredient = _target,
                    Duration = trailDuration
                });

                if (trailDuration > 0f)
                    yield return WaitCache.Seconds(trailDuration);
            }

            _target.CurrentScore += _scoreDelta;
                
            float remainingDelay = hasTrail ? _delay - trailDuration : _delay;
            if (remainingDelay > 0f)
                yield return WaitCache.Seconds(remainingDelay);
        }
    }
}
