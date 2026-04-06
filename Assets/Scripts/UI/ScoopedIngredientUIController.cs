using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using Architecture;
using Gameplay;

namespace UI
{
    /// <summary>
    /// Scoop 시 발생되는 이벤트를 수신하여 그리드 UI 요소들을 동적으로 생성하고 배치합니다.
    /// </summary>
    public class ScoopedIngredientUIController : MonoBehaviour, 
        IEventListener<ItemsHarvestedEvent>,
        IEventListener<PhaseChangedEvent>
    {
        [Header("UI References")]
        [SerializeField] private RectTransform gridContainer;
        [SerializeField] private ScoopedIngredientGridCellUI cellPrefab;

        [Header("Animation Options")]
        [SerializeField, Tooltip("재료가 여러 개 건져질 때 각 재료가 UI에 보여지는 시간 간격 (초)")] 
        private float spawnDelay = 0.1f;

        private ObjectPool<ScoopedIngredientGridCellUI> _cellPool;

        private void Awake()
        {
            // Object Caching Pool 초기화
            _cellPool = new ObjectPool<ScoopedIngredientGridCellUI>(
                createFunc: () => Instantiate(cellPrefab, gridContainer),
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
                ClearAllCells();
            }
        }

        public void OnEvent(ItemsHarvestedEvent eventData)
        {
            if (eventData.NewHarvestedItems == null || eventData.NewHarvestedItems.Count == 0) return;

            if (spawnDelay > 0f)
            {
                // 약간의 딜레이를 주면서 "하나씩 담기는" 연출
                StartCoroutine(SpawnCellsSequentially(eventData.NewHarvestedItems.ToArray()));
            }
            else
            {
                // 즉시 생성
                foreach (var t in eventData.NewHarvestedItems)
                {
                    AddGridCell(t);
                }
            }
        }

        private IEnumerator SpawnCellsSequentially(RuntimeIngredient[] items)
        {
            WaitForSeconds wait = new WaitForSeconds(spawnDelay);
            foreach (var t in items)
            {
                AddGridCell(t);
                yield return wait;
            }
        }

        private void AddGridCell(RuntimeIngredient ingredient)
        {
            ScoopedIngredientGridCellUI newCell = _cellPool.Get();
            newCell.transform.SetAsLastSibling(); // 그리드 끝에 쌓이도록 가장 아래로 보냄
            newCell.Bind(ingredient);
        }

        /// <summary>
        /// 추후에 게임 리셋이나 정산이 완료되어 UI를 비워야 할 때 호출합니다.
        /// </summary>
        private void ClearAllCells()
        {
            // gridContainer 하위에 활성화된 모든 cell들을 찾아 pool로 반환합니다.
            int childCount = gridContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = gridContainer.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    ScoopedIngredientGridCellUI cell = child.GetComponent<ScoopedIngredientGridCellUI>();
                    if (cell != null)
                    {
                        _cellPool.Release(cell);
                    }
                }
            }
        }
    }
}
