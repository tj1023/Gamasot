using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Architecture;
using Gameplay;

namespace UI
{
    /// <summary>
    /// 그리드 내에서 개별 재료의 스프라이트와 현재 점수를 표시합니다.
    /// 풀링에 의해 재사용될 것을 가정하여 설계되었습니다.
    /// EventBus를 통해 자신의 점수가 변경되는지 감지합니다.
    /// </summary>
    public class ScoopedIngredientGridCellUI : MonoBehaviour, IEventListener<IngredientScoreChangedEvent>
    {
        [Header("References")]
        [SerializeField] private Image ingredientIcon;
        [SerializeField] private TextMeshProUGUI scoreText;

        private RuntimeIngredient _boundIngredient;

        public void Bind(RuntimeIngredient ingredient)
        {
            _boundIngredient = ingredient;
            
            // 데이터 표기
            if (ingredient.OriginalData != null)
            {
                if (ingredientIcon != null && ingredient.OriginalData.sprite != null)
                {
                    ingredientIcon.sprite = ingredient.OriginalData.sprite;
                    ingredientIcon.enabled = true;
                }
            }
            else
            {
                if (ingredientIcon != null) ingredientIcon.enabled = false;
            }

            UpdateScoreUI(ingredient.CurrentScore);

            // 이벤트 구독 (정산 로직 시 점수 변경 반영)
            EventBus<IngredientScoreChangedEvent>.Subscribe(this);
        }

        public void Unbind()
        {
            _boundIngredient = null;
            EventBus<IngredientScoreChangedEvent>.Unsubscribe(this);
            
            // 비주얼 초기화
            if (ingredientIcon != null) ingredientIcon.enabled = false;
            if (scoreText != null) scoreText.text = "";
        }

        public void OnEvent(IngredientScoreChangedEvent eventData)
        {
            // 이 셀이 가리키는 재료의 점수 변경 이벤트인지 확인
            if (eventData.Ingredient == _boundIngredient)
            {
                UpdateScoreUI(eventData.NewScore);
                
                // TODO: 점수 상승 애니메이션이나 반짝임 이펙트
            }
        }

        private void UpdateScoreUI(int score)
        {
            if (scoreText != null)
                scoreText.text = score.ToString();
        }

        private void OnDestroy()
        {
            EventBus<IngredientScoreChangedEvent>.Unsubscribe(this);
        }
    }
}
