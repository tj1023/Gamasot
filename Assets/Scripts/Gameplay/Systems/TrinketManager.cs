using UnityEngine;
using Core;
using Data;
using Interfaces;
using Gameplay.TrinketEffects;

namespace Gameplay.Systems
{
    /// <summary>
    /// 장식품(Trinket) 효과의 라이프사이클을 관리합니다.
    /// 각 효과의 로직은 TrinketEffect 구현체에 위임합니다.
    /// </summary>
    public class TrinketManager : MonoBehaviour, 
        IEventListener<TrinketSelectedEvent>,
        IEventListener<PhaseChangedEvent>,
        IEventListener<ItemsHarvestedEvent>,
        IEventListener<DiscoverItemSelectedEvent>
    {
        [Header("References")]
        [SerializeField, Tooltip("IngredientManager를 인스펙터에서 할당해주세요")]
        private IngredientManager ingredientManager;

        [SerializeField, Tooltip("모든 재료 데이터를 할당해주세요 (무작위 보상용)")]
        private FoodIngredientData[] allIngredients;

        private TrinketServices _services;
        private TrinketEffect _currentDiscoverEffect;
        private int _discoverRemainingCount;

        private void Awake()
        {
            _services = new TrinketServices(ingredientManager, allIngredients);
        }

        private void OnEnable()
        {
            EventBus<TrinketSelectedEvent>.Subscribe(this);
            EventBus<PhaseChangedEvent>.Subscribe(this);
            EventBus<ItemsHarvestedEvent>.Subscribe(this);
            EventBus<DiscoverItemSelectedEvent>.Subscribe(this);
        }

        private void OnDisable()
        {
            EventBus<TrinketSelectedEvent>.Unsubscribe(this);
            EventBus<PhaseChangedEvent>.Unsubscribe(this);
            EventBus<ItemsHarvestedEvent>.Unsubscribe(this);
            EventBus<DiscoverItemSelectedEvent>.Unsubscribe(this);
        }

        #region 장식품 선택 처리

        public void OnEvent(TrinketSelectedEvent evt)
        {
            if (evt.SelectedTrinket == null)
            {
                ProceedToSelectionPhase();
                return;
            }
            
            var ctx = GameManager.Instance.Context;
            var trinket = evt.SelectedTrinket;
            
            // 보유 장식품 목록에 추가
            if (!ctx.TrinketCounts.TryAdd(trinket, 1))
            {
                ctx.TrinketCounts[trinket]++;
            }

            EventBus<OwnedTrinketsUpdatedEvent>.Publish(new OwnedTrinketsUpdatedEvent());

            // 효과가 없으면 바로 다음 페이즈로
            if (trinket.effect == null)
            {
                ProceedToSelectionPhase();
                return;
            }

            if (trinket.effect.IsDiscoverEffect)
            {
                StartDiscoverProcess(ctx, trinket.effect);
            }
            else
            {
                trinket.effect.OnAcquire(ctx);
                ProceedToSelectionPhase();
            }
        }

        private void ProceedToSelectionPhase()
        {
            EventBus<RequestPhaseChangeEvent>.Publish(new RequestPhaseChangeEvent { TargetPhase = GamePhase.OnSelection });
        }

        #endregion

        #region Discover 흐름

        private void StartDiscoverProcess(GameContext ctx, TrinketEffect effect)
        {
            _currentDiscoverEffect = effect;
            _discoverRemainingCount = effect.DiscoverCount;
            
            ProcessNextDiscover(ctx);
        }

        private void ProcessNextDiscover(GameContext ctx)
        {
            if (_discoverRemainingCount <= 0)
            {
                _currentDiscoverEffect = null;
                ProceedToSelectionPhase();
                return;
            }

            var candidates = _currentDiscoverEffect?.GetDiscoverCandidates(ctx, allIngredients);
            if (candidates == null || candidates.Count == 0)
            {
                _discoverRemainingCount = 0;
                _currentDiscoverEffect = null;
                ProceedToSelectionPhase();
                return;
            }

            EventBus<RequestDiscoverEvent>.Publish(new RequestDiscoverEvent { Candidates = candidates });
        }

        public void OnEvent(DiscoverItemSelectedEvent evt)
        {
            var ctx = GameManager.Instance.Context;
            if (evt.SelectedData != null)
            {
                ctx.SelectedIngredients.Add(evt.SelectedData);
            }

            _discoverRemainingCount--;
            
            GameManager.Instance.CheckExcessIngredients(() => 
            {
                ProcessNextDiscover(ctx);
            });
        }

        #endregion

        #region 라운드 시작 효과 (OnScoop)

        public void OnEvent(PhaseChangedEvent evt)
        {
            if (evt.NewPhase != GamePhase.OnScoop) return;

            StartCoroutine(DelayOnRoundStart());
        }

        private System.Collections.IEnumerator DelayOnRoundStart()
        {
            // IngredientManager가 같은 PhaseChangedEvent에서 스폰을 완료하도록 1프레임 대기
            yield return null;

            var ctx = GameManager.Instance.Context;
            foreach (var (trinket, count) in ctx.TrinketCounts)
            {
                trinket.effect?.OnRoundStart(ctx, count, _services);
            }
        }

        #endregion

        #region 건지기 효과 (Harvest)

        public void OnEvent(ItemsHarvestedEvent evt)
        {
            var newItems = evt.NewHarvestedItems;
            if (newItems == null || newItems.Count == 0) return;

            var ctx = GameManager.Instance.Context;

            foreach (var (trinket, count) in ctx.TrinketCounts)
            {
                trinket.effect?.OnHarvest(ctx, newItems, count, _services);
            }
        }

        #endregion
    }
}
