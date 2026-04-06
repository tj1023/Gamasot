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
    /// 시너지 조건 충족 시 발동될 효과 (혹은 점수 변형자) 인터페이스입니다.
    /// 정산 시 파이프라인 정렬을 위해 우선순위(Priority)를 가지며 비동기 커맨드를 생성합니다.
    /// </summary>
    public interface IEffect
    {
        int Priority { get; }
        ICommand GenerateCommand(GameContext context, Gameplay.RuntimeIngredient source);
    }

    /// <summary>
    /// 비동기 실행 가능한 커맨드 (UI 애니메이션 및 딜레이 포함)
    /// </summary>
    public interface ICommand
    {
        System.Collections.IEnumerator ExecuteAsync();
    }

    // --- State Machine ---

    /// <summary>
    /// FSM 구조에서 사용될 범용 상태 인터페이스입니다.
    /// GC 최소화를 위해 상태를 매번 new 하지 않고 컨텍스트를 주입받아 처리합니다.
    /// </summary>
    public interface IState<in TContext>
    {
        void Enter(TContext context);
        void Exit();
    }

    // --- Event System ---

    /// <summary>
    /// 박싱 없는 EventBus를 위한 리스너 인터페이스.
    /// </summary>
    public interface IEventListener<in TEvent> where TEvent : struct
    {
        void OnEvent(TEvent eventData);
    }
}
