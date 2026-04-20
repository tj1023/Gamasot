using System.Collections.Generic;
using UnityEngine;
using Data;

namespace UI
{
    /// <summary>
    /// UI 요소들(재료 셀, 총점 텍스트 등)의 월드 좌표를 추적하여 이펙트 생성을 돕는 레지스트리입니다.
    /// 싱글톤 패턴을 사용하여 쉽게 접근하도록 합니다.
    /// </summary>
    public class ScoreUIRegistry : MonoBehaviour
    {
        public static ScoreUIRegistry Instance { get; private set; }

        private readonly Dictionary<RuntimeIngredient, Transform> _cells = new();
        
        [Header("Global Score Texts")]
        [SerializeField] private RectTransform totalScoreRect;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterTotalScore(RectTransform totalScore)
        {
           totalScoreRect = totalScore;
        }

        public void RegisterIngredient(RuntimeIngredient ingredient, Transform ingredientTransform)
        {
            if (ingredient != null && ingredientTransform != null)
            {
                _cells[ingredient] = ingredientTransform;
            }
        }

        public void UnregisterIngredient(RuntimeIngredient ingredient)
        {
            if (ingredient != null)
            {
                _cells.Remove(ingredient);
            }
        }

        /// <summary>
        /// Transform의 중심점을 월드 좌표로 반환합니다. RectTransform일 경우 rect의 중심을 보정합니다.
        /// </summary>
        private Vector3 GetWorldPosition(Transform targetTransform)
        {
            if (targetTransform == null) return Vector3.zero;

            if (targetTransform is RectTransform rectTransform)
            {
                return rectTransform.TransformPoint(rectTransform.rect.center);
            }
            
            return targetTransform.position;
        }

        public bool TryGetIngredientPosition(RuntimeIngredient ingredient, out Vector3 position)
        {
            if (ingredient != null && _cells.TryGetValue(ingredient, out var rect))
            {
                position = GetWorldPosition(rect);
                return true;
            }
            position = Vector3.zero;
            return false;
        }

        public bool TryGetTotalScorePosition(out Vector3 position)
        {
            if (totalScoreRect != null)
            {
                position = GetWorldPosition(totalScoreRect);
                return true;
            }
            position = Vector3.zero;
            return false;
        }
    }
}
