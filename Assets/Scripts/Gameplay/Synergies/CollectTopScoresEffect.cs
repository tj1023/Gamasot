using System;
using UnityEngine;
using Data;
using Gameplay.Commands;
using Interfaces;

namespace Gameplay.Synergies
{
    [Serializable]
    public class CollectTopScoresEffect : IEffect
    {
        public int priority = 300;
        public int Priority => priority;
        [Tooltip("점수를 가져올 가장 높은 점수의 재료 개수")]
        public int topCount = 1;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (source == null || context.HarvestedIngredients == null || context.HarvestedIngredients.Count == 0) return null;

            return new CollectTopScoresCommand(context, source, topCount);
        }
    }
}
