using System.Collections.Generic;
using UnityEngine;
using Core;
using Interfaces;
using Gameplay.Systems;

namespace UI
{
    /// <summary>
    /// 보유 중인 장식품 목록을 화면에 표시하는 UI 패널입니다.
    /// Object Pooling을 사용하여 매번 Instantiate/Destroy하지 않습니다.
    /// </summary>
    public class OwnedTrinketsPanelUI : MonoBehaviour, IEventListener<OwnedTrinketsUpdatedEvent>
    {
        [Header("UI References")]
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private OwnedTrinketCellUI cellPrefab;

        private readonly List<OwnedTrinketCellUI> _activeCells = new();
        private readonly List<OwnedTrinketCellUI> _pooledCells = new();

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

            // 활성 셀을 풀로 반환
            for (int i = _activeCells.Count - 1; i >= 0; i--)
            {
                var cell = _activeCells[i];
                cell.gameObject.SetActive(false);
                _pooledCells.Add(cell);
            }
            _activeCells.Clear();

            // TrinketCounts에 맞춰 셀 생성/재사용
            foreach (var (trinket, count) in ctx.TrinketCounts)
            {
                var cell = GetOrCreateCell();
                cell.Setup(trinket, count);
                cell.gameObject.SetActive(true);
                _activeCells.Add(cell);
            }
        }

        private OwnedTrinketCellUI GetOrCreateCell()
        {
            if (_pooledCells.Count > 0)
            {
                int lastIndex = _pooledCells.Count - 1;
                var cell = _pooledCells[lastIndex];
                _pooledCells.RemoveAt(lastIndex);
                return cell;
            }

            return Instantiate(cellPrefab, contentContainer);
        }
    }
}
