using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Core;
using Data;
using Interfaces;
using Gameplay.Systems; // For GameManager

namespace UI
{
    public class TrinketSelectionUI : MonoBehaviour, 
        IEventListener<PhaseChangedEvent>
    {
        private bool _isSelecting;
        [Header("UI References")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private RectTransform container;
        [SerializeField] private TrinketInfoUI infoPrefab;
        [SerializeField] private Button skipButton;

        [Header("Selection Settings")]
        [SerializeField, Tooltip("한 번에 표시할 후보 수")]
        private int candidateCount = 3;

        [Header("Data Source")]
        [SerializeField, Tooltip("전체 장식품 후보 풀")]
        private TrinketData[] trinketPool;

        private TrinketInfoUI[] _cards;
        private bool _initialized;

        private void Awake()
        {
            InitializeCards();

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipButtonClicked);
            }
        }

        private void InitializeCards()
        {
            if (_initialized) return;
            _initialized = true;

            _cards = new TrinketInfoUI[candidateCount];

            for (int i = 0; i < candidateCount; i++)
            {
                var card = Instantiate(infoPrefab, container);

                if (!card.TryGetComponent<Button>(out var btn))
                {
                    btn = card.gameObject.AddComponent<Button>();
                }
                btn.onClick.AddListener(card.SelectThis);

                card.gameObject.SetActive(false);
                _cards[i] = card;
            }
        }

        private void OnEnable()
        {
            EventBus<PhaseChangedEvent>.Subscribe(this);
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(this);
        }

        public void OnEvent(PhaseChangedEvent eventData)
        {
            if (eventData.NewPhase == GamePhase.OnTrinketSelection)
            {
                ShowCandidates();
            }
            else
            {
                HideCandidates();
            }
        }

        private void ShowCandidates()
        {
            _isSelecting = false;
            if (trinketPool == null || trinketPool.Length == 0)
            {
                // 장식품 풀이 없으면 건너뛰기 처리
                OnTrinketSelected(null);
                return;
            }

            // 가용한 장식품 필터링 (최대 보유 횟수에 도달하지 않은 것들)
            var ctx = GameManager.Instance.Context;
            var availableTrinkets = ListPool<TrinketData>.Get();
            
            foreach (var trinket in trinketPool)
            {
                if (trinket == null) continue;

                int currentCount = 0;
                if (ctx.TrinketCounts.TryGetValue(trinket, out int c))
                {
                    currentCount = c;
                }

                if (currentCount < trinket.maxAccumulationCount)
                {
                    availableTrinkets.Add(trinket);
                }
            }

            if (availableTrinkets.Count == 0)
            {
                // 선택 가능한 장식품이 없으면 건너뛰기 처리
                ListPool<TrinketData>.Release(availableTrinkets);
                OnTrinketSelected(null);
                return;
            }

            if(rootPanel != null) rootPanel.SetActive(true);

            int count = Mathf.Min(candidateCount, availableTrinkets.Count);

            // 셔플
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(i, availableTrinkets.Count);
                (availableTrinkets[i], availableTrinkets[randomIndex]) = (availableTrinkets[randomIndex], availableTrinkets[i]);
            }

            for (int i = 0; i < count; i++)
            {
                TrinketData data = availableTrinkets[i];
                _cards[i].Setup(data);
                _cards[i].OnSelected = OnTrinketSelected;
                _cards[i].gameObject.SetActive(true);
            }

            // 안 쓰는 카드는 끄기
            for (int i = count; i < candidateCount; i++)
            {
                _cards[i].gameObject.SetActive(false);
            }

            ListPool<TrinketData>.Release(availableTrinkets);
        }

        private void HideCandidates()
        {
            if(rootPanel != null) rootPanel.SetActive(false);

            if (_cards == null) return;

            foreach (var t in _cards)
            {
                t.OnSelected = null;
                t.gameObject.SetActive(false);
            }
        }

        private void OnSkipButtonClicked()
        {
            OnTrinketSelected(null);
        }

        private void OnTrinketSelected(TrinketData data)
        {
            if (_isSelecting) return;
            _isSelecting = true;

            HideCandidates();
            
            EventBus<TrinketSelectedEvent>.Publish(new TrinketSelectedEvent
            {
                SelectedTrinket = data
            });
        }
    }
}
