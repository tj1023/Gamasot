using UnityEngine;
using TMPro;

using Core;
using Interfaces;

namespace UI
{
    /// <summary>
    /// 상단 또는 화면에 타이머, 라운드 정보, 점수 등을 표시하는 UI 컴포넌트입니다.
    /// </summary>
    public class GameInfoUI : MonoBehaviour, 
        IEventListener<TimerUpdatedEvent>,
        IEventListener<ScoopCountUpdatedEvent>,
        IEventListener<TotalScoreUpdatedEvent>,
        IEventListener<RoundScoreUpdatedEvent>,
        IEventListener<RoundUpdatedEvent>
    {
        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoopCountText;
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI roundScoreText;
        [SerializeField] private TextMeshProUGUI roundText;

        private int _lastSeconds = -1;

        private void Start()
        {
            if (ScoreUIRegistry.Instance != null)
            {
                RectTransform totalRect = totalScoreText != null ? totalScoreText.rectTransform : null;

                if (totalRect != null)
                {
                    ScoreUIRegistry.Instance.RegisterTotalScore(totalRect);
                }
            }

            // 시작 시 라운드 점수 텍스트 비활성화
            if (roundScoreText != null)
            {
                roundScoreText.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            EventBus<TimerUpdatedEvent>.Subscribe(this);
            EventBus<ScoopCountUpdatedEvent>.Subscribe(this);
            EventBus<TotalScoreUpdatedEvent>.Subscribe(this);
            EventBus<RoundScoreUpdatedEvent>.Subscribe(this);
            EventBus<RoundUpdatedEvent>.Subscribe(this);
        }

        private void OnDisable()
        {
            EventBus<TimerUpdatedEvent>.Unsubscribe(this);
            EventBus<ScoopCountUpdatedEvent>.Unsubscribe(this);
            EventBus<TotalScoreUpdatedEvent>.Unsubscribe(this);
            EventBus<RoundScoreUpdatedEvent>.Unsubscribe(this);
            EventBus<RoundUpdatedEvent>.Unsubscribe(this);
        }

        public void OnEvent(TimerUpdatedEvent eventData)
        {
            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(Mathf.Max(0, eventData.RemainTime));
                if (_lastSeconds != seconds)
                {
                    _lastSeconds = seconds;
                    timerText.text = $"{seconds}s |";
                }
            }
        }

        public void OnEvent(ScoopCountUpdatedEvent eventData)
        {
            if (scoopCountText != null)
            {
                scoopCountText.text = $" 남은 횟수: {eventData.RemainScoopCount}";
            }
        }

        public void OnEvent(TotalScoreUpdatedEvent eventData)
        {
            if (totalScoreText != null)
            {
                totalScoreText.text = $"{eventData.TotalScore}";
            }
        }

        public void OnEvent(RoundScoreUpdatedEvent eventData)
        {
            if (roundScoreText != null)
            {
                // 점수가 0보다 클 때만 활성화 (정산 연출 시작 시점부터 활성화, 끝나면 비활성화)
                bool shouldBeActive = eventData.RoundScore > 0;
                
                if (roundScoreText.gameObject.activeSelf != shouldBeActive)
                {
                    roundScoreText.gameObject.SetActive(shouldBeActive);
                }

                if (shouldBeActive)
                {
                    roundScoreText.text = $"+{eventData.RoundScore}";
                }
            }
        }

        public void OnEvent(RoundUpdatedEvent eventData)
        {
            if (roundText != null)
            {
                roundText.text = $"Round {eventData.CurrentRound}";
            }
        }
    }
}
