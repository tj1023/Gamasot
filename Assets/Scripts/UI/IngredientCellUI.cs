using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;
using Data;
using Interfaces;

namespace UI
{
    /// <summary>
    /// 그리드 내에서 개별 재료의 스프라이트와 현재 점수를 표시합니다.
    /// 풀링에 의해 재사용될 것을 가정하여 설계되었습니다.
    /// EventBus를 통해 자신의 점수가 변경되는지 감지합니다.
    /// </summary>
    public class IngredientCellUI : MonoBehaviour, IEventListener<IngredientScoreChangedEvent>, IEventListener<PlayScoreTrailEvent>
    {
        [Header("References")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Animation")]
        [SerializeField] private float popScale = 1.2f;
        [SerializeField] private float popDuration = 0.15f;

        [Header("Advanced Visual")]
        [SerializeField] private Color advancedScoreColor = new Color(1f, 0.84f, 0f, 1f);
        private Color _defaultScoreColor;

        private RuntimeIngredient _ingredient;
        private Coroutine _popRoutine;
        private Vector3 _defaultScale = Vector3.one;

        private void Awake()
        {
            _defaultScale = transform.localScale;
            if (scoreText != null) _defaultScoreColor = scoreText.color;
        }

        public void Bind(RuntimeIngredient ingredient)
        {
            _ingredient = ingredient;
            transform.localScale = _defaultScale;
            
            // 데이터 표기
            if (ingredient.OriginalData != null)
            {
                if (icon != null && ingredient.OriginalData.sprite != null)
                {
                    icon.sprite = ingredient.OriginalData.sprite;
                    icon.enabled = true;
                }
            }
            else
            {
                if (icon != null) icon.enabled = false;
            }

            if (scoreText != null)
            {
                scoreText.color = ingredient.IsAdvanced ? advancedScoreColor : _defaultScoreColor;
            }

            UpdateScore(ingredient.CurrentScore);

            // 이벤트 구독 (정산 로직 시 점수 변경 반영)
            EventBus<IngredientScoreChangedEvent>.Subscribe(this);
            EventBus<PlayScoreTrailEvent>.Subscribe(this);

            // 레지스트리에 위치 등록 (자신의 RectTransform)
            if (ScoreUIRegistry.Instance != null)
            {
                ScoreUIRegistry.Instance.RegisterIngredient(_ingredient, transform as RectTransform);
            }
        }

        public void Unbind()
        {
            if (ScoreUIRegistry.Instance != null && _ingredient != null)
            {
                ScoreUIRegistry.Instance.UnregisterIngredient(_ingredient);
            }

            _ingredient = null;
            EventBus<IngredientScoreChangedEvent>.Unsubscribe(this);
            EventBus<PlayScoreTrailEvent>.Unsubscribe(this);
            
            if (_popRoutine != null)
            {
                StopCoroutine(_popRoutine);
                _popRoutine = null;
            }
            transform.localScale = _defaultScale;

            // 비주얼 초기화
            if (icon != null) icon.enabled = false;
            if (scoreText != null) scoreText.text = "";
        }

        public void OnEvent(IngredientScoreChangedEvent eventData)
        {
            // 이 셀이 가리키는 재료의 점수 변경 이벤트인지 확인
            if (eventData.Ingredient == _ingredient)
            {
                if (scoreText != null)
                {
                    scoreText.color = _ingredient.IsAdvanced ? advancedScoreColor : _defaultScoreColor;
                }

                UpdateScore(eventData.NewScore);
                
                // 점수 상승 시 팝 애니메이션
                if (eventData.NewScore != eventData.OldScore && gameObject.activeInHierarchy)
                {
                    PlayPop();
                }
            }
        }

        public void OnEvent(PlayScoreTrailEvent eventData)
        {
            // 트레일 출발 시 출발 재료 팝 애니메이션
            if (eventData.SourceIngredient == _ingredient && gameObject.activeInHierarchy)
            {
                PlayPop();
            }
        }

        private void PlayPop()
        {
            if (_popRoutine != null)
            {
                StopCoroutine(_popRoutine);
            }
            _popRoutine = StartCoroutine(PopRoutine());
        }

        private IEnumerator PopRoutine()
        {
            float elapsed = 0f;
            Vector3 targetScale = _defaultScale * popScale;

            // 커지기
            while (elapsed < popDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (popDuration * 0.5f);
                transform.localScale = Vector3.Lerp(_defaultScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;
            // 작아지기
            while (elapsed < popDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (popDuration * 0.5f);
                transform.localScale = Vector3.Lerp(targetScale, _defaultScale, t);
                yield return null;
            }

            transform.localScale = _defaultScale;
            _popRoutine = null;
        }

        private void UpdateScore(int score)
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

