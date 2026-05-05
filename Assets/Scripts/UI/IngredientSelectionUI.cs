using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Core;
using Data;
using Interfaces;

namespace UI
{
    /// <summary>
    /// 매 라운드 시작 전(OnSelection 페이즈) 무작위 재료 3개를 IngredientInfoUI 카드로 표시하고,
    /// 플레이어가 클릭하면 선택 이벤트를 발행합니다.
    /// 카드는 미리 생성해두고 활성화/비활성화로 관리합니다.
    /// </summary>
    public class IngredientSelectionUI : MonoBehaviour, 
        IEventListener<PhaseChangedEvent>,
        IEventListener<RequestDeleteExcessEvent>,
        IEventListener<RequestDiscoverEvent>
    {
        [Header("UI References")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private RectTransform container;
        [SerializeField] private IngredientInfoUI infoPrefab;
        [SerializeField] private GameObject darkBackground;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button showAdvancedToggleButton;
        [SerializeField] private GameObject advancedCheckmark;

        [Header("Selection Settings")]
        [SerializeField, Tooltip("한 번에 표시할 후보 재료 수")]
        private int candidateCount = 3;

        [Header("Rarity Weights")]
        [SerializeField, Tooltip("Common 출현 가중치")] private float commonWeight = 6f;
        [SerializeField, Tooltip("Rare 출현 가중치")] private float rareWeight = 3f;
        [SerializeField, Tooltip("Legendary 출현 가중치")] private float legendaryWeight = 1f;

        [Header("Data Source")]
        [SerializeField, Tooltip("전체 재료 후보 풀")]
        private FoodIngredientData[] ingredientPool;

        private IngredientInfoUI[] _cards;
        private bool _initialized;
        private bool _isDiscoverMode;
        private bool _isShowingAdvanced;

        private void Awake()
        {
            InitializeCards();

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipButtonClicked);
            }
            if (showAdvancedToggleButton != null)
            {
                showAdvancedToggleButton.onClick.AddListener(ToggleAdvancedMode);
            }
        }

        private void ToggleAdvancedMode()
        {
            _isShowingAdvanced = !_isShowingAdvanced;
            
            if (advancedCheckmark != null)
            {
                advancedCheckmark.SetActive(_isShowingAdvanced);
            }

            if (_cards != null)
            {
                foreach (var card in _cards)
                {
                    if (card != null && card.gameObject.activeSelf)
                    {
                        card.ToggleAdvancedMode(_isShowingAdvanced);
                    }
                }
            }
        }

        /// <summary>
        /// candidateCount만큼 카드를 미리 생성하고 비활성화합니다.
        /// </summary>
        private void InitializeCards()
        {
            if (_initialized) return;
            _initialized = true;

            _cards = new IngredientInfoUI[candidateCount];

            for (int i = 0; i < candidateCount; i++)
            {
                var card = Instantiate(infoPrefab, container);

                // Button 컴포넌트가 없으면 추가
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
            EventBus<RequestDeleteExcessEvent>.Subscribe(this);
            EventBus<RequestDiscoverEvent>.Subscribe(this);
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(this);
            EventBus<RequestDeleteExcessEvent>.Unsubscribe(this);
            EventBus<RequestDiscoverEvent>.Unsubscribe(this);
        }

        public void OnEvent(PhaseChangedEvent eventData)
        {
            switch (eventData.NewPhase)
            {
                case GamePhase.OnSelection:
                    ShowCandidates();
                    break;
                case GamePhase.OnScoop:
                case GamePhase.OnSettlement:
                case GamePhase.Ready:
                case GamePhase.GameOver:
                    HideCandidates();
                    break;
            }
        }

        public void OnEvent(RequestDeleteExcessEvent eventData)
        {
            HideCandidates();
        }

        public void OnEvent(RequestDiscoverEvent eventData)
        {
            _isDiscoverMode = true;
            ShowCandidates(eventData.Candidates);
        }

        private void ShowCandidates()
        {
            if (ingredientPool == null || ingredientPool.Length == 0) return;

            ShowCandidates(ingredientPool);
        }

        private void ShowCandidates(IList<FoodIngredientData> candidates)
        {
            if (candidates == null || candidates.Count == 0) return;

            if(rootPanel != null) rootPanel.SetActive(true);

            // 레어도에 따른 재료 카드 출현 비율 (Common 60%, Rare 30%, Legendary 10%)
            int count = Mathf.Min(candidateCount, candidates.Count);

            var commons = ListPool<FoodIngredientData>.Get();
            var rares = ListPool<FoodIngredientData>.Get();
            var legendaries = ListPool<FoodIngredientData>.Get();

            foreach (var data in candidates)
            {
                if (data.rarity == Rarity.Common) commons.Add(data);
                else if (data.rarity == Rarity.Rare) rares.Add(data);
                else if (data.rarity == Rarity.Legendary) legendaries.Add(data);
                else commons.Add(data);
            }

            var selected = ListPool<FoodIngredientData>.Get();
            float totalWeight = commonWeight + rareWeight + legendaryWeight;

            for (int i = 0; i < count; i++)
            {
                float roll = Random.Range(0f, totalWeight);
                List<FoodIngredientData> targetPool = null;

                if (roll < commonWeight && commons.Count > 0) targetPool = commons;
                else if (roll < commonWeight + rareWeight && rares.Count > 0) targetPool = rares;
                else if (legendaries.Count > 0) targetPool = legendaries;

                // Fallback: 지정된 레어도의 카드가 소진된 경우 다른 레어도에서 가져옴
                if (targetPool == null)
                {
                    if (commons.Count > 0) targetPool = commons;
                    else if (rares.Count > 0) targetPool = rares;
                    else if (legendaries.Count > 0) targetPool = legendaries;
                }

                if (targetPool is { Count: > 0 })
                {
                    int idx = Random.Range(0, targetPool.Count);
                    selected.Add(targetPool[idx]);
                    
                    targetPool[idx] = targetPool[^1];
                    targetPool.RemoveAt(targetPool.Count - 1);
                }
            }

            for (int i = 0; i < selected.Count; i++)
            {
                _cards[i].Setup(selected[i], _isShowingAdvanced);
                _cards[i].OnSelected = OnIngredientSelected;
                _cards[i].gameObject.SetActive(true);
            }

            ListPool<FoodIngredientData>.Release(commons);
            ListPool<FoodIngredientData>.Release(rares);
            ListPool<FoodIngredientData>.Release(legendaries);
            ListPool<FoodIngredientData>.Release(selected);

            for (int i = count; i < candidateCount; i++)
            {
                _cards[i].gameObject.SetActive(false);
            }
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
            // 선택을 건너뛰는 경우 null을 전달합니다.
            OnIngredientSelected(null);
        }

        private void OnIngredientSelected(FoodIngredientData data)
        {
            if (_isDiscoverMode)
            {
                _isDiscoverMode = false;
                HideCandidates();
                EventBus<DiscoverItemSelectedEvent>.Publish(new DiscoverItemSelectedEvent
                {
                    SelectedData = data
                });
            }
            else
            {
                EventBus<IngredientSelectedEvent>.Publish(new IngredientSelectedEvent
                {
                    SelectedData = data
                });
            }
        }
    }
}
