using System;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Interfaces;

namespace Data
{
    /// <summary>
    /// 개별 시너지 조건을 묶어 관리하는 직렬화 가능(Serializable) 구조입니다.
    /// </summary>
    [Serializable]
    public class SynergyData
    {
        // 다형성을 보장하는 유니티 내장 직렬화. 
        // [SerializeReference]를 통해 ITrigger 인터페이스를 구현한 어떤 클래스든 에디터 상에서 할당 가능합니다.
        // [SubclassSelector]를 통해 자식 클래스(구현체)들을 드롭다운 메뉴로 인스펙터에서 선택할 수 있습니다.
        [SerializeReference, SubclassSelector]
        [Header("발동 조건")]
        public ITrigger trigger;

        [SerializeReference, SubclassSelector]
        [Header("발동 효과 목록")]
        public List<IEffect> effects = new();

        /// <summary>
        /// 시너지 검사 및 즉시 발동을 총괄합니다.
        /// 건지기(Scoop) 시점처럼 UI 애니메이션 없이 즉각 점수 반영이 필요한 경우에 사용됩니다.
        /// 정산(Settlement) 시에는 SettlementState의 코루틴 파이프라인을 통해 비동기로 처리됩니다.
        /// </summary>
        public void EvaluateAndApply(GameContext context)
        {
            if (trigger == null || !trigger.Evaluate(context)) return;

            foreach (var effect in effects)
            {
                if (effect == null) continue;

                var cmd = effect.GenerateCommand(context, context.Source);
                if (cmd == null) continue;

                // 의도된 동기 실행: 코루틴 열거자를 즉시 소비하여 애니메이션/대기 없이 점수만 반영
                var enumerator = cmd.ExecuteAsync();
                while (enumerator != null && enumerator.MoveNext()) { }
            }
        }

        /// <summary>
        /// 비동기 처리를 위해 시너지 효과 커맨드들을 생성하여 큐에 적재합니다.
        /// </summary>
        public void EvaluateAndEnqueueCommands(GameContext context, Queue<ICommand> commandQueue)
        {
            if (trigger == null || !trigger.Evaluate(context)) return;

            foreach (var effect in effects)
            {
                if (effect == null) continue;

                var cmd = effect.GenerateCommand(context, context.Source);
                if (cmd != null)
                {
                    commandQueue.Enqueue(cmd);
                }
            }
        }
    }
}
