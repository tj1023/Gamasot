using Data;

namespace Interfaces
{
    /// <summary>
    /// 시너지 조건 충족 시 발동될 효과 인터페이스입니다.
    /// 정산 시 파이프라인 정렬을 위해 우선순위(Priority)를 가지며 비동기 커맨드를 생성합니다.
    /// </summary>
    public interface IEffect
    {
        int Priority { get; }
        ICommand GenerateCommand(GameContext context, RuntimeIngredient source);
    }
}