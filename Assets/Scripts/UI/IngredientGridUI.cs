using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Core;
using Data;
using Interfaces;

namespace UI
{
    /// <summary>
    /// Scoop 시 발생되는 이벤트를 수신하여 그리드 UI 요소들을 동적으로 생성하고 배치합니다.
    /// </summary>
    public class IngredientGridUI : MonoBehaviour, 
        IEventListener<ItemsHarvestedEvent>,
        IEventListener<PhaseChangedEvent>
    {
        [Header("UI References")]
        [SerializeField] private RectTransform container;
        [SerializeField] private IngredientCellUI cellPrefab;

        [Header("Animation Options")]
        [SerializeField, Tooltip("재료가 여러 개 건져질 때 각 재료가 UI에 보여지는 시간 간격 (초)")] 
        private float spawnDelay = 0.1f;

        private ObjectPool<IngredientCellUI> _pool;
        private readonly List<IngredientCellUI> _cells = new();

        private void Awake()
        {
            // Object Caching Pool 초기화
            _pool = new ObjectPool<IngredientCellUI>(
                createFunc: () => Instantiate(cellPrefab, container),
                actionOnGet: (cell) => cell.gameObject.SetActive(true),
                actionOnRelease: (cell) => 
                {
                    cell.Unbind();
                    cell.gameObject.SetActive(false);
                },
                actionOnDestroy: (cell) => Destroy(cell.gameObject),
                collectionCheck: false,
                defaultCapacity: 20,
                maxSize: 100
            );
        }

        private void OnEnable()
        {
            EventBus<ItemsHarvestedEvent>.Subscribe(this);
            EventBus<PhaseChangedEvent>.Subscribe(this);
        }

        private void OnDisable()
        {
            EventBus<ItemsHarvestedEvent>.Unsubscribe(this);
            EventBus<PhaseChangedEvent>.Unsubscribe(this);
        }

        public void OnEvent(PhaseChangedEvent eventData)
        {
            if (eventData.NewPhase == GamePhase.OnScoop)
            {
                ClearCells();
            }
        }

        public void OnEvent(ItemsHarvestedEvent eventData)
        {
            if (eventData.NewHarvestedItems == null || eventData.NewHarvestedItems.Count == 0) return;

            if (spawnDelay > 0f)
            {
                // 약간의 딜레이를 주면서 "하나씩 담기는" 연출
                StartCoroutine(SpawnRoutine(eventData.NewHarvestedItems));
            }
            else
            {
                // 즉시 생성
                foreach (var t in eventData.NewHarvestedItems)
                {
                    AddCell(t);
                }
            }
        }

        private IEnumerator SpawnRoutine(List<RuntimeIngredient> items)
        {
            // ListPool을 사용하여 GC 할당을 방지합니다.
            var itemsCopy = ListPool<RuntimeIngredient>.Get();
            itemsCopy.AddRange(items);

            try
            {
                foreach (var t in itemsCopy)
                {
                    AddCell(t);
                    yield return WaitCache.Seconds(spawnDelay);
                }
            }
            finally
            {
                // 코루틴이 종료되거나 도중에 중단되더라도 리스트를 확실하게 반환합니다.
                ListPool<RuntimeIngredient>.Release(itemsCopy);
            }
        }

        private void AddCell(RuntimeIngredient ingredient)
        {
            IngredientCellUI newCell = _pool.Get();
            newCell.transform.SetAsLastSibling();
            newCell.Bind(ingredient);
            _cells.Add(newCell);
        }

        /// <summary>
        /// 추후에 게임 리셋이나 정산이 완료되어 UI를 비워야 할 때 호출합니다.
        /// </summary>
        private void ClearCells()
        {
            for (int i = _cells.Count - 1; i >= 0; i--)
            {
                _pool.Release(_cells[i]);
            }
            _cells.Clear();
        }
    }
}
