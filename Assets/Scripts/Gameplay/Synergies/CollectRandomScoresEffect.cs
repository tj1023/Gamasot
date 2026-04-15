using System;
using UnityEngine;
using Data;
using Gameplay.Commands;
using Interfaces;

namespace Gameplay.Synergies
{
    [Serializable]
    public class CollectRandomScoresEffect : IEffect
    {
        public int priority = 300;
        public int Priority => priority;
        [Tooltip("점수를 가져올 랜덤 재료의 개수")]
        public int pickCount = 1;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (source == null || context.HarvestedIngredients == null || context.HarvestedIngredients.Count == 0) return null;

            return new CollectRandomScoresCommand(context, source, pickCount);
        }
    }
}
