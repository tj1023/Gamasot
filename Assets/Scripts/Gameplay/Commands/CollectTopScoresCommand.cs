using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Commands
{
    public class CollectTopScoresCommand : ICommand
    {
        private readonly GameContext _context;
        private readonly RuntimeIngredient _source;
        private readonly int _topCount;
        private readonly float _delay;
        private readonly List<RuntimeIngredient> _validIngredients = new();

        public CollectTopScoresCommand(GameContext context, RuntimeIngredient source, int topCount, float delay = 0.5f)
        {
            _context = context;
            _source = source;
            _topCount = topCount;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            if (_source == null || _context.HarvestedIngredients == null || _context.HarvestedIngredients.Count == 0) yield break;

            _validIngredients.Clear();
            foreach (var item in _context.HarvestedIngredients)
            {
                if (item != null)
                {
                    _validIngredients.Add(item);
                }
            }

            // 점수 내림차순 정렬
            _validIngredients.Sort((a, b) => b.CurrentScore.CompareTo(a.CurrentScore));

            int loopCount = Mathf.Min(_topCount, _validIngredients.Count);
            int totalBonus = 0;

            for (int i = 0; i < loopCount; i++)
            {
                totalBonus += _validIngredients[i].CurrentScore;
            }

            if (totalBonus > 0)
            {
                float trailDuration = _delay * 0.8f;
                bool hasTrail = false;

                for (int i = 0; i < loopCount; i++)
                {
                    if (_validIngredients[i] != _source)
                    {
                        EventBus<PlayScoreTrailEvent>.Publish(new PlayScoreTrailEvent
                        {
                            SourceIngredient = _validIngredients[i],
                            TargetType = EffectTargetType.Ingredient,
                            TargetIngredient = _source,
                            Duration = trailDuration
                        });
                        hasTrail = true;
                    }
                }

                if (hasTrail && trailDuration > 0f)
                    yield return WaitCache.Seconds(trailDuration);

                _source.CurrentScore = totalBonus;

                float remainingDelay = hasTrail ? _delay - trailDuration : _delay;
                if (remainingDelay > 0f)
                    yield return WaitCache.Seconds(remainingDelay);
            }
        }
    }
}
