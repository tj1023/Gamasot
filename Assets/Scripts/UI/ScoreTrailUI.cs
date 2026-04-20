using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Core;
using Interfaces;

namespace UI
{
    /// <summary>
    /// 점수 획득 및 시너지 발동 시 날아가는 Trail 효과를 관리합니다.
    /// TrailRenderer가 부착된 프리팹을 요구합니다.
    /// </summary>
    public class ScoreTrailUI : MonoBehaviour, IEventListener<PlayScoreTrailEvent>
    {
        [Header("Effect References")]
        [SerializeField] private GameObject trailPrefab;
        
        [Header("Settings")]
        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private ObjectPool<GameObject> _pool;
        
        // TrailRenderer 캐싱: GetComponentInChildren 반복 호출 방지
        private readonly Dictionary<GameObject, TrailRenderer> _trailCache = new();

        private void Awake()
        {
            _pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var obj = Instantiate(trailPrefab, transform);
                    // 생성 시점에 TrailRenderer를 캐싱
                    var tr = obj.GetComponentInChildren<TrailRenderer>();
                    if (tr != null) _trailCache[obj] = tr;
                    return obj;
                },
                actionOnGet: (obj) => {
                    obj.SetActive(true);
                    if (_trailCache.TryGetValue(obj, out var tr) && tr != null)
                        tr.Clear();
                },
                actionOnRelease: (obj) => {
                    if (_trailCache.TryGetValue(obj, out var tr) && tr != null)
                        tr.Clear();
                    obj.SetActive(false);
                },
                actionOnDestroy: (obj) => {
                    _trailCache.Remove(obj);
                    Destroy(obj);
                },
                collectionCheck: false,
                defaultCapacity: 20,
                maxSize: 100
            );
        }

        private void OnEnable() => EventBus<PlayScoreTrailEvent>.Subscribe(this);
        private void OnDisable() => EventBus<PlayScoreTrailEvent>.Unsubscribe(this);

        public void OnEvent(PlayScoreTrailEvent eventData)
        {
            if (ScoreUIRegistry.Instance == null || trailPrefab == null) return;

            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.zero;
            bool hasStart = false;
            bool hasEnd = false;

            // Source가 없고, 타겟이 점수 합산(TotalScore)이 아니라면 효과 X
            if (eventData.SourceIngredient == null && eventData.TargetType != EffectTargetType.TotalScore) return; 

            // 1. Target 좌표 구하기
            if (eventData.TargetType == EffectTargetType.Ingredient)
            {
                if (eventData.TargetIngredient != null)
                {
                    hasEnd = ScoreUIRegistry.Instance.TryGetIngredientPosition(eventData.TargetIngredient, out endPos);
                }
            }
            else if (eventData.TargetType == EffectTargetType.TotalScore)
            {
                hasEnd = ScoreUIRegistry.Instance.TryGetTotalScorePosition(out endPos);
            }

            // 2. Source 좌표 구하기
            if (eventData.SourceIngredient != null)
            {
                hasStart = ScoreUIRegistry.Instance.TryGetIngredientPosition(eventData.SourceIngredient, out startPos);
            }

            if (hasStart && hasEnd && startPos != endPos)
            {
                var trailObj = _pool.Get();
                StartCoroutine(MoveRoutine(trailObj, startPos, endPos, eventData.Duration, eventData.FixedDirection));
            }
        }

        private IEnumerator MoveRoutine(GameObject obj, Vector3 start, Vector3 end, float duration, int fixedDirection = 0)
        {
            obj.transform.position = start;
            
            // 캐싱된 TrailRenderer 사용
            _trailCache.TryGetValue(obj, out var tr);
            if (tr != null) tr.Clear();
            
            // 곡선 제어점(Control Point) 계산: 두 점 사이의 중간에서 수직(혹은 랜덤) 방향으로 튀어나온 점
            Vector3 center = (start + end) / 2f;
            Vector3 direction = (end - start).normalized;
            // 2D 평면(X, Y)에서 수직 벡터 구하기
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);
            
            // 거리에 비례해서 높이(곡선 정도) 설정 (랜덤성 부여)
            float distance = Vector3.Distance(start, end);
            float height = distance * Random.Range(0.2f, 0.5f);
            
            // 방향을 랜덤하게 해서 위로 또는 아래로 휘어지게
            if (fixedDirection == 0)
            {
                if (Random.value > 0.5f) height = -height;
            }
            else
            {
                height = Mathf.Abs(height) * fixedDirection;
            }            
            Vector3 controlPoint = center + perpendicular * height;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curveT = curve.Evaluate(t);
                
                // 2차 베지에 곡선 적용
                Vector3 m1 = Vector3.LerpUnclamped(start, controlPoint, curveT);
                Vector3 m2 = Vector3.LerpUnclamped(controlPoint, end, curveT);
                obj.transform.position = Vector3.LerpUnclamped(m1, m2, curveT);

                yield return null;
            }

            obj.transform.position = end;

            // 대기 시간: 프리팹의 TrailRenderer Trail Time 에 맞춰 꼬리가 사라질때까지 대기
            float waitTime = tr != null ? tr.time : 0.2f;
            yield return WaitCache.Seconds(waitTime);

            _pool.Release(obj);
        }
    }
}
