using System;
using System.Collections.Generic;
using Data;

namespace Gameplay.TrinketEffects
{
    /// <summary>
    /// 장식품 효과의 추상 기반 클래스입니다.
    /// [SerializeReference, SubclassSelector]를 통해 인스펙터에서 구현체를 선택할 수 있습니다.
    /// 각 구현체는 필요한 타이밍(OnAcquire, OnRoundStart, OnHarvest)만 override합니다.
    /// </summary>
    [Serializable]
    public abstract class TrinketEffect
    {
        /// <summary>
        /// 장식품 획득 시 즉시 실행되는 효과 (일회성).
        /// </summary>
        public virtual void OnAcquire(GameContext ctx) { }

        /// <summary>
        /// Discover(카드 발견) 흐름이 필요한 효과인지 여부.
        /// true이면 TrinketManager가 Discover UI를 통해 처리합니다.
        /// </summary>
        public virtual bool IsDiscoverEffect => false;

        /// <summary>
        /// Discover 효과일 경우 반복 횟수.
        /// </summary>
        public virtual int DiscoverCount => 1;

        /// <summary>
        /// Discover 효과일 경우 후보 목록을 반환합니다.
        /// </summary>
        public virtual List<FoodIngredientData> GetDiscoverCandidates(
            GameContext ctx, FoodIngredientData[] allIngredients) => null;

        /// <summary>
        /// 라운드 시작(OnScoop 진입) 시 호출됩니다.
        /// </summary>
        public virtual void OnRoundStart(GameContext ctx, int trinketCount, TrinketServices services) { }

        /// <summary>
        /// 재료 건지기(Harvest) 시 호출됩니다.
        /// </summary>
        public virtual void OnHarvest(GameContext ctx, List<RuntimeIngredient> items,
            int trinketCount, TrinketServices services) { }
    }
}
