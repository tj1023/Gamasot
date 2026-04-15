using System;
using Data;
using Interfaces;

namespace Gameplay.Synergies
{
    /// <summary>
    /// 정산 시(OnSettlement)에만 발동하는 조건입니다.
    /// </summary>
    [Serializable]
    public class OnSettlementTrigger : ITrigger
    {
        public bool Evaluate(GameContext context)
        {
            return context is { CurrentPhase: GamePhase.OnSettlement };
        }
    }

    /// <summary>
    /// 재료를 건질 때(OnScoop) 발동하는 조건입니다.
    /// </summary>
    [Serializable]
    public class OnScoopTrigger : ITrigger
    {
        public bool Evaluate(GameContext context)
        {
            return context is { CurrentPhase: GamePhase.OnScoop };
        }
    }
}
