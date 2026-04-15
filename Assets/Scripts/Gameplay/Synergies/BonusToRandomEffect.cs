using System.Collections.Generic;
using System;
using UnityEngine;
using Data;
using Gameplay.Commands;
using Interfaces;

namespace Gameplay.Synergies
{
    [Serializable]
    public class BonusToRandomEffect : IEffect
    {
        public int priority = 100;
        public int Priority => priority;
        
        [Tooltip("추가할 점수량")]
        public int bonusScore;
        [Tooltip("특정 타입만 추가할 지 여부. None이면 타입 상관없이 추가")]
        public IngredientType ingredientFilter = IngredientType.None;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (context.HarvestedIngredients == null || context.HarvestedIngredients.Count == 0) return null;
            
            List<RuntimeIngredient> candidates = new List<RuntimeIngredient>();
            foreach (var item in context.HarvestedIngredients)
            {
                if (item != null)
                {
                    if (ingredientFilter == IngredientType.None || (item.OriginalData != null && item.OriginalData.type == ingredientFilter))
                    {
                        candidates.Add(item);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
                return new AddScoreCommand(source, candidates[randomIndex], bonusScore);
            }
            return null;
        }
    }
}
