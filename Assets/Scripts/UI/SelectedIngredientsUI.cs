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
    public class SelectedIngredientsUI : MonoBehaviour, IEventListener<IngredientSelectedEvent>
    {
        [Header("UI References")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private IngredientInfoUI ingredientInfoPrefab;
        [SerializeField] private Button showButton;
        [SerializeField] private Button backButton;

        private readonly List<IngredientInfoUI> _spawnedItems = new();

        private void Awake()
        {
            if (showButton != null)
            {
                showButton.onClick.AddListener(Show);
            }
            
            if (backButton != null)
            {
                backButton.onClick.AddListener(Hide);
            }
        }

        private void OnEnable()
        {
            EventBus<IngredientSelectedEvent>.Subscribe(this);
            RefreshUI();
        }

        private void OnDisable()
        {
            EventBus<IngredientSelectedEvent>.Unsubscribe(this);
        }
        
        public void OnEvent(IngredientSelectedEvent eventData)
        {
            if (eventData.SelectedData != null)
            {
                // 실시간으로 항목이 추가될 때, 순서 보장을 위해 전체를 다시 그립니다.
                // 굳이 AddIngredient로 맨 끝에 붙이지 않고 갱신합니다.
                RefreshUI();
            }
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
                if (backButton != null) backButton.gameObject.SetActive(true);
            }
            
            EventBus<GamePausedEvent>.Publish(new GamePausedEvent { IsPaused = true });
            
            RefreshUI();
        }

        private void Hide()
        {
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
            ui.Setup(data);
            
            // 단순 정보 표시용이므로 카드의 클릭 기능을 비활성화
            if (ui.TryGetComponent<Button>(out var btn))
            {
                btn.interactable = false; // 혹은 필요에 따라 true 유지
            }

            _spawnedItems.Add(ui);
        }
    }
}
