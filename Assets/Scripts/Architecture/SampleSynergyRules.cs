using System;
using UnityEngine;

namespace Architecture
{
    // [SerializeReference]를 통해 인스펙터 메뉴에 표시되려면
    // 인터페이스(ITrigger, IEffect)를 구현하면서 [Serializable] 속성이 있어야 합니다.
    
    // --- [예시] Trigger ---

    /// <summary>
    /// 특정 타입의 재료가 덱(Context)에 X개 이상 있을 때 발동하는 조건
    /// </summary>
    [Serializable]
    public class RequireIngredientTypeTrigger : ITrigger
    {
        [Tooltip("조건으로 요구할 재료 타입")]
        public IngredientType targetType;
        [Tooltip("필요한 최소 개수")]
        public int requiredCount;

        public bool Evaluate(GameContext context)
        {
            if (context == null || context.CurrentIngredients == null) return false;

            int count = 0;
            foreach (var item in context.CurrentIngredients)
            {
                if (item.type == targetType) count++;
            }
            return count >= requiredCount;
        }
    }

    // --- [예시] Effect ---

    /// <summary>
    /// 단순 보너스 점수 추가 효과
    /// </summary>
    [Serializable]
    public class AddScoreEffect : IEffect
    {
        [Tooltip("가산될 보너스 스코어")]
        public int bonusScore;

        public void Apply(GameContext context)
        {
            if (context == null) return;
            context.CurrentScore += bonusScore;
            Debug.Log($"[Effect] 보너스 효과 발동! +{bonusScore}점. (현재 점수: {context.CurrentScore})");
        }
    }
}
