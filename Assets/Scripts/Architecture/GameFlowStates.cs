using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Gameplay;

namespace Architecture
{
    /// <summary>
    /// 게임 진행 중 수집/선택 페이즈를 담당하는 상태
    /// </summary>
    public class PlayingState : IState<GameContext>
    {
        public void Enter(GameContext context)
        {
            context.CurrentPhase = GamePhase.OnScoop;
            context.RemainScoopCount = context.MaxScoopCount;
            context.RemainTime = context.MaxScoopCount * 5f;
            
            EventBus<PhaseChangedEvent>.Publish(new PhaseChangedEvent { NewPhase = GamePhase.OnScoop });
            EventBus<TimerUpdatedEvent>.Publish(new TimerUpdatedEvent { RemainTime = context.RemainTime });
            EventBus<ScoopCountUpdatedEvent>.Publish(new ScoopCountUpdatedEvent { RemainScoopCount = context.RemainScoopCount });
            EventBus<RoundUpdatedEvent>.Publish(new RoundUpdatedEvent { CurrentRound = context.CurrentRound });
            EventBus<TotalScoreUpdatedEvent>.Publish(new TotalScoreUpdatedEvent { TotalScore = context.TotalScore });
        }

        public void Update(GameContext context)
        {
            if (context.RemainTime > 0)
            {
                context.RemainTime -= Time.deltaTime;
                EventBus<TimerUpdatedEvent>.Publish(new TimerUpdatedEvent { RemainTime = context.RemainTime });

                if (context.RemainTime <= 0)
                {
                    EventBus<RequestPhaseChangeEvent>.Publish(new RequestPhaseChangeEvent { TargetPhase = GamePhase.OnSettlement });
                }
            }
        }

        public void Exit()
        {
        }
    }

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

            // 1. 커맨드 큐 및 모디파이어 목록 준비
            Queue<ICommand> commandQueue = new Queue<ICommand>();
            List<ModifierInstance> synergyModifiers = new List<ModifierInstance>();

            // 1-1. 활성화된 모든 시너지 수집
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
                            if (effect != null)
                            {
                                synergyModifiers.Add(new ModifierInstance 
                                { 
                                    Modifier = effect, 
                                    Source = item 
                                });
                            }
                        }
                    }
                }
            }

            // 1-2. 시너지 우선순위에 따라 정렬하여 파이프라인 구성 (Priority 오름차순 => 먼저할수록 값이 작음)
            var sortedModifiers = synergyModifiers.OrderBy(m => m.Modifier.Priority).ToList();

            foreach (var modInstance in sortedModifiers)
            {
                var cmd = modInstance.Modifier.GenerateCommand(context, modInstance.Source);
                if (cmd != null)
                {
                    commandQueue.Enqueue(cmd);
                }
            }

            // 1-3. 모든 시너지 계산이 끝난 후 각 재료의 최종 점수를 RoundScore로 누계
            commandQueue.Enqueue(new AccumulateToRoundScoreCommand(context));

            // 모든 정산이 끝난 후 RoundScore -> TotalScore 넘기기
            commandQueue.Enqueue(new TransferRoundScoreToTotalCommand(context));

            // 2. 비동기 커맨드 순차 실행 (UI 애니메이션 적용)
            while (commandQueue.Count > 0)
            {
                var command = commandQueue.Dequeue();
                yield return command.ExecuteAsync();
            }
            
            // 3. 잠시 후 다음 라운드로 진행
            yield return new WaitForSeconds(1f);
            
            PrepareNextRound(context);
        }

        private void PrepareNextRound(GameContext context)
        {
            // TransferRoundScoreToTotalCommand에서 이미 TotalScore 반영 및 RoundScore 0 처리를 했으므로 그대로 진행
            context.CurrentRound++;
            context.HarvestedIngredients.Clear();

            EventBus<RequestPhaseChangeEvent>.Publish(new RequestPhaseChangeEvent { TargetPhase = GamePhase.OnScoop });
        }
    }
}
