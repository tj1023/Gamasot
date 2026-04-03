using System;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture
{
    /// <summary>
    /// 개별 시너지 조건을 묶어 관리하는 직렬화 가능(Serializable) 구조입니다.
    /// </summary>
    [Serializable]
    public class SynergyData
    {
        // 다형성을 보장하는 유니티 내장 직렬화. 
        // [SerializeReference]를 통해 ITrigger 인터페이스를 구현한 어떤 클래스든 에디터 상에서 할당 가능합니다.
        [SerializeReference]
        [Header("발동 조건")]
        public ITrigger trigger;

        [SerializeReference]
        [Header("발동 효과 목록")]
        public List<IEffect> effects = new();

        /// <summary>
        /// 시너지 검사 및 발동을 총괄
        /// </summary>
        public void EvaluateAndApply(GameContext context)
        {
            if (trigger != null && trigger.Evaluate(context))
            {
                foreach (var effect in effects)
                {
                    effect?.Apply(context);
                }
            }
        }
    }
}
