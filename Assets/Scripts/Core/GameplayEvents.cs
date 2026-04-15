using System.Collections.Generic;
using Data;

namespace Core
{
    public struct ItemsHarvestedEvent
    {
        public List<RuntimeIngredient> NewHarvestedItems;
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

    public struct TimerUpdatedEvent
    {
        public float RemainTime;
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
    }
}
