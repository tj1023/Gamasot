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
        public int priority = 100;
        public int Priority => priority;
        
        [Tooltip("추가할 점수량")]
        public int bonusScore;
        [Tooltip("특정 타입만 추가할 지 여부. None이면 타입 상관없이 추가")]
        public IngredientType targetType = IngredientType.None;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (context.HarvestedIngredients == null || context.HarvestedIngredients.Count == 0) return null;
            
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
                return new IngredientScoreDeltaCommand(source, candidates[randomIndex], bonusScore);
            }
            return null;
        }
    }

    // 2. 본재료 점수 += 랜덤 재료 점수
    [Serializable]
    public class AddRandomScoreToSelfEffect : IEffect
    {
        public int priority = 100;
        public int Priority => priority;
        [Tooltip("점수를 가져올 랜덤 재료의 개수")]
        public int count = 1;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (source == null || context.HarvestedIngredients == null || context.HarvestedIngredients.Count == 0) return null;

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

            if (totalBonus > 0)
            {
                return new IngredientScoreDeltaCommand(source, source, totalBonus);
            }
            return null;
        }
    }

    // 3. 국자에 있는 모든 재료의 점수 +n
    [Serializable]
    public class AddScoreToScoopedIngredientsEffect : IEffect
    {
        public int priority = 100;
        public int Priority => priority;
        public int bonusScore;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (context.LastScooped == null || context.LastScooped.Count == 0) return null;
            
            List<ICommand> commands = new();
            foreach (var item in context.LastScooped)
            {
                if (item != null)
                {
                    commands.Add(new IngredientScoreDeltaCommand(source, item, bonusScore, 0f));
                }
            }
            return new AggregateCommand(commands);
        }
    }

    // 4. 자신의 점수 * n
    [Serializable]
    public class MultiplySelfScoreEffect : IEffect
    {
        public int priority = 200;
        public int Priority => priority;
        public float multiplier = 2f;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (source != null)
            {
                return new MultiplyIngredientScoreCommand(source, source, multiplier);
            }
            return null;
        }
    }

    // 5. 확률로 자신과 이름이 다른 재료 1개를 [고급]으로 전환
    [Serializable]
    public class TransformOtherToAdvancedEffect : IEffect
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
                return new TransformIngredientCommand(candidates[randomIndex]);
            }
            return null;
        }
    }

    // 6. 냄비 안의 모든 Type:X의 점수 +n
    [Serializable]
    public class AddScoreInPotEffect : IEffect
    {
        public int priority = 100;
        public int Priority => priority;
        public IngredientType targetType;
        public int bonusScore;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (context.PotIngredients == null || context.PotIngredients.Count == 0) return null;

            List<ICommand> commands = new();
            foreach (var item in context.PotIngredients)
            {
                if (item != null && item.OriginalData != null && item.OriginalData.type == targetType)
                {
                    commands.Add(new IngredientScoreDeltaCommand(source, item, bonusScore, 0f));
                }
            }
            if (commands.Count > 0) return new AggregateCommand(commands);
            return null;
        }
    }

    // 7. 모든(수집된) 재료의 점수 +n
    [Serializable]
    public class AddScoreToAllHarvestedEffect : IEffect
    {
        public int priority = 100;
        public int Priority => priority;
        public int bonusScore;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (context.HarvestedIngredients == null || context.HarvestedIngredients.Count == 0) return null;

            List<ICommand> commands = new();
            foreach (var item in context.HarvestedIngredients)
            {
                if (item != null) 
                {
                    commands.Add(new IngredientScoreDeltaCommand(source, item, bonusScore, 0.2f));
                }
            }
            if (commands.Count > 0) return new AggregateCommand(commands);
            return null;
        }
    }

    // 8. 점수가 가장 높은 재료 n개의 점수를 얻음
    [Serializable]
    public class AddHighestScoreToSelfEffect : IEffect
    {
        public int priority = 300;
        public int Priority => priority;
        [Tooltip("점수를 가져올 가장 높은 점수의 재료 개수")]
        public int count = 1;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (source == null || context.HarvestedIngredients == null || context.HarvestedIngredients.Count == 0) return null;

            return new AddHighestScoreToSelfCommand(context, source, count);
        }
    }
}
