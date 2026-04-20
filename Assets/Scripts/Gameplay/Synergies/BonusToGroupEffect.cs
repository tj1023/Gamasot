using System;
using System.Collections.Generic;
using Data;
using Gameplay.Commands;
using Interfaces;

namespace Gameplay.Synergies
{
    public enum TargetList
    {
        HarvestedIngredients,
        PotIngredients,
        LastScooped
    }

    /// <summary>
    /// 지정된 그룹(리스트)의 재료들에게 일괄로 보너스 점수를 부여하는 통합 이펙트입니다.
    /// 기존 AddScoreToAllHarvestedEffect, AddScoreInPotEffect, AddScoreToScoopedIngredientsEffect를 대체합니다.
    /// </summary>
    [Serializable]
    public class BonusToGroupEffect : IEffect
    {
        public int priority = 100;
        public int Priority => priority;

        public TargetList targetList;
        public int bonusScore;
        public IngredientType ingredientFilter = IngredientType.None;
        public float delay;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            List<RuntimeIngredient> targets = targetList switch
            {
                TargetList.HarvestedIngredients => context.HarvestedIngredients,
                TargetList.PotIngredients => context.PotIngredients,
                TargetList.LastScooped => context.LastScooped,
                _ => null
            };

            if (targets == null || targets.Count == 0) return null;

            List<RuntimeIngredient> filteredTargets = new();
            foreach (var item in targets)
            {
                if (item == null) continue;
                if (ingredientFilter != IngredientType.None
                    && (item.OriginalData == null || item.OriginalData.type != ingredientFilter)) continue;

                filteredTargets.Add(item);
            }

            return filteredTargets.Count > 0 ? new AddScoreToGroupCommand(source, filteredTargets, bonusScore, delay) : null;
        }
    }
}
