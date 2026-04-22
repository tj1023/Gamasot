using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Gameplay.TrinketEffects
{
    /// <summary>
    /// 보유 카드 복제 효과입니다.
    /// </summary>
    [Serializable]
    public class DuplicateCardsEffect : TrinketEffect
    {
        [Tooltip("복제 대상 등급")]
        public Rarity targetRarity = Rarity.Rare;

        [Tooltip("true이면 해당 등급 모두 복제, false이면 랜덤 count개")]
        public bool duplicateAll;

        [Tooltip("duplicateAll=false일 때 복제할 카드 수")]
        public int count = 1;

        public override void OnAcquire(GameContext ctx)
        {
            // 대상 등급 재료를 필터링
            var pool = new List<FoodIngredientData>();
            foreach (var item in ctx.SelectedIngredients)
            {
                if (item != null && item.rarity == targetRarity)
                {
                    pool.Add(item);
                }
            }

            if (pool.Count == 0) return;

            if (duplicateAll)
            {
                // 모두 복제
                foreach (var item in pool)
                {
                    ctx.SelectedIngredients.Add(item);
                }
            }
            else
            {
                // 랜덤 count개 복제
                for (int i = 0; i < count; i++)
                {
                    ctx.SelectedIngredients.Add(pool[UnityEngine.Random.Range(0, pool.Count)]);
                }
            }
        }
    }
}
