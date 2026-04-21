using System.Collections;
using System.Collections.Generic;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Commands
{
    public class AddScoreToGroupCommand : ICommand
    {
        private readonly RuntimeIngredient _source;
        private readonly List<RuntimeIngredient> _targets;
        private readonly int _scoreDelta;
        private readonly float _delay;

        public AddScoreToGroupCommand(RuntimeIngredient source, List<RuntimeIngredient> targets, int scoreDelta, float delay = 0.5f)
        {
            _source = source;
            _targets = targets;
            _scoreDelta = scoreDelta;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            if (_targets == null || _targets.Count == 0 || _scoreDelta == 0) yield break;

            float trailDuration = _delay * 0.8f;
            bool hasTrail = false;
            int fixedDirection = 1; // 1: Source 기준 좌/우 대칭 곡선

            foreach (var target in _targets)
            {
                if (target == null) continue;
                bool targetHasTrail = _source != null && _source != target;
                if (targetHasTrail)
                {
                    EventBus<PlayScoreTrailEvent>.Publish(new PlayScoreTrailEvent
                    {
                        SourceIngredient = _source,
                        TargetType = EffectTargetType.Ingredient,
                        TargetIngredient = target,
                        Duration = trailDuration,
                        FixedDirection = fixedDirection
                    });
                    hasTrail = true;
                }
            }

            if (hasTrail && trailDuration > 0f)
                yield return WaitCache.Seconds(trailDuration);

            foreach (var target in _targets)
            {
                if (target == null) continue;
                target.CurrentScore += _scoreDelta;
            }

            float remainingDelay = hasTrail ? _delay - trailDuration : _delay;
            if (remainingDelay > 0f)
                yield return WaitCache.Seconds(remainingDelay);
        }
    }
}
