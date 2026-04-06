using System.Collections;
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
    public class ScoopedIngredientGridCellUI : MonoBehaviour, IEventListener<IngredientScoreChangedEvent>, IEventListener<PlayScoreTrailEvent>
    {
        [Header("References")]
        [SerializeField] private Image ingredientIcon;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Animation")]
        [SerializeField] private float popScale = 1.2f;
        [SerializeField] private float popDuration = 0.15f;

        private RuntimeIngredient _boundIngredient;
        private Coroutine _popCoroutine;
        private Vector3 _originalScale = Vector3.one;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        public void Bind(RuntimeIngredient ingredient)
        {
            _boundIngredient = ingredient;
            transform.localScale = _originalScale;
            
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
            EventBus<PlayScoreTrailEvent>.Subscribe(this);

            // 레지스트리에 위치 등록 (자신의 RectTransform)
            if (ScoreUIRegistry.Instance != null)
            {
                ScoreUIRegistry.Instance.RegisterIngredient(_boundIngredient, transform as RectTransform);
            }
        }

        public void Unbind()
        {
            if (ScoreUIRegistry.Instance != null && _boundIngredient != null)
            {
                ScoreUIRegistry.Instance.UnregisterIngredient(_boundIngredient);
            }

            _boundIngredient = null;
            EventBus<IngredientScoreChangedEvent>.Unsubscribe(this);
            EventBus<PlayScoreTrailEvent>.Unsubscribe(this);
            
            if (_popCoroutine != null)
            {
                StopCoroutine(_popCoroutine);
                _popCoroutine = null;
            }
            transform.localScale = _originalScale;

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
                
                // 점수 상승 시 팝 애니메이션
                if (eventData.NewScore != eventData.OldScore && gameObject.activeInHierarchy)
                {
                    PlayPopAnimation();
                }
            }
        }

        public void OnEvent(PlayScoreTrailEvent eventData)
        {
            // 트레일 출발 시 출발 재료 팝 애니메이션
            if (eventData.SourceIngredient == _boundIngredient && gameObject.activeInHierarchy)
            {
                PlayPopAnimation();
            }
        }

        private void PlayPopAnimation()
        {
            if (_popCoroutine != null)
            {
                StopCoroutine(_popCoroutine);
            }
            _popCoroutine = StartCoroutine(PopCoroutine());
        }

        private IEnumerator PopCoroutine()
        {
            float elapsed = 0f;
            Vector3 targetScale = _originalScale * popScale;

            // 커지기
            while (elapsed < popDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (popDuration * 0.5f);
                transform.localScale = Vector3.Lerp(_originalScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;
            // 작아지기
            while (elapsed < popDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (popDuration * 0.5f);
                transform.localScale = Vector3.Lerp(targetScale, _originalScale, t);
                yield return null;
            }

            transform.localScale = _originalScale;
            _popCoroutine = null;
        }

        private void UpdateScoreUI(int score)
        {
            if (scoreText != null)
                scoreText.text = score.ToString();
        }

        private void OnDestroy()
        {
            EventBus<IngredientScoreChangedEvent>.Unsubscribe(this);
            EventBus<PlayScoreTrailEvent>.Unsubscribe(this);
        }
    }
}

