using System;
using UnityEngine;
using System.Collections.Generic;
using Gameplay;

namespace Architecture
{
    // 1. 랜덤 재료 점수 += n
    [Serializable]
    public class AddScoreToRandomEffect : IEffect
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

    // 2. 본재료 점수 += 랜덤 재료 점수
    [Serializable]
    public class AddRandomScoreToSelfEffect : IEffect
    {
        [Tooltip("점수를 가져올 랜덤 재료의 개수")]
        public int count = 1;

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

            int loopCount = Mathf.Min(count, validIngredients.Count);
            int totalBonus = 0;

            for (int i = 0; i < loopCount; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, validIngredients.Count);
                totalBonus += validIngredients[randomIndex].CurrentScore;
                validIngredients.RemoveAt(randomIndex);
            }

            context.Source.CurrentScore += totalBonus;
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
    public class TransformOtherToAdvancedEffect : IEffect
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
    public class AddScoreInPotEffect : IEffect
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
    public class AddScoreToAllHarvestedEffect : IEffect
    {
        public int bonusScore;

        public void Apply(GameContext context)
        {
            if (context.HarvestedIngredients == null) return;

            foreach (var item in context.HarvestedIngredients)
                if (item != null) item.CurrentScore += bonusScore;
        }
    }

    // 8. 점수가 가장 높은 재료 n개의 점수를 얻음
    [Serializable]
    public class AddHighestScoreToSelfEffect : IEffect
    {
        [Tooltip("점수를 가져올 가장 높은 점수의 재료 개수")]
        public int count = 1;

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

            // 점수 내림차순 정렬
            validIngredients.Sort((a, b) => b.CurrentScore.CompareTo(a.CurrentScore));

            int loopCount = Mathf.Min(count, validIngredients.Count);
            int totalBonus = 0;

            for (int i = 0; i < loopCount; i++)
            {
                totalBonus += validIngredients[i].CurrentScore;
            }

            context.Source.CurrentScore += totalBonus;
        }
    }
}
