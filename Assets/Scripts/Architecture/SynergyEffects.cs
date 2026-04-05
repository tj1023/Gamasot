using System;
using UnityEngine;
using System.Collections.Generic;
using Gameplay;

namespace Architecture
{
    // 1. 수집된 재료중 랜덤으로 1개의 점수 + n
    [Serializable]
    public class AddScoreToSelfEffect : IEffect
    {
        [Tooltip("추가할 점수량")]
        public int bonusScore;
        [Tooltip("특정 타입만 추가할 지 여부. None이면 타입 상관없이 추가")]
        public IngredientType targetType = IngredientType.None;

        public void Apply(GameContext context)
        {
            if (context.HarvestedIngredients == null || context.HarvestedIngredients.Count == 0) return;
            
            List<RuntimeIngredient> candidates = new List<RuntimeIngredient>();
            foreach (var item in context.HarvestedIngredients)
            {
                if (item != null)
                {
                    if (targetType == IngredientType.None || (item.OriginalData != null && item.OriginalData.type == targetType))
                    {
                        candidates.Add(item);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
                candidates[randomIndex].CurrentScore += bonusScore;
            }
        }
    }

    // 2. 본재료에 수집된 재료중 랜덤으로 1개의 점수를 획득
    [Serializable]
    public class AddRandomScoreToSelfEffect : IEffect
    {
        public void Apply(GameContext context)
        {
            if (context.Source == null || context.HarvestedIngredients == null || context.HarvestedIngredients.Count == 0) return;

            List<RuntimeIngredient> validIngredients = new List<RuntimeIngredient>();
            foreach (var item in context.HarvestedIngredients)
            {
                if (item != null)
                {
                    validIngredients.Add(item);
                }
            }

            if (validIngredients.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, validIngredients.Count);
                int randomIngredientScore = validIngredients[randomIndex].CurrentScore;
                context.Source.CurrentScore += randomIngredientScore;
            }
        }
    }

    // 3. 국자에 있는 모든 재료의 점수 +n
    [Serializable]
    public class AddScoreToScoopedIngredientsEffect : IEffect
    {
        public int bonusScore;

        public void Apply(GameContext context)
        {
            if (context.LastScooped == null) return;
            foreach (var item in context.LastScooped)
            {
                if (item != null)
                {
                    item.CurrentScore += bonusScore;
                }
            }
        }
    }

    // 4. 자신의 점수 * n
    [Serializable]
    public class MultiplySelfScoreEffect : IEffect
    {
        public float multiplier = 2f;

        public void Apply(GameContext context)
        {
            if (context.Source != null)
            {
                context.Source.CurrentScore = Mathf.RoundToInt(context.Source.CurrentScore * multiplier);
            }
        }
    }

    // 5. 확률로 자신과 이름이 다른 재료 1개를 [고급]으로 전환
    [Serializable]
    public class TransformOtherIntoPremiumEffect : IEffect
    {
        public float probability = 0.5f;

        public void Apply(GameContext context)
        {
            if (context.Source == null || context.Source.OriginalData == null) return;

            // 확률 검사
            if (UnityEngine.Random.value > probability) return;

            // 전체(수집된) 재료 중 자신과 이름이 다르고, 아직 고급이 아닌 재료 하나를 찾음
            List<RuntimeIngredient> candidates = new List<RuntimeIngredient>();
            foreach (var item in context.HarvestedIngredients)
            {
                if (item != null && item.OriginalData != null)
                {
                    if (item.OriginalData.ingredientName != context.Source.OriginalData.ingredientName && !item.IsAdvanced)
                    {
                        candidates.Add(item);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                // 랜덤으로 하나 선택
                int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
                candidates[randomIndex].TransformToAdvanced();
            }
        }
    }

    // 6. 냄비 안의 모든 Type:X의 점수 +n
    [Serializable]
    public class AddScoreToTypeInPotEffect : IEffect
    {
        public IngredientType targetType;
        public int bonusScore;

        public void Apply(GameContext context)
        {
            if (context.PotIngredients == null) return;

            foreach (var item in context.PotIngredients)
            {
                if (item != null && item.OriginalData != null && item.OriginalData.type == targetType)
                    item.CurrentScore += bonusScore;
            }
        }
    }

    // 7. 모든(수집된) 재료의 점수 +n
    [Serializable]
    public class AddScoreToAllHarvestedIngredientsEffect : IEffect
    {
        public int bonusScore;

        public void Apply(GameContext context)
        {
            if (context.HarvestedIngredients == null) return;

            foreach (var item in context.HarvestedIngredients)
                if (item != null) item.CurrentScore += bonusScore;
        }
    }

    // 8. 점수가 가장 높은 재료 1개의 점수를 얻음
    [Serializable]
    public class AddRandomBonusToHighestScoreIngredientEffect : IEffect
    {
        public void Apply(GameContext context)
        {
            if (context.Source == null || context.HarvestedIngredients == null || context.HarvestedIngredients.Count == 0) return;

            RuntimeIngredient highest = null;

            foreach (var current in context.HarvestedIngredients)
            {
                if (current == null) continue;
                if (highest == null || current.CurrentScore > highest.CurrentScore)
                    highest = current;
            }

            if (highest != null)
                context.Source.CurrentScore = highest.CurrentScore;
        }
    }
}
