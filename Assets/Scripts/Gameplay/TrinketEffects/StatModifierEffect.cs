using System;
using Data;
using UnityEngine;

namespace Gameplay.TrinketEffects
{
    public enum TrinketStatType
    {
        MaxSelectedIngredients,
        MaxScoopCount,
        ScoopRadius,
        PuddingEffect
    }

    /// <summary>
    /// 스탯 수정 효과입니다.
    /// 4가지 패시브/즉시 효과를 설정값으로 통합합니다:
    /// - IncreaseMaxSelectedIngredients: statType=MaxSelectedIngredients, amount=1
    /// - IncreaseMaxScoopCount: statType=MaxScoopCount, amount=1
    /// - IncreaseScoopRadius: statType=ScoopRadius, amount=0.5
    /// - PuddingEffect: statType=PuddingEffect (amount 무시, boolean 토글)
    /// </summary>
    [Serializable]
    public class StatModifierEffect : TrinketEffect
    {
        [Tooltip("수정할 스탯 종류")]
        public TrinketStatType statType;

        [Tooltip("증가량 (PuddingEffect일 경우 무시)")]
        public float amount = 1f;

        public override void OnAcquire(GameContext ctx)
        {
            switch (statType)
            {
                case TrinketStatType.MaxSelectedIngredients:
                    ctx.MaxSelectedIngredients += (int)amount;
                    break;
                case TrinketStatType.MaxScoopCount:
                    ctx.MaxScoopCount += (int)amount;
                    ctx.RemainScoopCount += (int)amount;
                    break;
                case TrinketStatType.ScoopRadius:
                    ctx.TrinketModifiers.ScoopRadiusModifier += amount;
                    break;
                case TrinketStatType.PuddingEffect:
                    ctx.TrinketModifiers.HasPuddingEffect = true;
                    break;
            }
        }
    }
}
