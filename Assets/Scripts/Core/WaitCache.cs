using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// WaitForSeconds 인스턴스를 캐싱하여 반복 생성에 의한 GC 할당을 방지합니다.
    /// 부동소수점 오차 방지를 위해 초 단위(float)를 밀리초 단위(int)로 변환해 해싱합니다.
    /// </summary>
    public static class WaitCache
    {
        private static readonly Dictionary<int, WaitForSeconds> Cache = new();

        public static WaitForSeconds Seconds(float duration)
        {
            int key = Mathf.RoundToInt(duration * 1000f);
            if (!Cache.TryGetValue(key, out var wait))
            {
                wait = new WaitForSeconds(duration);
                Cache[key] = wait;
            }
            return wait;
        }
    }
}
