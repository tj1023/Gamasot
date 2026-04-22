using System;
using System.Collections.Generic;
using Core;
using Data;
using UnityEngine;

namespace Gameplay.TrinketEffects
{
    /// <summary>
    /// 츠바부: 라운드 시작 시, 정산 패널에 특정 재료를 보너스로 추가합니다.
    /// </summary>
    [Serializable]
    public class TsubaEffect : TrinketEffect
    {
        [Tooltip("추가할 재료의 이름 키워드 (ingredientName에 포함 여부로 검색)")]
        public string ingredientNameKeyword = "츠바";

        // 캐싱된 재료 데이터 (최초 1회 검색)
        private FoodIngredientData _cachedData;
        private bool _cacheResolved;

        public override void OnRoundStart(GameContext ctx, int trinketCount, TrinketServices services)
        {
            if (services.AllIngredients == null) return;

            // 최초 1회만 이름으로 검색하여 캐싱
            if (!_cacheResolved)
            {
                _cacheResolved = true;
                foreach (var ingredient in services.AllIngredients)
                {
                    if (ingredient != null && ingredient.ingredientName.Contains(ingredientNameKeyword))
                    {
                        _cachedData = ingredient;
                        break;
                    }
                }
            }

            if (_cachedData == null) return;

            var bonusItems = new List<RuntimeIngredient>();
            for (int i = 0; i < trinketCount; i++)
            {
                bonusItems.Add(new RuntimeIngredient(_cachedData));
            }

            EventBus<ItemsHarvestedEvent>.Publish(new ItemsHarvestedEvent
            {
                NewHarvestedItems = bonusItems,
                IsBonus = true
            });
        }
    }
}
