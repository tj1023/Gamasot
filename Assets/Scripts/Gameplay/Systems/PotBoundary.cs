using UnityEngine;

namespace Gameplay.Systems
{
    /// <summary>
    /// 원형 경계를 생성하여 재료들이 밖으로 나가지 못하게 합니다.
    /// EdgeCollider2D를 사용하여 물리 연산 부하를 최소화합니다.
    /// </summary>
    [RequireComponent(typeof(EdgeCollider2D))]
    public class PotBoundary : MonoBehaviour
    {
        [Header("Boundary Settings")]
        [SerializeField] private float radius = 5f;
        [SerializeField] private int segments = 64;

        private EdgeCollider2D _edgeCollider;

        private void Awake()
        {
            _edgeCollider = GetComponent<EdgeCollider2D>();
            
            // PhysicsMaterial2D 셋팅 (Bounce: 1, Friction: 0 효과 부여를 위해)
            
            GenerateCircularBoundary();
        }

        /// <summary>
        /// 동적으로 다각형 정점을 생성해 완벽한 원형 경계 콜라이더를 만듭니다.
        /// </summary>
        private void GenerateCircularBoundary()
        {
            Vector2[] points = new Vector2[segments + 1];
            float angleStep = 360f / segments;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            _edgeCollider.points = points;
        }

        public float Radius => radius;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
