using System.Collections.Generic;
using Gameplay;

namespace Architecture
{
    /// <summary>
    /// 재료 건지기(Scoop) 시 발생되는 이벤트.
    /// PotInteraction 등에서 건져진 재료들의 목록을 전달합니다.
    /// </summary>
    public struct ItemsHarvestedEvent
    {
        // 런타임에 새로 채집된 재료 목록입니다.
        // GC 최소화를 위해 보통 이벤트 발행 직전에 리스트를 넘겨주거나, ReadOnly 콜렉션을 사용합니다.
        public List<RuntimeIngredient> NewHarvestedItems;
    }

    /// <summary>
    /// 런타임 재료의 점수 변경(예: 정산 로직) 시 발생되는 이벤트.
    /// UI 프레젠테이션(그리드 셀) 갱신 용도로 사용합니다.
    /// </summary>
    public struct IngredientScoreChangedEvent
    {
        public RuntimeIngredient Ingredient;
        public int OldScore;
        public int NewScore;
    }
}
