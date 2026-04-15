using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Commands
{
    public class CollectRandomScoresCommand : ICommand
    {
        private readonly GameContext _context;
        private readonly RuntimeIngredient _source;
        private readonly int _pickCount;
        private readonly float _delay;
        private readonly List<RuntimeIngredient> _validIngredients = new();

        public CollectRandomScoresCommand(GameContext context, RuntimeIngredient source, int pickCount, float delay = 0.5f)
        {
            _context = context;
            _source = source;
            _pickCount = pickCount;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            if (_source == null || _context.HarvestedIngredients == null || _context.HarvestedIngredients.Count == 0) yield break;

            _validIngredients.Clear();
            foreach (var item in _context.HarvestedIngredients)
            {
                if (item != null && item != _source)
                {
                    _validIngredients.Add(item);
                }
            }

            int loopCount = Mathf.Min(_pickCount, _validIngredients.Count);
            int totalBonus = 0;
            List<RuntimeIngredient> pickedIngredients = new List<RuntimeIngredient>();

            for (int i = 0; i < loopCount; i++)
            {
                int randomIndex = Random.Range(0, _validIngredients.Count);
                var picked = _validIngredients[randomIndex];
                pickedIngredients.Add(picked);
                totalBonus += picked.CurrentScore;
                _validIngredients.RemoveAt(randomIndex);
            }

            if (totalBonus > 0)
            {
                float trailDuration = _delay * 0.8f;
                bool hasTrail = false;

                foreach (var picked in pickedIngredients)
                {
                    EventBus<PlayScoreTrailEvent>.Publish(new PlayScoreTrailEvent
                    {
                        SourceIngredient = picked,
                        TargetType = EffectTargetType.Ingredient,
                        TargetIngredient = _source,
                        Duration = trailDuration
                    });
                    hasTrail = true;
                }

                if (hasTrail && trailDuration > 0f)
                    yield return WaitCache.Seconds(trailDuration);

                _source.CurrentScore += totalBonus;

                float remainingDelay = hasTrail ? _delay - trailDuration : _delay;
                if (remainingDelay > 0f)
                    yield return WaitCache.Seconds(remainingDelay);
            }
        }
    }
}
