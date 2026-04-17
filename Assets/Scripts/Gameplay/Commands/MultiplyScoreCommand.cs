using System.Collections;
using Core;
using Data;
using Interfaces;
using UnityEngine;

namespace Gameplay.Commands
{
    public class MultiplyScoreCommand : ICommand
    {
        private readonly RuntimeIngredient _source;
        private readonly RuntimeIngredient _target;
        private readonly float _multiplier;
        private readonly float _delay;

        public MultiplyScoreCommand(RuntimeIngredient source, RuntimeIngredient target, float multiplier, float delay = 0.5f)
        {
            _source = source;
            _target = target;
            _multiplier = multiplier;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            if (_target == null) yield break;

            int scoreDelta = Mathf.RoundToInt(_target.CurrentScore * _multiplier) - _target.CurrentScore;
            if (scoreDelta == 0) yield break;

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

            _target.CurrentScore += scoreDelta;
                
            float remainingDelay = hasTrail ? _delay - trailDuration : _delay;
            if (remainingDelay > 0f)
                yield return WaitCache.Seconds(remainingDelay);
        }
    }
}