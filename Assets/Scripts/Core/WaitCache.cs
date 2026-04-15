using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// WaitForSeconds 인스턴스를 캐싱하여 반복 생성에 의한 GC 할당을 방지합니다.
    /// </summary>
    public static class WaitCache
    {
        private static readonly Dictionary<float, WaitForSeconds> Cache = new();

        public static WaitForSeconds Seconds(float duration)
        {
            if (!Cache.TryGetValue(duration, out var wait))
            {
                wait = new WaitForSeconds(duration);
                Cache[duration] = wait;
            }
            return wait;
        }
    }
}
