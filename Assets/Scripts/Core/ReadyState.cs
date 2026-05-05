using Data;
using Interfaces;

namespace Core
{
    /// <summary>
    /// 게임 시작 전 대기 상태.
    /// 게임 시작 UI가 표시되며, 플레이어가 시작 버튼을 누르면 OnSelection으로 전환됩니다.
    /// </summary>
    public class ReadyState : IState<GameContext>
    {
        public void Enter(GameContext context)
        {
            context.CurrentPhase = GamePhase.Ready;
            EventBus<PhaseChangedEvent>.Publish(new PhaseChangedEvent { NewPhase = GamePhase.Ready });
        }

        public void Update(GameContext context)
        {
        }

        public void Exit()
        {
        }
    }
}
