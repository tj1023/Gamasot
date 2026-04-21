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
            switch (eventData.NewPhase)
            {
                case GamePhase.OnTrinketSelection:
                case GamePhase.OnSelection:
                    ClearAll();
                    break;
                case GamePhase.OnScoop:
                    SpawnSelected();
                    break;
            }
        }

        /// <summary>
        /// GameContext.SelectedIngredients에 담긴 재료를 각각의 count만큼 스폰합니다.
        /// </summary>
        private void SpawnSelected()
        {
            var context = GameManager.Instance.Context;
            if (context.SelectedIngredients == null || context.SelectedIngredients.Count == 0) return;

            foreach (var data in context.SelectedIngredients)
            {
                if (data == null) continue;
                
                int spawnCount = Mathf.Max(1, data.count);
                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnIngredient(data);
                }
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
        public void SpawnIngredient(FoodIngredientData data)
        {
            IngredientEntity ingredient = _ingredientPool.Get();
            
            // 원형 경계의 가장자리에 끼이지 않도록 반지름의 80~90% 내에만 스폰 되게 처리.
            Vector2 randomPoint = Random.insideUnitCircle * (potBoundary.Radius * 0.8f);
            
            ingredient.transform.position = potBoundary.transform.position + (Vector3)randomPoint;
            ingredient.Initialize(data);
        }

        /// <summary>
        /// 오브젝트를 삭제하지 않고 풀에 반환합니다.
        /// </summary>
        public void ReturnToPool(IngredientEntity node)
        {
            _ingredientPool.Release(node);
        }

        public void TransformRandomToAdvanced(int count)
        {
            int transformed = 0;
            // 리스트를 셔플하거나 랜덤하게 접근
            var indices = ListPool<int>.Get();
            for (int i = 0; i < ActiveIngredients.Count; i++) indices.Add(i);

            for (int i = 0; i < indices.Count; i++)
            {
                int r = Random.Range(i, indices.Count);
                (indices[i], indices[r]) = (indices[r], indices[i]);
            }

            foreach (int index in indices)
            {
                var ingredient = ActiveIngredients[index];
                if (!ingredient.RuntimeData.IsAdvanced)
                {
                    ingredient.RuntimeData.TransformToAdvanced();
                    transformed++;
                    if (transformed >= count) break;
                }
            }

            ListPool<int>.Release(indices);
        }

        private void FixedUpdate()
        {
            var ctx = GameManager.Instance?.Context;
            if (ctx is { HasPuddingEffect: true, IsPaused: false })
            {
                Vector3 center = potBoundary.transform.position;
                float pullForce = 0.6f; // 중심축으로 당기는 힘 (필요시 조절)
                
                foreach (var ingredient in ActiveIngredients)
                {
                    if (ingredient.TryGetComponent<Rigidbody2D>(out var rb))
                    {
                        Vector2 dir = (center - ingredient.transform.position).normalized;
                        rb.AddForce(dir * pullForce, ForceMode2D.Force);
                    }
                }
            }
        }
    }
}
