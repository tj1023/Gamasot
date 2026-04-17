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
    public class IngredientSelectionUI : MonoBehaviour, IEventListener<PhaseChangedEvent>
    {
        [Header("UI References")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private RectTransform container;
        [SerializeField] private IngredientInfoUI infoPrefab;
        [SerializeField] private GameObject darkBackground;
        [SerializeField] private Button skipButton;

        [Header("Selection Settings")]
        [SerializeField, Tooltip("한 번에 표시할 후보 재료 수")]
        private int candidateCount = 3;

        [Header("Data Source")]
        [SerializeField, Tooltip("전체 재료 후보 풀")]
        private FoodIngredientData[] ingredientPool;

        private IngredientInfoUI[] _cards;
        private bool _initialized;

        private void Awake()
        {
            InitializeCards();

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipButtonClicked);
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
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(this);
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
                    HideCandidates();
                    break;
            }
        }

        private void ShowCandidates()
        {
            if (ingredientPool == null || ingredientPool.Length == 0) return;

            if(rootPanel != null) rootPanel.SetActive(true);

            // Fisher-Yates 부분 셔플로 중복 없는 무작위 후보 선택 (GC 할당 최소화)
            int count = Mathf.Min(candidateCount, ingredientPool.Length);

            var indices = ListPool<int>.Get();
            for (int i = 0; i < ingredientPool.Length; i++)
                indices.Add(i);

            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(i, indices.Count);
                (indices[i], indices[randomIndex]) = (indices[randomIndex], indices[i]);
            }

            for (int i = 0; i < count; i++)
            {
                FoodIngredientData data = ingredientPool[indices[i]];
                _cards[i].Setup(data);
                _cards[i].OnSelected = OnIngredientSelected;
                _cards[i].gameObject.SetActive(true);
            }

            ListPool<int>.Release(indices);
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

        private static void OnSkipButtonClicked()
        {
            // 선택을 건너뛰는 경우 null을 전달합니다.
            OnIngredientSelected(null);
        }

        private static void OnIngredientSelected(FoodIngredientData data)
        {
            EventBus<IngredientSelectedEvent>.Publish(new IngredientSelectedEvent
            {
                SelectedData = data
            });
        }
    }
}
