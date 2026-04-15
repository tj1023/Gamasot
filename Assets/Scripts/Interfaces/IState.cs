namespace Interfaces
{
    /// <summary>
    /// FSM 구조에서 사용될 범용 상태 인터페이스입니다.
    /// GC 최소화를 위해 상태를 매번 new 하지 않고 컨텍스트를 주입받아 처리합니다.
    /// </summary>
    public interface IState<in TContext>
    {
        void Enter(TContext context);
        void Update(TContext context);
        void Exit();
    }
}
