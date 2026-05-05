using Data;
using Interfaces;

namespace Core
{
    /// <summary>
    /// 게임 종료 상태.
    /// 게임 오버 UI가 표시되며, 플레이어가 재시작 버튼을 누르면 게임이 초기화됩니다.
    /// </summary>
    public class GameOverState : IState<GameContext>
    {
        public void Enter(GameContext context)
        {
            context.CurrentPhase = GamePhase.GameOver;
            EventBus<PhaseChangedEvent>.Publish(new PhaseChangedEvent { NewPhase = GamePhase.GameOver });
        }

        public void Update(GameContext context)
        {
        }

        public void Exit()
        {
        }
    }
}
