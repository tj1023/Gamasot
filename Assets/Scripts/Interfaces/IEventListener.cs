namespace Interfaces
{
    /// <summary>
    /// 박싱 없는 EventBus를 위한 리스너 인터페이스.
    /// </summary>
    public interface IEventListener<in TEvent> where TEvent : struct
    {
        void OnEvent(TEvent eventData);
    }
}
