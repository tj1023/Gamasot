using System.Collections.Generic;
using Data;

namespace Core
{
    public struct ItemsHarvestedEvent
    {
        public List<RuntimeIngredient> NewHarvestedItems;
        public bool IsBonus;
    }

    public struct IngredientScoreChangedEvent
    {
        public RuntimeIngredient Ingredient;
        public int OldScore;
        public int NewScore;
    }

    public struct PhaseChangedEvent
    {
        public GamePhase NewPhase;
    }

    public struct RequestPhaseChangeEvent
    {
        public GamePhase TargetPhase;
    }

    public struct RoundScoreUpdatedEvent
    {
        public int RoundScore;
    }

    public struct TotalScoreUpdatedEvent
    {
        public int TotalScore;
    }

    public struct RoundUpdatedEvent
    {
        public int CurrentRound;
    }

    public struct ScoopCountUpdatedEvent
    {
        public int RemainScoopCount;
    }

    public struct IngredientSelectedEvent
    {
        public FoodIngredientData SelectedData;
    }

    public struct TrinketSelectedEvent
    {
        public TrinketData SelectedTrinket;
    }

    public struct RequestDeleteExcessEvent
    {
        public int ExcessCount;
    }

    public struct ExcessDeletedEvent
    {
    }

    public struct RequestDiscoverEvent
    {
        public List<FoodIngredientData> Candidates;
    }

    public struct DiscoverItemSelectedEvent
    {
        public FoodIngredientData SelectedData;
    }

    public struct TimerUpdatedEvent
    {
        public float RemainTime;
    }

    public struct GamePausedEvent
    {
        public bool IsPaused;
    }

    public enum EffectTargetType
    {
        Ingredient,
        TotalScore
    }

    public struct PlayScoreTrailEvent
    {
        public RuntimeIngredient SourceIngredient;
        public RuntimeIngredient TargetIngredient;
        public EffectTargetType TargetType;
        public float Duration;
        public int FixedDirection; // 0: random, 1: up, -1: down
    }

    public struct OwnedTrinketsUpdatedEvent
    {
    }

    public struct SynergyActivatedEvent
    {
    }

    public struct TrailArrivedEvent
    {
    }
}
