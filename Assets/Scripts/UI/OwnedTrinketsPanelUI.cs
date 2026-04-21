using System.Collections.Generic;
using UnityEngine;
using Core;
using Interfaces;
using Gameplay.Systems;

namespace UI
{
    /// <summary>
    /// 보유 중인 장식품 목록을 화면에 표시하는 UI 패널입니다.
    /// 마우스 클릭/드래그를 통해 스크롤하려면 이 컴포넌트가 부착된 또는 부모의 
    /// ScrollRect(Vertical) 컴포넌트에 이 패널의 contentContainer를 Content로 연결해주세요.
    /// </summary>
    public class OwnedTrinketsPanelUI : MonoBehaviour, IEventListener<OwnedTrinketsUpdatedEvent>
    {
        [Header("UI References")]
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private OwnedTrinketCellUI cellPrefab;

        private readonly List<OwnedTrinketCellUI> _activeCells = new();

        private void OnEnable()
        {
            EventBus<OwnedTrinketsUpdatedEvent>.Subscribe(this);
            RefreshUI();
        }

        private void OnDisable()
        {
            EventBus<OwnedTrinketsUpdatedEvent>.Unsubscribe(this);
        }

        public void OnEvent(OwnedTrinketsUpdatedEvent evt)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (GameManager.Instance == null || GameManager.Instance.Context == null) return;
            var ctx = GameManager.Instance.Context;

            // Clear existing cells
            foreach (var cell in _activeCells)
            {
                if (cell != null) Destroy(cell.gameObject);
            }
            _activeCells.Clear();

            // Instantiate new cells based on TrinketCounts
            foreach (var (trinket, count) in ctx.TrinketCounts)
            {
                var cell = Instantiate(cellPrefab, contentContainer);
                cell.Setup(trinket, count);
                cell.gameObject.SetActive(true);
                _activeCells.Add(cell);
            }
        }
    }
}
