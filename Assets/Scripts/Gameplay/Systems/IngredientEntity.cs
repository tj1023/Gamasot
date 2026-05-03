using System;
using System.Collections;
using UnityEngine;
using Core;
using Data;
using Interfaces;
using UI;

namespace Gameplay.Systems
{
    /// <summary>
    /// 솥 내부를 떠다니는 재료 오브젝트의 행동을 제어합니다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class IngredientEntity : MonoBehaviour, IEventListener<IngredientScoreChangedEvent>, IEventListener<PlayScoreTrailEvent>
    {
        [Header("Movement Settings")]
        [SerializeField] private float minSpeed = 1f;
        [SerializeField] private float maxSpeed = 3f;

        [Header("Rotation Settings")]
        [SerializeField] private float minRotationSpeed = 20f;
        [SerializeField] private float maxRotationSpeed = 120f;

        [Header("Animation Settings")]
        [SerializeField] private float popScale = 1.2f;
        [SerializeField] private float popDuration = 0.15f;
        
        [Header("Outline Settings")]
        [SerializeField] private Shader outlineShader;
        
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        
        // --- Outline ---
        private GameObject _outlineObj;
        private SpriteRenderer _outlineRenderer;
        private static Material _outlineMaterial;
        private const float OutlineThickness = 0.1f;

        // --- Animation ---
        private Coroutine _popRoutine;
        private Vector3 _defaultScale = Vector3.one;

        private Vector3 _potCenter;

        public RuntimeIngredient RuntimeData { get; private set; }
        
        // 풀 회수 콜백용 (선택적 사용)
        public Action<IngredientEntity> OnDestroyNode;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            _rb.gravityScale = 0f;
            _defaultScale = transform.localScale;
        }

        public void Initialize(FoodIngredientData data, Vector3 potCenter)
        {
            _potCenter = potCenter;

            // 인스턴스마다 독립적인 런타임 데이터 생성
            RuntimeData = new RuntimeIngredient(data);
            
            // 데이터 기반 비주얼 갱신 (예시: ScriptableObject의 스프라이트 복사)
            if (data.sprite != null)
                _spriteRenderer.sprite = data.sprite;
            
            // 시작 시 무작위 방향과 속도로 발진시킵니다.
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            float randomSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
            _rb.linearVelocity = randomDir * randomSpeed;

            // 자연스러운 초기 회전 속도 부여
            float randomRot = UnityEngine.Random.Range(minRotationSpeed, maxRotationSpeed);
            _rb.angularVelocity = UnityEngine.Random.value > 0.5f ? randomRot : -randomRot;

            if (ScoreUIRegistry.Instance != null)
            {
                ScoreUIRegistry.Instance.RegisterIngredient(RuntimeData, transform);
            }

            SetOutline(false, Color.white);
            transform.localScale = _defaultScale;

            // 이벤트 구독
            EventBus<IngredientScoreChangedEvent>.Subscribe(this);
            EventBus<PlayScoreTrailEvent>.Subscribe(this);
        }

        public void OnEvent(IngredientScoreChangedEvent eventData)
        {
            if (eventData.Ingredient == RuntimeData)
            {
                if (eventData.NewScore != eventData.OldScore && gameObject.activeInHierarchy)
                {
                    PlayPop();
                }
            }
        }

        public void OnEvent(PlayScoreTrailEvent eventData)
        {
            if (eventData.SourceIngredient == RuntimeData && gameObject.activeInHierarchy)
            {
                PlayPop();
            }
        }

        private void PlayPop()
        {
            if (_popRoutine != null)
            {
                StopCoroutine(_popRoutine);
            }
            _popRoutine = StartCoroutine(PopRoutine());
        }

        private IEnumerator PopRoutine()
        {
            float elapsed = 0f;
            Vector3 targetScale = _defaultScale * popScale;

            // 커지기
            while (elapsed < popDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (popDuration * 0.5f);
                transform.localScale = Vector3.Lerp(_defaultScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;
            // 작아지기
            while (elapsed < popDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (popDuration * 0.5f);
                transform.localScale = Vector3.Lerp(targetScale, _defaultScale, t);
                yield return null;
            }

            transform.localScale = _defaultScale;
            _popRoutine = null;
        }

        /// <summary>
        /// 하이라이트(아웃라인)를 켜고 끕니다. Sliced 모드와 스케일에 관계없이 일정한 두께를 유지합니다.
        /// </summary>
        public void SetOutline(bool show, Color color)
        {
            if (!show)
            {
                if (_outlineObj != null) _outlineObj.SetActive(false);
                return;
            }

            if (_outlineObj == null)
            {
                _outlineObj = new GameObject("Outline");
                _outlineObj.transform.SetParent(transform);
                
                _outlineRenderer = _outlineObj.AddComponent<SpriteRenderer>();
                
                if (_outlineMaterial == null && outlineShader != null)
                {
                    _outlineMaterial = new Material(outlineShader);
                }
                if (_outlineMaterial != null)
                    _outlineRenderer.material = _outlineMaterial;
            }

            _outlineObj.SetActive(true);
            _outlineObj.transform.localPosition = Vector3.zero;
            _outlineObj.transform.localRotation = Quaternion.identity;

            // 부모의 로컬 스케일에 반비례하게 설정하여 월드 스케일 1을 유지
            Vector3 parentScale = transform.localScale;
            float scaleX = Mathf.Abs(parentScale.x) > 0.001f ? parentScale.x : 1f;
            float scaleY = Mathf.Abs(parentScale.y) > 0.001f ? parentScale.y : 1f;
            _outlineObj.transform.localScale = new Vector3(1f / scaleX, 1f / scaleY, 1f);

            // SpriteRenderer 속성 동기화
            _outlineRenderer.sprite = _spriteRenderer.sprite;
            _outlineRenderer.drawMode = _spriteRenderer.drawMode;
            _outlineRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
            _outlineRenderer.sortingOrder = _spriteRenderer.sortingOrder - 1; // 부모 뒤에 렌더링
            _outlineRenderer.color = color;
            _outlineRenderer.size = new Vector2(1f + (OutlineThickness * 1f) / scaleX, 1f + (OutlineThickness * 1f) / scaleY);

        }

        /// <summary>
        /// 물리 연산 프레임마다 호출되어 속도를 일정 범위로 자가 교정합니다.
        /// 충돌로 인해 속력이 너무 줄거나 미쳐서 빨라지는 현상을 제어합니다.
        /// </summary>
        private void FixedUpdate()
        {
            var ctx = GameManager.Instance?.Context;
            if (ctx is { IsPaused: true })
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
                return;
            }

            if (ctx != null && ctx.TrinketModifiers.HasPuddingEffect)
            {
                Vector2 dir = (_potCenter - transform.position).normalized;
                _rb.AddForce(dir * 0.6f, ForceMode2D.Force);
            }

            float currentSpeed = _rb.linearVelocity.magnitude;
            
            // 속도가 0에 수렴하는 등 일정 수치 이하라면 강제로 최소 속도 부여
            if (currentSpeed < minSpeed)
            {
                // 방향성을 잃었을 경우를 방지
                Vector2 dir = currentSpeed > 0.01f ? _rb.linearVelocity.normalized : UnityEngine.Random.insideUnitCircle.normalized;
                _rb.linearVelocity = dir * minSpeed;
            }
            else if (currentSpeed > maxSpeed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
            }

            // 충돌 등으로 인해 회전이 멈추거나 너무 빨라지는 것을 방지하여 자연스러운 회전 유지
            float currentAngularSpeed = Mathf.Abs(_rb.angularVelocity);
            if (currentAngularSpeed < minRotationSpeed)
            {
                float sign = _rb.angularVelocity >= 0f ? 1f : -1f;
                _rb.angularVelocity = sign * minRotationSpeed;
            }
            else if (currentAngularSpeed > maxRotationSpeed)
            {
                float sign = _rb.angularVelocity >= 0f ? 1f : -1f;
                _rb.angularVelocity = sign * maxRotationSpeed;
            }
        }

        private void OnDisable()
        {
            // 이벤트 해제
            EventBus<IngredientScoreChangedEvent>.Unsubscribe(this);
            EventBus<PlayScoreTrailEvent>.Unsubscribe(this);

            if (_popRoutine != null)
            {
                StopCoroutine(_popRoutine);
                _popRoutine = null;
            }
            transform.localScale = _defaultScale;

            // 오브젝트가 비활성화될 때 초기화 처리
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;

            if (ScoreUIRegistry.Instance != null && RuntimeData != null)
            {
                ScoreUIRegistry.Instance.UnregisterIngredient(RuntimeData);
            }

            SetOutline(false, Color.white);
        }
        
        private void OnDestroy()
        {
            EventBus<IngredientScoreChangedEvent>.Unsubscribe(this);
            EventBus<PlayScoreTrailEvent>.Unsubscribe(this);
        }
    }
}