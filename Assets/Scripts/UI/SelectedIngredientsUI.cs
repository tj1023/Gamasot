using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core;
using Data;
using Interfaces;

namespace UI
{
    /// <summary>
    /// 선택된 재료(SelectedIngredients) 목록을 가로 스크롤 뷰를 통해 보여주는 UI 클래스입니다.
    /// 유니티 기본 ScrollRect를 사용하여 마우스 클릭 및 드래그 스크롤링을 지원합니다.
    /// </summary>
    public class SelectedIngredientsUI : MonoBehaviour, 
        IEventListener<IngredientSelectedEvent>,
        IEventListener<RequestDeleteExcessEvent>
    {
        [Header("UI References")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private IngredientInfoUI ingredientInfoPrefab;
        [SerializeField] private Button showButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button showAdvancedToggleButton;
        [SerializeField] private GameObject advancedCheckmark;
        
        [Header("Delete Mode Settings")]
        [SerializeField] private Button deleteConfirmButton;
        [SerializeField] private TMPro.TextMeshProUGUI deleteStatusText;

        private readonly List<IngredientInfoUI> _spawnedItems = new();
        
        // 동일한 재료(Data)를 여러 개 가지고 있을 경우를 대비하여 UI 인스턴스 자체를 식별자로 사용
        private readonly Dictionary<IngredientInfoUI, FoodIngredientData> _selectedForDeletion = new();
        private bool _isDeleteMode;
        private int _requiredDeleteCount;
        private bool _isShowingAdvanced;

        private void Awake()
        {
            if (showButton != null) showButton.onClick.AddListener(Show);
            if (backButton != null) backButton.onClick.AddListener(Hide);
            if (deleteConfirmButton != null) deleteConfirmButton.onClick.AddListener(ConfirmDeletion);
            if (showAdvancedToggleButton != null) showAdvancedToggleButton.onClick.AddListener(ToggleAdvancedMode);
        }

        private void ToggleAdvancedMode()
        {
            _isShowingAdvanced = !_isShowingAdvanced;
            
            if (advancedCheckmark != null)
            {
                advancedCheckmark.SetActive(_isShowingAdvanced);
            }

            foreach (var item in _spawnedItems)
            {
                if (item != null) item.ToggleAdvancedMode(_isShowingAdvanced);
            }
        }

        private void OnEnable()
        {
            EventBus<IngredientSelectedEvent>.Subscribe(this);
            EventBus<RequestDeleteExcessEvent>.Subscribe(this);
            RefreshUI();
        }

        private void OnDisable()
        {
            EventBus<IngredientSelectedEvent>.Unsubscribe(this);
            EventBus<RequestDeleteExcessEvent>.Unsubscribe(this);
        }
        
        public void OnEvent(IngredientSelectedEvent eventData)
        {
            if (eventData.SelectedData != null)
            {
                RefreshUI();
            }
        }

        public void OnEvent(RequestDeleteExcessEvent eventData)
        {
            _isDeleteMode = true;
            _requiredDeleteCount = eventData.ExcessCount;
            _selectedForDeletion.Clear();
            
            UpdateDeleteUIState();
            Show();
        }

        private void Show()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }
            else
            {
                if (scrollRect != null) scrollRect.gameObject.SetActive(true);
            }

            UpdateDeleteUIState();
            
            EventBus<GamePausedEvent>.Publish(new GamePausedEvent { IsPaused = true });
            
            RefreshUI();
        }

        private void Hide()
        {
            if (_isDeleteMode) return; // 삭제 모드에서는 일반적인 방법으로 닫을 수 없음

            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
            else
            {
                if (scrollRect != null) scrollRect.gameObject.SetActive(false);
                if (backButton != null) backButton.gameObject.SetActive(false);
            }
            
            EventBus<GamePausedEvent>.Publish(new GamePausedEvent { IsPaused = false });
        }

        private void UpdateDeleteUIState()
        {
            if (backButton != null) backButton.gameObject.SetActive(!_isDeleteMode);
            if (deleteConfirmButton != null)
            {
                deleteConfirmButton.gameObject.SetActive(_isDeleteMode);
                deleteConfirmButton.interactable = _selectedForDeletion.Count == _requiredDeleteCount;
            }

            if (deleteStatusText != null)
            {
                deleteStatusText.gameObject.SetActive(_isDeleteMode);
                if (_isDeleteMode)
                {
                    deleteStatusText.text = $"버릴 재료 선택 ({_selectedForDeletion.Count}/{_requiredDeleteCount})";
                }
            }
        }

        private void ConfirmDeletion()
        {
            var context = Gameplay.Systems.GameManager.Instance?.Context;
            if (context != null)
            {
                foreach (var kvp in _selectedForDeletion)
                {
                    // List.Remove는 동일한 객체 참조 중 첫 번째 것을 삭제하므로 
                    // 동일한 재료가 여러 개 있을 때 한 개씩 정확히 삭제됩니다.
                    context.SelectedIngredients.Remove(kvp.Value);
                }
            }

            _isDeleteMode = false;
            _selectedForDeletion.Clear();
            
            EventBus<ExcessDeletedEvent>.Publish(new ExcessDeletedEvent());
            
            Hide();
        }

        /// <summary>
        /// Context에 저장된 현재 선택된 재료 리스트를 정렬하여 UI를 초기화합니다.
        /// </summary>
        private void RefreshUI()
        {
            // 기존 생성된 아이템 초기화
            foreach (var item in _spawnedItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _spawnedItems.Clear();

            // GameManager를 통해 데이터 접근
            var context = Gameplay.Systems.GameManager.Instance?.Context;
            if (context is { SelectedIngredients: not null })
            {
                // UI 표현을 위해 정렬: 희귀도 내림차순 -> 타입 오름차순 -> 이름 오름차순
                var sortedIngredients = context.SelectedIngredients
                    .OrderBy(data => data.rarity)
                    .ThenBy(data => data.type)
                    .ThenBy(data => data.ingredientName);

                foreach (var data in sortedIngredients)
                {
                    AddIngredient(data);
                }
            }
        }

        private void AddIngredient(FoodIngredientData data)
        {
            var ui = Instantiate(ingredientInfoPrefab, contentContainer);
            ui.Setup(data, _isShowingAdvanced);
            
            // 프리팹에 Button이 없을 수 있으므로 동적으로 추가
            if (!ui.TryGetComponent<Button>(out var btn))
            {
                btn = ui.gameObject.AddComponent<Button>();
            }
            
            // 유니티 Button 기본 Transition(ColorTint) 때문에 interactable=false 시 어두워지는 현상 방지
            btn.transition = Selectable.Transition.None;

            if (_isDeleteMode)
            {
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnCardClickedForDeletion(data, ui));
                UpdateCardVisual(ui, _selectedForDeletion.ContainsKey(ui));
            }
            else
            {
                btn.interactable = false; // 단순 정보 표시용
                UpdateCardVisual(ui, false); // 일반 모드일 때는 선택 효과 리셋
            }

            _spawnedItems.Add(ui);
        }

        private void OnCardClickedForDeletion(FoodIngredientData data, IngredientInfoUI ui)
        {
            if (_selectedForDeletion.ContainsKey(ui))
            {
                _selectedForDeletion.Remove(ui);
            }
            else
            {
                if (_selectedForDeletion.Count < _requiredDeleteCount)
                {
                    _selectedForDeletion.Add(ui, data);
                }
            }

            UpdateCardVisual(ui, _selectedForDeletion.ContainsKey(ui));
            UpdateDeleteUIState();
        }

        private void UpdateCardVisual(IngredientInfoUI ui, bool isSelected)
        {
            // 선택 시 크기를 줄이거나 알파값을 변경하여 시각적 피드백 제공
            var cg = ui.GetComponent<CanvasGroup>();
            if (cg == null) cg = ui.gameObject.AddComponent<CanvasGroup>();
            
            cg.alpha = isSelected ? 0.5f : 1f;
            ui.transform.localScale = isSelected ? Vector3.one * 0.9f : Vector3.one;
        }
    }
}
