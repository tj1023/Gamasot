using System.Collections;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Commands
{
    public class UpgradeIngredientCommand : ICommand
    {
        private readonly RuntimeIngredient _source;
        private readonly RuntimeIngredient _target;
        private readonly float _delay;

        public UpgradeIngredientCommand(RuntimeIngredient source, RuntimeIngredient target, float delay = 0.5f)
        {
            _source = source;
            _target = target;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            if (_target is { IsAdvanced: false })
            {
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

                _target.TransformToAdvanced();
                
                float remainingDelay = hasTrail ? _delay - trailDuration : _delay;
                if (remainingDelay > 0f)
                    yield return WaitCache.Seconds(remainingDelay);
            }
        }
    }
}
