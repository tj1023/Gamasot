using System;
using System.Collections.Generic;
using Data;

namespace Gameplay.TrinketEffects
{
    /// <summary>
    /// 우유부: 건질 때, 같은 재료 2개를 동시에 건지면 pot 안에 해당 재료 1개를 추가합니다.
    /// </summary>
    [Serializable]
    public class MilkEffect : TrinketEffect
    {
        public override void OnHarvest(GameContext ctx, List<RuntimeIngredient> items,
            int trinketCount, TrinketServices services)
        {
            if (items == null || items.Count == 0) return;
            if (services.IngredientManager == null) return;

            // LINQ GroupBy 대신 Dictionary 수동 그룹핑 (GC-free)
            var groupCounts = new Dictionary<FoodIngredientData, int>();
            foreach (var item in items)
            {
                if (item?.OriginalData == null) continue;

                if (groupCounts.TryGetValue(item.OriginalData, out int c))
                {
                    groupCounts[item.OriginalData] = c + 1;
                }
                else
                {
                    groupCounts[item.OriginalData] = 1;
                }
            }

            foreach (var kvp in groupCounts)
            {
                if (kvp.Value >= 2)
                {
                    for (int i = 0; i < trinketCount; i++)
                    {
                        services.IngredientManager.SpawnIngredient(kvp.Key);
                    }
                }
            }
        }
    }
}
