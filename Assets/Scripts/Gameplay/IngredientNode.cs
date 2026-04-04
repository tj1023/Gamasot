using System;
using Architecture;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// 솥 내부를 떠다니는 재료 오브젝트의 행동을 제어합니다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class IngredientNode : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float minSpeed = 1f;
        [SerializeField] private float maxSpeed = 3f;
        
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        
        public RuntimeIngredient RuntimeData { get; private set; }
        
        // 풀 회수 콜백용 (선택적 사용)
        public Action<IngredientNode> OnDestroyNode;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            _rb.gravityScale = 0f;
        }

        public void Initialize(FoodIngredientData data)
        {
            // 인스턴스마다 독립적인 런타임 데이터 생성
            RuntimeData = new RuntimeIngredient(data);
            
            // 데이터 기반 비주얼 갱신 (예시: ScriptableObject의 스프라이트 복사)
            if (data.sprite != null)
                _spriteRenderer.sprite = data.sprite;
            
            // 시작 시 무작위 방향과 속도로 발진시킵니다.
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            float randomSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
            _rb.linearVelocity = randomDir * randomSpeed;
        }

        /// <summary>
        /// 물리 연산 프레임마다 호출되어 속도를 일정 범위로 자가 교정합니다.
        /// 충돌로 인해 속력이 너무 줄거나 미쳐서 빨라지는 현상을 제어합니다.
        /// </summary>
        private void FixedUpdate()
        {
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
        }

        private void OnDisable()
        {
            // 오브젝트가 비활성화될 때 초기화 처리
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
    }
}
