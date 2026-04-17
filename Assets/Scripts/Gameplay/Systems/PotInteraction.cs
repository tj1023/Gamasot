using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Core;
using Data;
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

        private Camera _mainCam;
        private Collider2D[] _overlapResults;
        private ContactFilter2D _ingredientFilter;
        private readonly List<RuntimeIngredient> _newHarvested = new();
        private readonly List<RuntimeIngredient> _potIngredients = new();

        private void Awake()
        {
            _mainCam = Camera.main;
            
            // GC 발생을 차단하기 위해, 배열을 생성하여 재사용
            // size = 한 번에 건지는 최대 수량 가정
            _overlapResults = new Collider2D[10];
            
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
        }

        private void Update()
        {
            if (potBoundary == null || Mouse.current == null) return;

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
            bool isScoopPhase = ctx != null && ctx.CurrentPhase == GamePhase.OnScoop && !ctx.IsPaused;

            HandleCursorVisual(isInsidePot && isScoopPhase, worldPos);

            // 솥 안에 방금 막 클릭이 발생했을 때
            if (isInsidePot && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // 게임 매니저(컨텍스트) 체크하여 건지기 허용 상태인지 판별
                if (ctx is { CurrentPhase: GamePhase.OnScoop, RemainScoopCount: > 0, IsPaused: false })
                {
                    ScoopIngredients(worldPos);
                }
            }
        }

        private void HandleCursorVisual(bool isInsidePot, Vector2 worldPos)
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

        private void ScoopIngredients(Vector2 scoopPos)
        {
            int count = Physics2D.OverlapCircle(scoopPos, scoopRadius, _ingredientFilter, _overlapResults);

            _newHarvested.Clear();

            for (int i = 0; i < count; i++)
            {
                IngredientEntity node = _overlapResults[i].GetComponent<IngredientEntity>();
                
                if (node != null && node.RuntimeData != null && node.gameObject.activeInHierarchy)
                {
                    _newHarvested.Add(node.RuntimeData);
                    ingredientManager.ReturnToPool(node);
                }
            }
            
            if (_newHarvested.Count > 0)
            {
                // [건질 때] 시너지 발동 처리
                ApplyScoopSynergies(_newHarvested);

                EventBus<ItemsHarvestedEvent>.Publish(new ItemsHarvestedEvent
                {
                    NewHarvestedItems = _newHarvested
                });
            }
        }

        private void ApplyScoopSynergies(List<RuntimeIngredient> newlyScooped)
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

            foreach (var ingredient in newlyScooped)
            {
                if (ingredient == null || ingredient.OriginalData == null) continue;

                scoopContext.Source = ingredient;

                foreach (var synergy in ingredient.ActiveSynergies)
                {
                    synergy?.EvaluateAndApply(scoopContext);
                }
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
