using UnityEngine;
using Data;
using Interfaces;

namespace Core
{
    /// <summary>
    /// 게임 진행 중 수집/선택 페이즈를 담당하는 상태
    /// </summary>
    public class PlayingState : IState<GameContext>
    {
        public void Enter(GameContext context)
        {
            context.CurrentPhase = GamePhase.OnScoop;
            context.RemainScoopCount = context.MaxScoopCount;
            context.RemainTime = context.MaxScoopCount * 5f;
            
            EventBus<PhaseChangedEvent>.Publish(new PhaseChangedEvent { NewPhase = GamePhase.OnScoop });
            EventBus<TimerUpdatedEvent>.Publish(new TimerUpdatedEvent { RemainTime = context.RemainTime });
            EventBus<ScoopCountUpdatedEvent>.Publish(new ScoopCountUpdatedEvent { RemainScoopCount = context.RemainScoopCount });
            EventBus<RoundUpdatedEvent>.Publish(new RoundUpdatedEvent { CurrentRound = context.CurrentRound });
            EventBus<TotalScoreUpdatedEvent>.Publish(new TotalScoreUpdatedEvent { TotalScore = context.TotalScore });
        }

        public void Update(GameContext context)
        {
            if (context.RemainTime > 0)
            {
                context.RemainTime -= Time.deltaTime;
                EventBus<TimerUpdatedEvent>.Publish(new TimerUpdatedEvent { RemainTime = context.RemainTime });

                if (context.RemainTime <= 0)
                {
                    EventBus<RequestPhaseChangeEvent>.Publish(new RequestPhaseChangeEvent { TargetPhase = GamePhase.OnSettlement });
                }
            }
        }

        public void Exit()
        {
        }
    }
}
