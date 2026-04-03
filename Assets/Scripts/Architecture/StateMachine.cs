namespace Architecture
{
    /// <summary>
    /// 게임 흐름 제어를 위한 범용 상태 머신.
    /// State 팩토리나 외부에서 주입받은 상태 인스턴스(IState)의 참조만 교체(ChangeState)하므로
    /// 매 프레임이나 전환 시에 GC 할당이 발생하지 않습니다.
    /// </summary>
    public class StateMachine<TContext>
    {
        private readonly TContext _context;
        private IState<TContext> _currentState;

        public StateMachine(TContext context)
        {
            _context = context;
        }

        public void ChangeState(IState<TContext> newState)
        {
            if (_currentState == newState) return;

            // 기존 상태 Exit
            _currentState?.Exit(_context);
            
            // 상태 교체
            _currentState = newState;
            
            // 새 상태 Enter
            _currentState?.Enter(_context);
        }

        public void Update()
        {
            _currentState?.Update(_context);
        }
    }
}
