using System;
using Data;

namespace Gameplay.TrinketEffects
{
    /// <summary>
    /// 팝콘부: 라운드 시작 시, 일반 재료를 고급 재료로 변환합니다.
    /// </summary>
    [Serializable]
    public class PopcornEffect : TrinketEffect
    {
        [UnityEngine.Tooltip("Trinket 1개당 변환할 재료 수")]
        public int transformCountPerTrinket = 2;

        public override void OnRoundStart(GameContext ctx, int trinketCount, TrinketServices services)
        {
            if (services.IngredientManager == null) return;

            services.IngredientManager.TransformRandomToAdvanced(transformCountPerTrinket * trinketCount);
        }
    }
}
