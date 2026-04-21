using Data;
using Interfaces;

namespace Core
{
    /// <summary>
    /// 일정 라운드마다 장식품 선택 페이즈를 담당하는 상태.
    /// UI에서 장식품 선택이 완료되면 OnSelection으로 전환됩니다.
    /// </summary>
    public class TrinketSelectionState : IState<GameContext>
    {
        public void Enter(GameContext context)
        {
            context.CurrentPhase = GamePhase.OnTrinketSelection;

            EventBus<PhaseChangedEvent>.Publish(new PhaseChangedEvent { NewPhase = GamePhase.OnTrinketSelection });
        }

        public void Update(GameContext context)
        {
        }

        public void Exit()
        {
        }
    }
}
