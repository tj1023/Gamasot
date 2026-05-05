using System.Collections.Generic;

namespace Data
{
    public enum GamePhase
    {
        None,
        Ready,
        OnTrinketSelection,
        OnSelection,
        OnScoop,
        OnSettlement,
        GameOver
    }

    /// <summary>
    /// 게임 모델/데이터 컨테이너입니다.
    /// FSM 및 시너지 Trigger/Effect 발동 시 전역 컨텍스트(데이터)를 전달하여
    /// 서로 다른 시스템 간 결합도를 낮추기 위해 사용됩니다.
    /// </summary>
    public class GameContext
    {
        // 전역 누적 점수
        public int TotalScore { get; set; }
        
        // 현재 라운드에서 획득한 점수
        public int RoundScore { get; set; }

        public GamePhase CurrentPhase { get; set; }
        
        // 라운드 및 수집 제한 데이터
        public static int MaxRound => 10;
        public int CurrentRound { get; set; } = 1;
        public int MaxScoopCount { get; set; } = 3;
        public int RemainScoopCount { get; set; }
        public int RemainSelectionCount { get; set; }
        public int MaxSelectedIngredients { get; set; } = 7;
        public float RemainTime { get; set; }
        
        // 게임 일시정지 상태
        public bool IsPaused { get; set; }

        // 현재 이벤트를 발동시킨 주체 재료
        public RuntimeIngredient Source { get; set; }

        // 현재 수집된 모든 재료 목록
        public List<RuntimeIngredient> HarvestedIngredients { get; } = new();

        // 방금 막 건져진 재료 목록 (건질 때 사용할 임시 리스트)
        public List<RuntimeIngredient> LastScooped { get; } = new();

        // 라운드 시작 전 플레이어가 선택한 재료 데이터 목록
        public List<FoodIngredientData> SelectedIngredients { get; } = new();

        // 아직 냄비 안에 남아있는 재료 목록 
        // (실제 게임에서는 IngredientManager 등을 통해 갱신해야 함)
        public List<RuntimeIngredient> PotIngredients { get; } = new();

        /// <summary>
        /// 리스트의 내용물을 교체합니다. 리스트 인스턴스 자체는 유지됩니다.
        /// </summary>
        private static void ReplaceContents<T>(List<T> target, List<T> source)
        {
            target.Clear();
            target.AddRange(source);
        }

        /// <summary>
        /// LastScooped 리스트의 내용물을 교체합니다.
        /// </summary>
        public void SetLastScooped(List<RuntimeIngredient> source) => ReplaceContents(LastScooped, source);

        /// <summary>
        /// PotIngredients 리스트의 내용물을 교체합니다.
        /// </summary>
        public void SetPotIngredients(List<RuntimeIngredient> source) => ReplaceContents(PotIngredients, source);

        // 장식품(Trinket) 데이터
        public Dictionary<TrinketData, int> TrinketCounts { get; } = new();
        
        // 장식품 패시브 효과들
        public TrinketModifiers TrinketModifiers { get; } = new();
    }

    /// <summary>
    /// 장식품(Trinket)에 의한 패시브 효과들을 관리합니다.
    /// 새 Trinket 패시브가 추가될 때 이 클래스만 확장하면 됩니다.
    /// </summary>
    public class TrinketModifiers
    {
        public float ScoopRadiusModifier { get; set; } = 1f;
        public bool HasPuddingEffect { get; set; }
    }
}
