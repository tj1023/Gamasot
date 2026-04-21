using System.Linq;
using UnityEngine;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Systems
{
    public class TrinketManager : MonoBehaviour, 
        IEventListener<TrinketSelectedEvent>,
        IEventListener<PhaseChangedEvent>,
        IEventListener<ItemsHarvestedEvent>,
        IEventListener<DiscoverItemSelectedEvent>
    {
        [Header("Data References")]
        [SerializeField, Tooltip("모든 재료 데이터를 할당해주세요 (무작위 보상용)")]
        private FoodIngredientData[] allIngredients;

        private int _discoverRemainingCount;
        private TrinketEffectType _currentDiscoverType;

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

            if (IsDiscoverEffect(trinket.effectType))
            {
                StartDiscoverProcess(ctx, trinket.effectType);
            }
            else
            {
                ApplyInstantAndPassiveEffects(ctx, trinket);
                ProceedToSelectionPhase();
            }
        }

        private void ProceedToSelectionPhase()
        {
            EventBus<RequestPhaseChangeEvent>.Publish(new RequestPhaseChangeEvent { TargetPhase = GamePhase.OnSelection });
        }

        private bool IsDiscoverEffect(TrinketEffectType effectType)
        {
            return effectType == TrinketEffectType.DiscoverLegendaryCard ||
                   effectType == TrinketEffectType.DiscoverTwoRareCards ||
                   effectType == TrinketEffectType.DiscoverOwnedRareOrHigherCard;
        }

        private void StartDiscoverProcess(GameContext ctx, TrinketEffectType effectType)
        {
            _currentDiscoverType = effectType;
            _discoverRemainingCount = effectType == TrinketEffectType.DiscoverTwoRareCards ? 2 : 1;
            
            ProcessNextDiscover(ctx);
        }

        private void ProcessNextDiscover(GameContext ctx)
        {
            if (_discoverRemainingCount <= 0)
            {
                ProceedToSelectionPhase();
                return;
            }

            var candidates = GetDiscoverCandidates(ctx, _currentDiscoverType);
            if (candidates == null || candidates.Count == 0)
            {
                // 더 이상 발견할 카드가 없으면 스킵
                _discoverRemainingCount = 0;
                ProceedToSelectionPhase();
                return;
            }

            EventBus<RequestDiscoverEvent>.Publish(new RequestDiscoverEvent { Candidates = candidates });
        }

        private System.Collections.Generic.List<FoodIngredientData> GetDiscoverCandidates(GameContext ctx, TrinketEffectType effectType)
        {
            var candidates = new System.Collections.Generic.List<FoodIngredientData>();
            System.Collections.Generic.IEnumerable<FoodIngredientData> pool = null;

            switch (effectType)
            {
                case TrinketEffectType.DiscoverLegendaryCard:
                    pool = allIngredients.Where(x => x.rarity == Rarity.Legendary);
                    break;
                case TrinketEffectType.DiscoverTwoRareCards:
                    pool = allIngredients.Where(x => x.rarity == Rarity.Rare);
                    break;
                case TrinketEffectType.DiscoverOwnedRareOrHigherCard:
                    pool = ctx.SelectedIngredients.Where(x => x.rarity >= Rarity.Rare).Distinct();
                    break;
            }

            if (pool != null)
            {
                candidates.AddRange(pool);
                // 중복 방지를 위한 셔플 및 최대 3개 선택 로직은 IngredientSelectionUI에서 처리하지만, 
                // 전체 리스트를 넘겨서 그쪽에서 처리하도록 합니다.
            }

            return candidates;
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

        private void ApplyInstantAndPassiveEffects(GameContext ctx, TrinketData trinket)
        {
            switch (trinket.effectType)
            {
                case TrinketEffectType.DuplicateOwnedLegendaryCard:
                    DuplicateOwnedCardOfRarity(ctx, Rarity.Legendary, 1);
                    break;
                case TrinketEffectType.DuplicateAllOwnedRareCards:
                    DuplicateAllOwnedCardsOfRarity(ctx, Rarity.Rare);
                    break;
                case TrinketEffectType.IncreaseMaxSelectedIngredients:
                    ctx.MaxSelectedIngredients++;
                    break;
                case TrinketEffectType.PuddingEffect:
                    ctx.HasPuddingEffect = true;
                    break;
                case TrinketEffectType.IncreaseMaxScoopCount:
                    ctx.MaxScoopCount++;
                    // 바로 다음 라운드(또는 현재)에 적용될 수 있도록 Remain도 올려줌
                    ctx.RemainScoopCount++; 
                    break;
                case TrinketEffectType.IncreaseScoopRadius:
                    ctx.ScoopRadiusModifier += 0.5f; // 반경 50% 증가
                    break;
            }
        }

        public void OnEvent(PhaseChangedEvent evt)
        {
            var ctx = GameManager.Instance.Context;
            
            // 라운드 스쿱 시작 시 (재료 스폰 직후) 작동하는 효과들
            if (evt.NewPhase == GamePhase.OnScoop)
            {
                // 팝콘부: 라운드 시작 시, 일반 재료 2개를 고급 재료로 변환
                int popcornCount = GetTrinketCount(ctx, TrinketEffectType.PopcornEffect);
                if (popcornCount > 0)
                {
                    var im = FindAnyObjectByType<IngredientManager>();
                    if (im != null)
                    {
                        im.TransformRandomToAdvanced(2 * popcornCount);
                    }
                }

                // 츠바부: 라운드 시작 시 (OnSelection 진입 전), 정산 패널에 '홍탕 츠바' 1개 추가
                int tsubaCount = GetTrinketCount(ctx, TrinketEffectType.TsubaEffect);
                if (tsubaCount > 0)
                {
                    FoodIngredientData tsubaData = allIngredients?.FirstOrDefault(x => x.ingredientName.Contains("츠바"));
                    if (tsubaData != null)
                    {
                        var tsubaItems = new System.Collections.Generic.List<RuntimeIngredient>();
                        for (int i = 0; i < tsubaCount; i++)
                        {
                            tsubaItems.Add(new RuntimeIngredient(tsubaData));
                        }
                        
                        EventBus<ItemsHarvestedEvent>.Publish(new ItemsHarvestedEvent
                        {
                            NewHarvestedItems = tsubaItems,
                            IsBonus = true
                        });
                    }
                }
            }
        }

        public void OnEvent(ItemsHarvestedEvent evt)
        {
            var ctx = GameManager.Instance.Context;
            var newItems = evt.NewHarvestedItems;
            if (newItems == null || newItems.Count == 0) return;

            var im = FindAnyObjectByType<IngredientManager>();
            if (im == null) return;

            // 우유부: 건질때, 같은 재료 2개를 동시에 건지면 pot 안에 해당 재료 1개 추가
            int milkCount = GetTrinketCount(ctx, TrinketEffectType.MilkEffect);
            if (milkCount > 0)
            {
                var groups = newItems.GroupBy(x => x.OriginalData);
                foreach (var group in groups)
                {
                    if (group.Count() >= 2)
                    {
                        for(int i = 0; i < milkCount; i++)
                        {
                            im.SpawnIngredient(group.Key);
                        }
                    }
                }
            }

            // 소다부: 건질때, 4개 이상을 건지면 보유재료 중 랜덤으로 1개를 pot 안에 추가
            int sodaCount = GetTrinketCount(ctx, TrinketEffectType.SodaEffect);
            if (sodaCount > 0 && newItems.Count >= 4)
            {
                if (ctx.SelectedIngredients.Count > 0)
                {
                    for (int i = 0; i < sodaCount; i++)
                    {
                        var randomOwned = ctx.SelectedIngredients[Random.Range(0, ctx.SelectedIngredients.Count)];
                        im.SpawnIngredient(randomOwned);
                    }
                }
            }
        }

        private int GetTrinketCount(GameContext ctx, TrinketEffectType effectType)
        {
            int count = 0;
            foreach (var kvp in ctx.TrinketCounts)
            {
                if (kvp.Key.effectType == effectType)
                {
                    count += kvp.Value;
                }
            }
            return count;
        }

        #region Helper Methods
        private void DuplicateOwnedCardOfRarity(GameContext ctx, Rarity rarity, int count)
        {
            var pool = ctx.SelectedIngredients.Where(x => x.rarity == rarity).ToList();
            if (pool.Count == 0) return;

            for (int i = 0; i < count; i++)
            {
                ctx.SelectedIngredients.Add(pool[Random.Range(0, pool.Count)]);
            }
        }

        private void DuplicateAllOwnedCardsOfRarity(GameContext ctx, Rarity rarity)
        {
            var pool = ctx.SelectedIngredients.Where(x => x.rarity == rarity).ToList();
            foreach (var item in pool)
            {
                ctx.SelectedIngredients.Add(item); // 각각 복사
            }
        }
        #endregion
    }
}
