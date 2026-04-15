using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Systems
{
    /// <summary>
    /// 음식 재료(IngredientEntity)를 솥(PotBoundary) 안으로 생성하고 관리합니다.
    /// ObjectPool을 사용하여 GC 스파이크를 방지합니다.
    /// </summary>
    public class IngredientManager : MonoBehaviour, IEventListener<PhaseChangedEvent>
    {
        [Header("References")]
        [SerializeField] private PotBoundary potBoundary;
        [SerializeField] private IngredientEntity ingredientPrefab;
        
        [Header("Pool Settings")]
        [SerializeField] private int defaultCapacity = 20;
        [SerializeField] private int maxSize = 100;
        
        [Header("Test")]
        [SerializeField] private FoodIngredientData[] testIngredients;
        
        private ObjectPool<IngredientEntity> _ingredientPool;
        
        // 현재 솥(Pot)에 존재하는 활성화된 재료 노드 리스트
        public List<IngredientEntity> ActiveIngredients { get; private set; } = new();

        private void Awake()
        {
            InitializePool();
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
            if (eventData.NewPhase == GamePhase.OnScoop)
            {
                ClearAll();
                SpawnTest();
            }
        }

        private void ClearAll()
        {
            for (int i = ActiveIngredients.Count - 1; i >= 0; i--)
            {
                ReturnToPool(ActiveIngredients[i]);
            }
        }

        private void InitializePool()
        {
            _ingredientPool = new ObjectPool<IngredientEntity>(
                createFunc: () =>
                {
                    IngredientEntity node = Instantiate(ingredientPrefab, transform);
                    node.OnDestroyNode = ReturnToPool;
                    return node;
                },
                actionOnGet: (node) => 
                {
                    node.gameObject.SetActive(true);
                    ActiveIngredients.Add(node);
                },
                actionOnRelease: (node) => 
                {
                    // O(1) swap-and-remove: 순서 무관하므로 마지막 요소와 교체 후 제거
                    int index = ActiveIngredients.IndexOf(node);
                    if (index >= 0)
                    {
                        int lastIndex = ActiveIngredients.Count - 1;
                        ActiveIngredients[index] = ActiveIngredients[lastIndex];
                        ActiveIngredients.RemoveAt(lastIndex);
                    }
                    node.gameObject.SetActive(false);
                },
                actionOnDestroy: (node) => Destroy(node.gameObject),
                collectionCheck: false, // 성능을 위해 중복 릴리스 체크 비활성화
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        /// <summary>
        /// 풀에서 오브젝트를 꺼내와서 원형 공간 안에 무작위 위치로 스폰합니다.
        /// </summary>
        public IngredientEntity SpawnIngredient(FoodIngredientData data)
        {
            IngredientEntity node = _ingredientPool.Get();
            
            // 원형 경계의 가장자리에 끼이지 않도록 반지름의 80~90% 내에만 스폰 되게 처리.
            Vector2 randomPoint = Random.insideUnitCircle * (potBoundary.Radius * 0.8f);
            
            node.transform.position = potBoundary.transform.position + (Vector3)randomPoint;
            node.Initialize(data);
            
            return node;
        }

        /// <summary>
        /// 오브젝트를 삭제하지 않고 풀에 반환합니다.
        /// </summary>
        public void ReturnToPool(IngredientEntity node)
        {
            _ingredientPool.Release(node);
        }

        // 테스트용 유틸리티 (에디터 내에서 호출 테스트용)
        [ContextMenu("Spawn Dummy Ingredient")]
        private void SpawnTest()
        {
            foreach (var t in testIngredients)
                SpawnIngredient(t);
        }
    }
}
