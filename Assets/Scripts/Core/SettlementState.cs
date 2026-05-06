using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Gameplay.Commands;
using Gameplay.Systems;
using Interfaces;

namespace Core
{
    /// <summary>
    /// 파이프라인 패턴과 커맨드 패턴을 활용하여 정산을 비동기로 처리하는 상태
    /// </summary>
    public class SettlementState : IState<GameContext>
    {
        private Coroutine _settlementCoroutine;

        public void Enter(GameContext context)
        {
            context.CurrentPhase = GamePhase.OnSettlement;
            EventBus<PhaseChangedEvent>.Publish(new PhaseChangedEvent { NewPhase = GamePhase.OnSettlement });
            
            _settlementCoroutine = GameManager.Instance.StartCoroutine(ProcessSettlementPipeline(context));
        }

        public void Update(GameContext context)
        {
        }

        public void Exit()
        {
            if (_settlementCoroutine != null)
            {
                GameManager.Instance.StopCoroutine(_settlementCoroutine);
                _settlementCoroutine = null;
            }
            
            // 만약 상태를 벗어날 때 배속이 적용되어 있다면 원래대로 복구
            Time.timeScale = 1f;
        }

        private struct ModifierInstance
        {
            public IEffect Modifier;
            public RuntimeIngredient Source;
        }

        private IEnumerator ProcessSettlementPipeline(GameContext context)
        {
            context.RoundScore = 0;
            EventBus<RoundScoreUpdatedEvent>.Publish(new RoundScoreUpdatedEvent { RoundScore = context.RoundScore });

            // 1. 동적 시너지 처리 파이프라인
            var processedEffects = new HashSet<(RuntimeIngredient, IEffect)>();
            float startTime = Time.unscaledTime;
            
            while (true)
            {
                int minPriority = int.MaxValue;
                ModifierInstance? nextMod = null;

                foreach (var item in context.HarvestedIngredients)
                {
                    if (item == null || item.OriginalData == null) continue;
                    context.Source = item;

                    foreach (var synergy in item.ActiveSynergies)
                    {
                        if (synergy is { trigger: not null } && synergy.trigger.Evaluate(context))
                        {
                            foreach (var effect in synergy.effects)
                            {
                                if (effect != null && !processedEffects.Contains((item, effect)))
                                {
                                    if (effect.Priority < minPriority)
                                    {
                                        minPriority = effect.Priority;
                                        nextMod = new ModifierInstance { Modifier = effect, Source = item };
                                    }
                                }
                            }
                        }
                    }
                }

                if (!nextMod.HasValue)
                    break; // 처리할 시너지 없음

                var modifier = nextMod.Value.Modifier;
                var source = nextMod.Value.Source;
                processedEffects.Add((source, modifier));

                float elapsedTime = Time.unscaledTime - startTime;
                if (elapsedTime > 8f)
                {
                    Time.timeScale = 2f;
                }

                var cmd = modifier.GenerateCommand(context, source);
                if (cmd != null)
                {
                    EventBus<SynergyActivatedEvent>.Publish(new SynergyActivatedEvent());
                    yield return cmd.ExecuteAsync();
                }
            }

            // 1-3. 모든 시너지 계산이 끝난 후 각 재료의 최종 점수를 RoundScore로 집계
            yield return new TallyRoundScoreCommand(context).ExecuteAsync();

            // 모든 정산이 끝난 후 RoundScore -> TotalScore 확정
            yield return new FinalizeRoundScoreCommand(context).ExecuteAsync();
            
            Time.timeScale = 1f;
            
            // 3. 잠시 후 다음 라운드로 진행
            yield return WaitCache.Seconds(1f);
            
            PrepareNextRound(context);
        }

        private void PrepareNextRound(GameContext context)
        {
            // FinalizeRoundScoreCommand에서 이미 TotalScore 반영 및 RoundScore 0 처리를 했으므로 그대로 진행
            context.CurrentRound++;
            context.HarvestedIngredients.Clear();

            // 최대 라운드에 도달하면 게임 오버
            if (GameContext.MaxRound > 0 && context.CurrentRound > GameContext.MaxRound)
            {
                EventBus<RequestPhaseChangeEvent>.Publish(new RequestPhaseChangeEvent { TargetPhase = GamePhase.GameOver });
                return;
            }

            EventBus<RequestPhaseChangeEvent>.Publish(context.CurrentRound % 3 == 0
                ? new RequestPhaseChangeEvent { TargetPhase = GamePhase.OnTrinketSelection }
                : new RequestPhaseChangeEvent { TargetPhase = GamePhase.OnSelection });
        }
    }

}
