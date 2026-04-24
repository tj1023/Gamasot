using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Core;
using Data;
using Interfaces;
using UnityEngine.Pool;

// FoodIngredientData 및 기타 컨텍스트 연동을 위함

namespace Gameplay.Systems
{
    /// <summary>
    /// 마우스가 솥 공간 안에 있을 때의 시각적 피드백과,
    /// 클릭 시 해당 범위의 재료를 건져내는 상호작용을 담당합니다.
    /// </summary>
    public class PotInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PotBoundary potBoundary;
        [SerializeField] private IngredientManager ingredientManager;
        
        [Header("Scooping Settings")]
        [SerializeField] private Transform cursorVisual;
        [SerializeField] private float scoopRadius = 2f;
        [SerializeField] private LayerMask ingredientLayer;
        [SerializeField] private ParticleSystem scoopParticlePrefab;

        private Camera _mainCam;
        private Collider2D[] _overlapResults;
        private ContactFilter2D _ingredientFilter;
        private readonly List<RuntimeIngredient> _newHarvested = new();
        private readonly List<RuntimeIngredient> _potIngredients = new();
        
        // --- Outline ---
        private readonly HashSet<IngredientEntity> _hoveredIngredients = new();
        private readonly HashSet<IngredientEntity> _currentFrameHovered = new();
        private readonly Color _outlineColor = new(0.5f, 1f, 0f, 1f);

        // --- Particle Pool ---
        private ObjectPool<ParticleSystem> _particlePool;

        private void Awake()
        {
            _mainCam = Camera.main;
            
            // GC 발생을 차단하기 위해, 배열을 생성하여 재사용
            // size = 넉넉하게 50으로 늘려서 누락 방지
            _overlapResults = new Collider2D[50];
            
            // 필터 초기화
            _ingredientFilter = new ContactFilter2D();
            _ingredientFilter.SetLayerMask(ingredientLayer);
            _ingredientFilter.useLayerMask = true;
            
            if (cursorVisual != null)
            {
                // visual scale을 radius 크기에 맞게 조절 (원형 스프라이트가 기본 1x1 단위라고 가정)
                cursorVisual.localScale = new Vector3(scoopRadius * 2f, scoopRadius * 2f, 1f);
                cursorVisual.gameObject.SetActive(false);
            }

            if (scoopParticlePrefab != null)
            {
                _particlePool = new ObjectPool<ParticleSystem>(
                    createFunc: () => Instantiate(scoopParticlePrefab, transform),
                    actionOnGet: (ps) => ps.gameObject.SetActive(true),
                    actionOnRelease: (ps) => ps.gameObject.SetActive(false),
                    actionOnDestroy: (ps) => Destroy(ps.gameObject),
                    defaultCapacity: 10,
                    maxSize: 50
                );
            }
        }

        private bool _isProcessingScoop;

        private void Update()
        {
            if (_isProcessingScoop || potBoundary == null || Mouse.current == null) return;

            Vector2 mousePos2D = Mouse.current.position.ReadValue();
            Vector3 mousePos = new Vector3(mousePos2D.x, mousePos2D.y, 0f)
            {
                // 2D 평면 투영을 위해 카메라 깊이값 활용
                z = -_mainCam.transform.position.z
            };
            Vector2 worldPos = _mainCam.ScreenToWorldPoint(mousePos);

            // 제곱근이 무겁다면 sqrMagnitude를 써도 되지만, 
            // 솥 중심과의 거리 단순 비교는 매 프레임 한 번뿐이므로 가볍고 직관적인 Distance를 사용
            float distToPotCenter = Vector2.Distance(worldPos, potBoundary.transform.position);
            bool isInsidePot = distToPotCenter <= potBoundary.Radius;

            var ctx = GameManager.Instance?.Context;
            bool isScoopPhase = ctx is { CurrentPhase: GamePhase.OnScoop, IsPaused: false };
            float currentRadius = scoopRadius * (ctx?.TrinketModifiers.ScoopRadiusModifier ?? 1f);

            HandleCursorVisual(isInsidePot && isScoopPhase, worldPos, currentRadius);

            // --- 아웃라인 처리 로직 ---
            if (isInsidePot && isScoopPhase)
            {
                UpdateHoverOutlines(worldPos, currentRadius);
            }
            else
            {
                ClearHoverOutlines();
            }

            // 솥 안에 방금 막 클릭이 발생했을 때
            if (isInsidePot && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // 게임 매니저(컨텍스트) 체크하여 건지기 허용 상태인지 판별
                if (ctx is { CurrentPhase: GamePhase.OnScoop, RemainScoopCount: > 0, IsPaused: false })
                {
                    ClearHoverOutlines(); // 스쿱 시작 시 아웃라인 해제
                    StartCoroutine(ScoopIngredientsAsync(worldPos, currentRadius, ctx));
                }
            }
        }

        private void UpdateHoverOutlines(Vector2 scoopPos, float currentRadius)
        {
            int count = Physics2D.OverlapCircle(scoopPos, currentRadius, _ingredientFilter, _overlapResults);
            
            _currentFrameHovered.Clear();
            
            for (int i = 0; i < count; i++)
            {
                IngredientEntity node = _overlapResults[i].GetComponent<IngredientEntity>();
                if (node != null && node.gameObject.activeInHierarchy)
                {
                    _currentFrameHovered.Add(node);
                    node.SetOutline(true, _outlineColor);
                    _hoveredIngredients.Add(node);
                }
            }

            // 이전에 hover 상태였지만 이번 프레임에서 벗어난 것들 처리
            _hoveredIngredients.RemoveWhere(node => 
            {
                if (!_currentFrameHovered.Contains(node))
                {
                    if (node != null && node.gameObject.activeInHierarchy)
                    {
                        node.SetOutline(false, _outlineColor);
                    }
                    return true; // remove from hashset
                }
                return false; // keep in hashset
            });
        }

        private void ClearHoverOutlines()
        {
            if (_hoveredIngredients.Count > 0)
            {
                foreach (var node in _hoveredIngredients)
                {
                    if (node != null && node.gameObject.activeInHierarchy)
                    {
                        node.SetOutline(false, _outlineColor);
                    }
                }
                _hoveredIngredients.Clear();
            }
        }

        private void HandleCursorVisual(bool isInsidePot, Vector2 worldPos, float currentRadius)
        {
            if (cursorVisual == null) return;

            if (isInsidePot)
            {
                if (!cursorVisual.gameObject.activeSelf)
                {
                    cursorVisual.gameObject.SetActive(true);
                    Cursor.visible = false; // 기본 마우스 커서 숨기기
                }
                cursorVisual.position = worldPos;
                cursorVisual.localScale = new Vector3(currentRadius * 2f, currentRadius * 2f, 1f);
            }
            else
            {
                if (cursorVisual.gameObject.activeSelf)
                {
                    cursorVisual.gameObject.SetActive(false);
                    Cursor.visible = true; // 기본 마우스 커서 복원
                }
            }
        }

        private IEnumerator ScoopIngredientsAsync(Vector2 scoopPos, float currentRadius, GameContext ctx)
        {
            _isProcessingScoop = true;
            ctx.IsPaused = true;
            HandleCursorVisual(false, scoopPos, currentRadius); // 숨기기

            int count = Physics2D.OverlapCircle(scoopPos, currentRadius, _ingredientFilter, _overlapResults);

            _newHarvested.Clear();
            List<IngredientEntity> overlappingNodes = new();

            for (int i = 0; i < count; i++)
            {
                IngredientEntity node = _overlapResults[i].GetComponent<IngredientEntity>();
                
                if (node != null && node.RuntimeData != null && node.gameObject.activeInHierarchy)
                {
                    _newHarvested.Add(node.RuntimeData);
                    overlappingNodes.Add(node);
                }
            }
            
            if (_newHarvested.Count > 0)
            {
                yield return StartCoroutine(PlayScoopAnimationAsync(overlappingNodes));

                yield return StartCoroutine(ApplyScoopSynergiesAsync(_newHarvested));

                // 연출 완료 후 UI로 수거
                foreach (var node in overlappingNodes)
                {
                    ingredientManager.ReturnToPool(node);
                }
            }

            EventBus<ItemsHarvestedEvent>.Publish(new ItemsHarvestedEvent
            {
                NewHarvestedItems = _newHarvested
            });

            ClearHoverOutlines();
            _isProcessingScoop = false;
            ctx.IsPaused = false;
            
            // 마지막 스쿱 후 정산 페이즈 등 다른 페이즈로 넘어가지 않은 경우에만 커서를 다시 표시
            if (ctx is { CurrentPhase: GamePhase.OnScoop })
            {
                if (Mouse.current != null)
                {
                    Vector2 mousePos2D = Mouse.current.position.ReadValue();
                    Vector3 mousePos = new Vector3(mousePos2D.x, mousePos2D.y, 0f)
                    {
                        z = -_mainCam.transform.position.z
                    };
                    Vector2 worldPos = _mainCam.ScreenToWorldPoint(mousePos);
                    float distToPotCenter = Vector2.Distance(worldPos, potBoundary.transform.position);
                    bool isInsidePot = distToPotCenter <= potBoundary.Radius;
                    
                    HandleCursorVisual(isInsidePot, worldPos, currentRadius);
                }
            }
            else
            {
                // 페이즈가 넘어갔다면 무조건 커서를 숨김
                HandleCursorVisual(false, scoopPos, currentRadius);
            }
        }

        private IEnumerator PlayScoopAnimationAsync(List<IngredientEntity> nodes)
        {
            foreach (var t in nodes)
            {
                if (t == null) continue;
                if (_particlePool == null) continue;
                
                var ps = _particlePool.Get();
                ps.transform.position = t.transform.position;
                ps.Play();
                StartCoroutine(ReleaseParticleRoutine(ps));
            }

            yield return null;
        }

        private IEnumerator ReleaseParticleRoutine(ParticleSystem ps)
        {
            if (ps == null) yield break;
            
            while (ps != null && ps.IsAlive(true))
            {
                yield return null;
            }
            
            if (ps != null && _particlePool != null)
            {
                _particlePool.Release(ps);
            }
        }

        private IEnumerator ApplyScoopSynergiesAsync(List<RuntimeIngredient> newlyScooped)
        {
            var scoopContext = GameManager.Instance.Context;

            // 현재 활성화된(솥 안에 있는) 재료들의 RuntimeData 추출
            _potIngredients.Clear();
            foreach (var node in ingredientManager.ActiveIngredients)
            {
                if (node != null && node.RuntimeData != null)
                {
                    _potIngredients.Add(node.RuntimeData);
                }
            }

            scoopContext.SetLastScooped(newlyScooped);
            scoopContext.SetPotIngredients(_potIngredients);

            Queue<ICommand> commandQueue = new Queue<ICommand>();

            foreach (var ingredient in newlyScooped)
            {
                if (ingredient == null || ingredient.OriginalData == null) continue;

                scoopContext.Source = ingredient;

                foreach (var synergy in ingredient.ActiveSynergies)
                {
                    synergy?.EvaluateAndEnqueueCommands(scoopContext, commandQueue);
                }
            }

            while (commandQueue.Count > 0)
            {
                var command = commandQueue.Dequeue();
                yield return command.ExecuteAsync();
            }
        }

        private void OnDisable()
        {
            Cursor.visible = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Vector3 pos = Application.isPlaying && cursorVisual != null && cursorVisual.gameObject.activeSelf 
                ? cursorVisual.position 
                : transform.position;
            
            Gizmos.DrawWireSphere(pos, scoopRadius);
        }
    }
}
