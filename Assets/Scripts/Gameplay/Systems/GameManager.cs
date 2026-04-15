using UnityEngine;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Systems
{
    /// <summary>
    /// 게임 전체의 흐름(상태), 라운드, 전역 컨텍스트를 소유하고 관리하는 컨트롤러입니다.
    /// EventBus를 통해 다른 시스템과 소통하여 결합도를 낮춥니다.
    /// </summary>
    public class GameManager : MonoBehaviour, 
        IEventListener<RequestPhaseChangeEvent>, 
        IEventListener<ItemsHarvestedEvent>
    {
        public static GameManager Instance { get; private set; }

        public GameContext Context { get; private set; }
        private StateMachine<GameContext> _stateMachine;

        private PlayingState _playingState;
        private SettlementState _settlementState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 컨텍스트 및 상태 머신 초기화
            Context = new GameContext();
            _stateMachine = new StateMachine<GameContext>(Context);

            _playingState = new PlayingState();
            _settlementState = new SettlementState();
        }

        private void Start()
        {
            // 이벤트 구독
            EventBus<RequestPhaseChangeEvent>.Subscribe(this);
            EventBus<ItemsHarvestedEvent>.Subscribe(this);

            // 초기 상태 진입
            _stateMachine.ChangeState(_playingState);
        }

        private void Update()
        {
            _stateMachine?.Update();
        }
        
        private void OnDestroy()
        {
            EventBus<RequestPhaseChangeEvent>.Unsubscribe(this);
            EventBus<ItemsHarvestedEvent>.Unsubscribe(this);
        }

        public void OnEvent(RequestPhaseChangeEvent evt)
        {
            switch (evt.TargetPhase)
            {
                case GamePhase.OnSettlement:
                    _stateMachine.ChangeState(_settlementState);
                    break;
                case GamePhase.OnScoop:
                    _stateMachine.ChangeState(_playingState);
                    break;
            }
        }

        public void OnEvent(ItemsHarvestedEvent evt)
        {
            // 컨텍스트에 수집된 재료 누적
            if (evt.NewHarvestedItems != null)
            {
                Context.HarvestedIngredients.AddRange(evt.NewHarvestedItems);
            }

            // 건지기 횟수 소진 및 UI 이벤트 브로드캐스팅
            if (Context.CurrentPhase == GamePhase.OnScoop)
            {
                Context.RemainScoopCount--;
                
                EventBus<ScoopCountUpdatedEvent>.Publish(new ScoopCountUpdatedEvent 
                { 
                    RemainScoopCount = Context.RemainScoopCount, 
                });

                // 횟수를 모두 소진하면 정산 페이즈로 전환 요청
                if (Context.RemainScoopCount <= 0)
                {
                    EventBus<RequestPhaseChangeEvent>.Publish(new RequestPhaseChangeEvent 
                    { 
                        TargetPhase = GamePhase.OnSettlement 
                    });
                }
            }
        }
    }
}
