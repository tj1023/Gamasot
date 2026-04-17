using Data;
using Interfaces;

namespace Core
{
    /// <summary>
    /// 라운드 시작 전 재료 선택 페이즈를 담당하는 상태.
    /// UI에서 재료 선택이 완료되면 OnScoop으로 전환됩니다.
    /// </summary>
    public class SelectionState : IState<GameContext>
    {
        public void Enter(GameContext context)
        {
            context.CurrentPhase = GamePhase.OnSelection;

            // 첫 라운드는 2번, 이후 라운드는 1번 선택
            context.RemainSelectionCount = context.CurrentRound == 1 ? 2 : 1;

            EventBus<PhaseChangedEvent>.Publish(new PhaseChangedEvent { NewPhase = GamePhase.OnSelection });
            EventBus<RoundUpdatedEvent>.Publish(new RoundUpdatedEvent { CurrentRound = context.CurrentRound });
            EventBus<TotalScoreUpdatedEvent>.Publish(new TotalScoreUpdatedEvent { TotalScore = context.TotalScore });
        }

        public void Update(GameContext context)
        {
            // UI 클릭 이벤트 기반이므로 Update에서 처리할 것 없음
        }

        public void Exit()
        {
        }
    }
}
