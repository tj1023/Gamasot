using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Gameplay.TrinketEffects
{
    /// <summary>
    /// 소다부: 건질 때, 일정 수 이상을 건지면 보유 재료 중 랜덤으로 1개를 pot 안에 추가합니다.
    /// </summary>
    [Serializable]
    public class SodaEffect : TrinketEffect
    {
        [Tooltip("효과 발동에 필요한 최소 건지기 수")]
        public int minScoopCount = 4;

        public override void OnHarvest(GameContext ctx, List<RuntimeIngredient> items,
            int trinketCount, TrinketServices services)
        {
            if (items == null || items.Count < minScoopCount) return;
            if (ctx.SelectedIngredients.Count == 0) return;
            if (services.IngredientManager == null) return;

            for (int i = 0; i < trinketCount; i++)
            {
                var randomOwned = ctx.SelectedIngredients[UnityEngine.Random.Range(0, ctx.SelectedIngredients.Count)];
                services.IngredientManager.SpawnIngredient(randomOwned);
            }
        }
    }
}
