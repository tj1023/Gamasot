using Data;

namespace Interfaces
{
    /// <summary>
    /// 시너지 발동 조건을 검사하는 인터페이스입니다.
    /// ScriptableObject 인스펙터 직렬화를 위해 [SerializeReference]를 지원합니다.
    /// </summary>
    public interface ITrigger
    {
        bool Evaluate(GameContext context);
    }
}