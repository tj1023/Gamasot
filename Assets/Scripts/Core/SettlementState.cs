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

        private readonly Queue<ICommand> _commandQueue = new Queue<ICommand>();
        private readonly List<ModifierInstance> _synergyModifiers = new List<ModifierInstance>();

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

            // 1. 커맨드 큐 및 모디파이어 목록 준비
            _commandQueue.Clear();
            _synergyModifiers.Clear();

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
                                _synergyModifiers.Add(new ModifierInstance 
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
            _synergyModifiers.Sort((a, b) => a.Modifier.Priority.CompareTo(b.Modifier.Priority));

            foreach (var modInstance in _synergyModifiers)
            {
                var cmd = modInstance.Modifier.GenerateCommand(context, modInstance.Source);
                if (cmd != null)
                {
                    _commandQueue.Enqueue(cmd);
                }
            }

            // 1-3. 모든 시너지 계산이 끝난 후 각 재료의 최종 점수를 RoundScore로 집계
            _commandQueue.Enqueue(new TallyRoundScoreCommand(context));

            // 모든 정산이 끝난 후 RoundScore -> TotalScore 확정
            _commandQueue.Enqueue(new FinalizeRoundScoreCommand(context));

            // 2. 비동기 커맨드 순차 실행 (UI 애니메이션 적용)
            float startTime = Time.unscaledTime;
            while (_commandQueue.Count > 0)
            {
                float elapsedTime = Time.unscaledTime - startTime;
                if (elapsedTime > 10f)
                {
                    Time.timeScale = 2f;
                }

                var command = _commandQueue.Dequeue();
                yield return command.ExecuteAsync();
            }
            
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

            EventBus<RequestPhaseChangeEvent>.Publish(context.CurrentRound % 3 == 0
                ? new RequestPhaseChangeEvent { TargetPhase = GamePhase.OnTrinketSelection }
                : new RequestPhaseChangeEvent { TargetPhase = GamePhase.OnSelection });
        }
    }

}
