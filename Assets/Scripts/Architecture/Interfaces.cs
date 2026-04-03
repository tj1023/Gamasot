namespace Architecture
{
    // --- Data & Synergy System ---
    
    /// <summary>
    /// 시너지 발동 조건을 검사하는 인터페이스입니다.
    /// ScriptableObject 인스펙터 직렬화를 위해 [SerializeReference]를 지원합니다.
    /// </summary>
    public interface ITrigger
    {
        bool Evaluate(GameContext context);
    }

    /// <summary>
    /// 시너지 조건 충족 시 발동될 효과 인터페이스입니다.
    /// </summary>
    public interface IEffect
    {
        void Apply(GameContext context);
    }

    // --- State Machine ---

    /// <summary>
    /// FSM 구조에서 사용될 범용 상태 인터페이스입니다.
    /// GC 최소화를 위해 상태를 매번 new 하지 않고 컨텍스트를 주입받아 처리합니다.
    /// </summary>
    public interface IState<TContext>
    {
        void Enter(TContext context);
        void Update(TContext context);
        void Exit(TContext context);
    }

    // --- Event System ---

    /// <summary>
    /// 박싱 없는 EventBus를 위한 리스너 인터페이스.
    /// </summary>
    public interface IEventListener<TEvent> where TEvent : struct
    {
        void OnEvent(TEvent eventData);
    }
}
