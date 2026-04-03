using System.Collections.Generic;

namespace Architecture
{
    /// <summary>
    /// 게임 모델/데이터 컨테이너입니다.
    /// 이벤트 버스를 통해 데이터를 직접 주고받을 수도 있지만,
    /// FSM 전환 시나 시너지 Trigger/Effect 발동 시 전역 컨텍스트(데이터)를 전달하여
    /// 서로 다른 시스템 간 결합도를 낮추기 위해 사용됩니다.
    /// </summary>
    public class GameContext
    {
        // 런타임 동안 모은 점수
        public int CurrentScore { get; set; }
        
        // 현재 수집/선택된 재료 목록 (시너지 판단용)
        public List<FoodIngredientData> CurrentIngredients { get; set; } = new();
    }
}
