using System.Collections.Generic;

namespace Architecture
{
    /// <summary>
    /// 싱글톤을 배제하고 GC 할당을 완전히 차단한 제네릭 기반 정적 이벤트 버스입니다.
    /// TEvent를 struct로 강제하여 박싱/언박싱 비용이 없습니다.
    /// Action/Delegate 대신 명시적인 인터페이스 구독을 활용해 클로저 할당 문제 역시 회피합니다.
    /// </summary>
    public static class EventBus<TEvent> where TEvent : struct
    {
        private static readonly List<IEventListener<TEvent>> Listeners = new(64);
        
        public static void Subscribe(IEventListener<TEvent> listener)
        {
            if (!Listeners.Contains(listener))
            {
                Listeners.Add(listener);
            }
        }

        public static void Unsubscribe(IEventListener<TEvent> listener)
        {
            Listeners.Remove(listener);
        }

        public static void Publish(TEvent eventData)
        {
            // 역순 탐색으로 OnEvent 실행 중 구독 해지(Unsubscribe)가 발생하더라도 OutOfRange 에러 회피
            for (int i = Listeners.Count - 1; i >= 0; i--)
            {
                if (Listeners[i] != null)
                {
                    Listeners[i].OnEvent(eventData);
                }
                else
                {
                    Listeners.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 씬 전환 시 등 명시적으로 초기화가 필요할 때 호출
        /// </summary>
        public static void Clear()
        {
            Listeners.Clear();
        }
    }
}
