using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Gameplay.TrinketEffects
{
    /// <summary>
    /// 카드 발견(Discover) 효과입니다.
    /// </summary>
    [Serializable]
    public class DiscoverCardEffect : TrinketEffect
    {
        [Tooltip("발견 대상의 최소 등급")]
        public Rarity minRarity = Rarity.Rare;

        [Tooltip("발견 횟수")]
        public int discoverCount = 1;

        [Tooltip("true이면 보유 재료(SelectedIngredients)에서만 검색")]
        public bool ownedOnly;

        public override bool IsDiscoverEffect => true;
        public override int DiscoverCount => discoverCount;

        public override List<FoodIngredientData> GetDiscoverCandidates(
            GameContext ctx, FoodIngredientData[] allIngredients)
        {
            var candidates = new List<FoodIngredientData>();

            if (ownedOnly)
            {
                // 보유 재료에서 검색 (중복 제거)
                var seen = new HashSet<FoodIngredientData>();
                foreach (var item in ctx.SelectedIngredients)
                {
                    if (item != null && item.rarity >= minRarity && seen.Add(item))
                    {
                        candidates.Add(item);
                    }
                }
            }
            else
            {
                // 전체 풀에서 검색
                if (allIngredients == null) return candidates;

                foreach (var item in allIngredients)
                {
                    if (item != null && item.rarity >= minRarity)
                    {
                        candidates.Add(item);
                    }
                }
            }

            return candidates;
        }
    }
}
