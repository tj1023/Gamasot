using System.Collections.Generic;
using System;
using Data;
using Gameplay.Commands;
using Interfaces;

namespace Gameplay.Synergies
{
    [Serializable]
    public class UpgradeRandomEffect : IEffect
    {
        public int priority = 90;
        public int Priority => priority;
        public float probability = 0.5f;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (source == null || source.OriginalData == null) return null;

            if (UnityEngine.Random.value > probability) return null;

            List<RuntimeIngredient> candidates = new List<RuntimeIngredient>();
            foreach (var item in context.HarvestedIngredients)
            {
                if (item != null && item.OriginalData != null)
                {
                    if (item.OriginalData.ingredientName != source.OriginalData.ingredientName && !item.IsAdvanced)
                    {
                        candidates.Add(item);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
                return new UpgradeIngredientCommand(candidates[randomIndex]);
            }
            return null;
        }
    }
}
